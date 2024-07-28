using Microsoft.Extensions.Logging;

namespace ImmoUpdateCheck.Test
{
    public class HTMLCompareTest
    {
        [Theory]                                                        
        [InlineData("Same11.html", "Same12.html", "a", "href=\"https://www.immowyss.ch", false)]
        [InlineData("Same21.html", "Same22.html", "a", "href=\"https://www.immowyss.ch", false)]
        [InlineData("Same31.html", "Same32.html", "a", "href=\"/de/objects/detail", false)]
        [InlineData("Different11.html", "Different12.html", "a", "href=\"https://www.immowyss.ch", true)]
        [InlineData("Different21.html", "Different22.html", "a", "href=\"https://www.immowyss.ch", true)]
        [InlineData("Different31.html", "Different32.html", "a", "href=\"/de/objects/detail", true)]
        public void CompareTest(string file1, string file2, string nodeType, string nodeAttribute, bool expected)
        {
            string testFilesDir = "TestFiles";
            HtmlDocument doc1 = new();
            HtmlDocument doc2 = new();
            doc1.Load(Path.Combine(testFilesDir, file1));
            doc2.Load(Path.Combine(testFilesDir, file2));

            var result = HTMLCompare.Compare(doc1, doc2, nodeType, nodeAttribute);
            Assert.Equal(expected, result);
        }
    }
}