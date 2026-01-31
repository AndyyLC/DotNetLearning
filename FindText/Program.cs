// See https://aka.ms/new-console-template for more information
using FindText;
using CommandLine;
using CommandLine.Text;
using System.Security;
var results = Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(
    options =>
    {
        if (options.Filename != null) //if only a file name is specified then read it
        {
            SearchFile(new FileInfo(options.Filename), options.Text!); //creates object for file and tells the method thatthe text is not null
            //options.Text! basically says it looks like its nullable but its not null
        }
        else
        {
            string? filename;
            while (!string.IsNullOrWhiteSpace(filename = ReadLine())) //reads text until an empty line or null line(end of file)
            {
                SearchFile(new FileInfo(filename), options.Text!);
            }
        }
    });

results.WithNotParsed(_ => WriteLine(HelpText.RenderUsageText(results)));

static void SearchFile(FileInfo file, string text)
{
    if (!file.Exists)
    {
        Console.Error.WriteLine($"{file.FullName} does not exist");
        return;
    }
    try
    {
        using var reader = file.OpenText(); //this shorthand method of dispose gets rid of the disposable reader at end of code block
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                WriteLine(line);
            }
        }
    }
    catch (UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"(Unauthorizd: {file.FullName})");
    }
    catch (IOException)
    {
        Console.Error.WriteLine($"(IO Error: {file.FullName})");
    }
}