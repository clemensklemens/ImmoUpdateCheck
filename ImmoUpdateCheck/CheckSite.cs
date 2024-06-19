using HtmlAgilityPack;
using System.Net;

namespace ImmoUpdateCheck
{
    public class CheckSite(string name, string url, string lastContentPath, ILogger logger)
    {
        private readonly ILogger _logger = logger;

        public string Name { get; set; } = name;
        public string DumpName { get => Path.Combine(_lastContentPath, Name.Replace(" ", "_") + ".html"); }
        public string Url { get; set; } = url;
        public bool ContentChanged { get; set; } = false;
        private readonly string _lastContentPath = lastContentPath;

        public async Task CheckAsync(CancellationToken ct)
        {
            ContentChanged = false;
            var newHtml  = await GetContentAsync(ct);
            var oldHtml = GetLastContent();
            
            if (HTMLCompare.Compare(newHtml, oldHtml))
            {
                ContentChanged = true;
                newHtml.Save(DumpName);
            }
        }

        private async Task<HtmlDocument> GetContentAsync(CancellationToken ct)
        {
            var web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
            web.UseCookies = true;
            web.PreRequest += request =>
            {
                request.CookieContainer = new CookieContainer();
                return true;
            };

            var htmlDoc = await web.LoadFromWebAsync(Url, ct);
            return htmlDoc;
        }

        private HtmlDocument GetLastContent()
        {
            var htmlDoc = new HtmlDocument();
            try
            {
                if (System.IO.File.Exists(DumpName))
                {                    
                    htmlDoc.Load(DumpName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading last content for {Name}: {ex.Message}");
            }
            return htmlDoc;
        }
    }
}
