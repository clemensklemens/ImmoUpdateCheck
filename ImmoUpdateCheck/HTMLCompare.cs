using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ImmoUpdateCheck
{
    public static class HTMLCompare
    {
        //to do compare counts of a given element
        /// <summary>
        /// Comapres two HTML documents and returns true if the content is different.
        /// </summary>
        /// <param name="site1"></param>
        /// <param name="site2"></param>
        /// <returns></returns>
        public static bool Compare(HtmlDocument site1, HtmlDocument site2, string compareNode)
        {
            bool different = false;
            if (string.IsNullOrWhiteSpace(compareNode))
            {
                if (NormalizeContent(RemoveUnwantedTags(site1).ParsedText) != NormalizeContent(RemoveUnwantedTags(site2).ParsedText))
                {
                    different = true;
                }
            }
            else
            {
                if(CountNodes(site1, compareNode) != CountNodes(site2, compareNode))
                {
                    different = true;
                }
            }
            return different;
        }

        private static HtmlDocument RemoveUnwantedTags(HtmlDocument document)
        {
            document.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style")
                    .ToList()
                    .ForEach(n => n.Remove());

            // Remove elements by class name, e.g., "jet"
            document.DocumentNode.Descendants()
                    .Where(n => n.GetAttributeValue("class", "").Contains("jet"))
                    .ToList()
                    .ForEach(n => n.Remove());
            return document;
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

        private static int CountNodes(HtmlDocument document, string compareNode)
        {
            string xpath = $"//*[contains(text(), '{compareNode}')]";
            var nodes = document.DocumentNode.SelectNodes(xpath);
            return nodes?.Count ?? 0;
        }
    }
}
