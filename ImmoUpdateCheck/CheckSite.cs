using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.RegularExpressions;

namespace ImmoUpdateCheck
{
    internal class CheckSite(string name, string url, string lastContentPath, ILogger logger)
    {
        private readonly ILogger _logger = logger;

        public string Name { get; set; } = name;
        public string DumpName { get => Path.Combine(_lastContentPath, Name.Replace(" ", "_")); }
        public string Url { get; set; } = url;
        public bool ContentChanged { get; set; } = false;
        private readonly string _lastContentPath = lastContentPath;

        public async Task CheckAsync(CancellationToken ct)
        {
            ContentChanged = false;
            Task<string> getContentTask = GetContentAsync(ct);
            Task<string> getLastContentTask = GetLastContentAsync(ct);
            List<Task<string>> tasks = [ getContentTask, getLastContentTask ];
            var results = await Task.WhenAll(tasks);

            string currentContentNormalized = NormalizeContent(results[0]);
            string lastContentNormalized = NormalizeContent(results[1]);
            if (currentContentNormalized != lastContentNormalized)
            {
                ContentChanged = true;
                await WriteLastResultAsync(results[0], ct);
            }
        }

        private async Task<string> GetContentAsync(CancellationToken ct)
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
            return htmlDoc.ParsedText ?? string.Empty;
        }

        private async Task<string> GetLastContentAsync(CancellationToken ct)
        {
            try
            {
                if (!System.IO.File.Exists(DumpName))
                {
                    return string.Empty;
                }

                return await System.IO.File.ReadAllTextAsync(DumpName, ct) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading last content for {Name}: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task WriteLastResultAsync(string newContent, CancellationToken ct)
        {
            try
            {
                await System.IO.File.WriteAllTextAsync(DumpName, newContent, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing last content for {Name}: {ex.Message}");
            }
        }

        private static string NormalizeContent(string content)
        {
            // Example normalization: Remove dynamic query parameters from URLs
            // This is a simplistic approach; you may need a more sophisticated method
            // depending on the structure and nature of the content.            
            return Regex.Replace(content
                .Trim()
                .Replace(" ", "")
                .ToLower()                                
                .ReplaceLineEndings(), @"nocache=\d+", string.Empty);
        }
    }
}
