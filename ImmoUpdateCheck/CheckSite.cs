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
        public string DumpName { get => Path.Combine(_lastContentPath, Name.Replace(" ", "_") + ".html"); }
        public string Url { get; set; } = url;
        public bool ContentChanged { get; set; } = false;
        private readonly string _lastContentPath = lastContentPath;

        public async Task CheckAsync(CancellationToken ct)
        {
            ContentChanged = false;
            var newHtml  = await GetContentAsync(ct);
            var oldHtml = GetLastContent();
            
            if (CompareSites(newHtml, oldHtml))
            {
                ContentChanged = true;
                newHtml.Save(DumpName);
            }
        }

        public bool CompareSites(HtmlDocument site1, HtmlDocument site2)
        {
            bool changed = false;
            if (NormalizeContent(site1.ParsedText) != NormalizeContent(site2.ParsedText))
            {
                changed = true;
            }
            return changed;
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

        private static string NormalizeContent(string content)
        {
            // Example normalization: Remove dynamic query parameters from URLs
            // This is a simplistic approach; you may need a more sophisticated method
            // depending on the structure and nature of the content.     
            if(string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }
            return Regex.Replace(content
                .Trim()
                .Replace(" ", "")
                .ToLower()                                
                .ReplaceLineEndings(), @"nocache=\d+", string.Empty);
        }
    }
}
