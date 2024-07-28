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
        public static bool Compare(HtmlDocument site1, HtmlDocument site2, string nodeType, string nodeAttribute, string nodeText = "")
        {
            bool different = false;
            if (string.IsNullOrWhiteSpace(nodeText))
            {
                if (CountNodes(site1, nodeType, nodeAttribute) != CountNodes(site2, nodeType, nodeAttribute))
                {
                    different = true;
                }
            }
            else
            {
                if(CountNodes(site1, nodeType, nodeAttribute, nodeText) != CountNodes(site2, nodeType, nodeAttribute, nodeText))
                {
                    different = true;
                }
            }
            return different;
        }

        private static int CountNodes(HtmlDocument document, string nodeType, string nodeAttribute)
        {
            string xpath = $"//{nodeType}['{nodeAttribute}']";
            var nodes = document.DocumentNode.SelectNodes(xpath);
            return nodes?.Count ?? 0;
        }

        private static int CountNodes(HtmlDocument document, string nodeType, string nodeAttribute, string nodeText)
        {
            string xpath = $"//{nodeType}['{nodeAttribute}' and contains(text(), '{nodeText}')]";
            var nodes = document.DocumentNode.SelectNodes(xpath);
            return nodes?.Count ?? 0;
        }
    }
}
