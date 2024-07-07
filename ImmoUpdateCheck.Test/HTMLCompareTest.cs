using Microsoft.Extensions.Logging;

namespace ImmoUpdateCheck.Test
{
    public class HTMLCompareTest
    {
        [Theory]                                                        
        [InlineData("Same11.html", "Same12.html", "<a href=\\\"https://www.immowyss.ch", false)]
        [InlineData("Same21.html", "Same22.html", "", false)]
        [InlineData("Same31.html", "Same32.html", "", false)]
        [InlineData("Different11.html", "Different12.html", "", true)]
        public void CompareTest(string file1, string file2, string checkNode, bool expected)
        {
            string testFilesDir = "TestFiles";
            HtmlDocument doc1 = new HtmlDocument();
            HtmlDocument doc2 = new HtmlDocument();
            doc1.Load(Path.Combine(testFilesDir, file1, checkNode));
            doc2.Load(Path.Combine(testFilesDir, file2, checkNode));

            var result = HTMLCompare.Compare(doc1, doc2, checkNode);
            Assert.Equal(expected, result);
        }
    }
}