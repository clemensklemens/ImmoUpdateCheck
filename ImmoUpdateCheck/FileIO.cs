using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using NLog.Layouts;

namespace ImmoUpdateCheck
{
    internal class FileIO(string lastContentPath, ILogger logger)
    {
        private string _lastContentPath = lastContentPath;       
        private readonly ILogger _logger = logger;

        internal async Task CleanSiteDumpsAsync(string dumpDir, IEnumerable<string> sites, CancellationToken ct)
        {
            List<string> dumpFiles = [];
            EnumerationOptions options = new()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true,
            };

            try
            {
                foreach (var file in Directory.EnumerateFiles(dumpDir).AsParallel())
                {
                    dumpFiles.Add(file);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dumpDir);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError($"Error reading dump directory: {ex.Message}");
            }

            var filesToDelete = dumpFiles.Except(sites);
            ParallelOptions po = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
                CancellationToken = ct
            };
            await Parallel.ForEachAsync(filesToDelete, po, async (file, ct) =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Run(() => File.Delete(file), ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError($"Error deleting file {file}: {ex.Message}");
                }
            });
        }

        public async Task<IEnumerable<(string name, string url, string checkNode)>> LoadSiteCSVAsync(string filePath, string dumpDir, CancellationToken ct)
        {
            var sites = new List<(string name, string url, string checkNode)>();
            try
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true
                };
                using var streamReader = File.OpenText(filePath);
                using var csvReader = new CsvReader(streamReader, csvConfig);

                var records = csvReader.GetRecordsAsync<CsvMapping>().WithCancellation(ct);
                await foreach (var record in records)
                {
                    ct.ThrowIfCancellationRequested();
                    sites.Add(new (record.AgencyName, record.CheckURL, record.CheckNode));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError($"Error loading CSV: {ex.Message}");
            }

            return sites;
        }

        private class CsvMapping
        {
            public string AgencyName { get; set; } = string.Empty;
            public string CheckURL { get; set; } = string.Empty;
            public string CheckNode { get; set; } = string.Empty;
        }
    }
}
