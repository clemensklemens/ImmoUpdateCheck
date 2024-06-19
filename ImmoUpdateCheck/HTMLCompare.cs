using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ImmoUpdateCheck
{
    public static class HTMLCompare
    {
        /// <summary>
        /// Comapres two HTML documents and returns true if the content is different.
        /// </summary>
        /// <param name="site1"></param>
        /// <param name="site2"></param>
        /// <returns></returns>
        public static bool Compare(HtmlDocument site1, HtmlDocument site2)
        {
            bool different = false;
            if (NormalizeContent(site1.ParsedText) != NormalizeContent(site2.ParsedText))
            {
                different = true;
            }
            return different;
        }

        private static string NormalizeContent(string content)
        {
            // Example normalization: Remove dynamic query parameters from URLs
            // This is a simplistic approach; you may need a more sophisticated method
            // depending on the structure and nature of the content.     
            if (string.IsNullOrWhiteSpace(content))
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
