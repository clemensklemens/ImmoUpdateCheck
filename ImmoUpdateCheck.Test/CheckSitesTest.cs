namespace ImmoUpdateCheck.Test;

public class CheckSitesTest
{
    [Theory]
    [InlineData("Same11.html", "Same12.html", false)]
    [InlineData("Same21.html", "Same22.html", false)]
    [InlineData("Same31.html", "Same32.html", false)]
    [InLineData("Different11.html", "Different12.html", true)]
    public void CompareSitesTest(string file1, string file2, bool expected)
    {
        string testFilesDir = "TestFiles"; 
        var doc1 = new HtmlDocument().Load(Path.Combine(testFilesDir, file1));
        var doc2 = new HtmlDocument().Load(Path.Combine(testFilesDir, file2));

        var result = CheckSites.CompareSites(doc1, doc2);
        Assert.Equal(expected, result);
    }
}