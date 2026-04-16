namespace xUnitStreamLib;

public static class Utils
{
    public static string? FindFirstMatchingLine(Stream stream, string searchText)
    {
        var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                return line;
            }
        }

        return null;
    }
}
