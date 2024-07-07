using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.ComponentModel;
using System.Globalization;

namespace ImmoUpdateCheck
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IConfiguration _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation($"Service started");
            while (!ct.IsCancellationRequested)
            {
                #region Configuration
                int intervallMin;
                try
                {
                    intervallMin = _config.GetValue<int>("IntervallMin");
                }
                catch
                {
                    intervallMin = 60; //default to 1 hour
                }

                string? filePath = _config.GetValue<string>("AgencyList");
                if(string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("No AgencyList defined in appsettings.json");
                    return;
                }

                string? lastContentPath = _config.GetValue<string>("LastContentPath");
                if(string.IsNullOrWhiteSpace(lastContentPath))
                {
                    _logger.LogError("No LastContentPath defined in appsettings.json");
                    return;
                }

                List<string> receivers = _config.GetSection("MailReceiver").GetChildren().Select(x => x.Value??string.Empty).ToList();
                if(receivers.All(x => string.IsNullOrEmpty(x)))
                {
                    _logger.LogError("No Receivers defined in appsettings.json");
                    return;
                }

                var mailSettings = _config.GetSection("MailSettings");
                if (mailSettings is null)
                {
                    _logger.LogError("No Mail settings defined in appsettings.json");
                    return;
                }
                string? smtpServer = mailSettings.GetValue<string>("SmtpServer");
                if(string.IsNullOrWhiteSpace(smtpServer))
                {
                    _logger.LogError("No SmtpServer defined in appsettings.json");
                    return;
                }

                string? smtpPassword = mailSettings.GetValue<string>("SmtpPassword");
                smtpPassword ??= string.Empty;

                int? smtpPort = mailSettings.GetValue<int>("SmtpPort");
                if(smtpPort is null)
                {
                    _logger.LogError("No SmtpPort defined in appsettings.json");
                    return;
                }

                string? smtpSender = mailSettings.GetValue<string>("SmtpSender");
                if(string.IsNullOrWhiteSpace(smtpSender))
                {
                    _logger.LogError("No SmtpSender defined in appsettings.json");
                    return;
                }
                _logger.LogInformation($"Settings successfully read. Timer is {intervallMin} min");
                #endregion

                #region mainpart                
                var fileIO = new FileIO(lastContentPath, _logger);
                var sitesTmp = await fileIO.LoadSiteCSVAsync(filePath, lastContentPath, ct);
                var sites = sitesTmp.Select(x => new CheckSite(x.name, x.url, lastContentPath, x.checkNode, _logger)).ToList();
                _logger.LogInformation($"Found {sites.Count} Websites to check");
                await fileIO.CleanSiteDumpsAsync(lastContentPath, sites.Select(x => x.DumpName), ct);

                ParallelOptions po = new()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1
                };
                try
                {
                    await Parallel.ForEachAsync(sites, po, async (site, ct) =>
                    {
                        ct.ThrowIfCancellationRequested();
                        await (site.CheckAsync(ct));
                    });
                }
                catch(Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError($"Error checking sites: {ex.Message}");
                }

                ct.ThrowIfCancellationRequested();
                var changedSites = sites.Where(x => x.ContentChanged).ToList();
                _logger.LogInformation($"{changedSites.Count() / sites.Count()} Websites changed");
                if (changedSites.Count > 0)
                {
                    string mailBody = "Following Websites have changed:" + Environment.NewLine;
                    mailBody += string.Join(Environment.NewLine, changedSites.Select(x => $"{x.Name}:  {x.Url}"));
                    //await Mailer.SendMailAsync(smtpSender, receivers, smtpServer, smtpPassword, smtpPort.Value, "New immobiles for sale :-)", mailBody, _logger, ct);
                    _logger.LogInformation($"{changedSites.Count} Websites changed. Mail sent");
                }
                #endregion
                await Task.Delay(intervallMin * 60 * 1000, ct);
            }
        }
    }
}
