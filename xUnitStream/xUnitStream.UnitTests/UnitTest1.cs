

using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using xUnitStreamLib;

namespace xUnitStream.UnitTests;

public class UnitTest1
{
    [Fact]
    public void MemoryStream()
    {
        using MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.WriteLine("abc");
        writer.WriteLine("def");
        writer.WriteLine("ghi");
        writer.Flush();

        stream.Seek(0, SeekOrigin.Begin);

        var line = Utils.FindFirstMatchingLine(stream, "f");
        Assert.Equal("def", line);
    }

    [Fact]
    public void FileStream()
    {
        var file = new FileInfo("testdata.txt");
        using var stream = file.OpenRead(); //throw exception if txt file is not found
        stream.Position = 0;

        var line = Utils.FindFirstMatchingLine(stream, "ou");
        Assert.Equal("You", line);
    }
}
