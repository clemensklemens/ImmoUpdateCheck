using Microsoft.Extensions.Logging;

namespace ImmoUpdateCheck.Test
{
    public class HTMLCompareTest
    {
        [Theory]
        [InlineData("Same11.html", "Same12.html", false)]
        [InlineData("Same21.html", "Same22.html", false)]
        [InlineData("Same31.html", "Same32.html", false)]
        [InlineData("Different11.html", "Different12.html", true)]
        public void CompareTest(string file1, string file2, bool expected)
        {
            string testFilesDir = "TestFiles";
            HtmlDocument doc1 = new HtmlDocument();
            HtmlDocument doc2 = new HtmlDocument();
            doc1.Load(Path.Combine(testFilesDir, file1));
            doc2.Load(Path.Combine(testFilesDir, file2));

            var result = HTMLCompare.Compare(doc1, doc2);
            Assert.Equal(expected, result);
        }
    }
}