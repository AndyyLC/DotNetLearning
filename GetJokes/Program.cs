// See https://aka.ms/new-console-template for more information
using GetJokes;
using CommandLine;
using CommandLine.Text;
using System.Text;
using System.Text.Json;

var results = Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(
    options => {
        var inFile = new FileInfo(options.InputFile!);
        if (!inFile.Exists) //check if input file exists
        {
            Console.Error.WriteLine($"{inFile.FullName} does not exist");
            return;
        }
        using var inStream = inFile.OpenRead();
        FileInfo? outFile = null;
        if (options.OutputFile != null)
        {
            outFile = new FileInfo(options.OutputFile);
            if (outFile.Exists)
            {
                outFile.Delete();
            }
        }
        using var outStream = outFile != null ? outFile.OpenWrite() : Console.OpenStandardError(); //writes to console if there is no output file
        //FindWithJsonDom(inStream, outStream, options.Category);
        FindWithSerialization(inStream, outStream, options.Category);
    });

results.WithNotParsed(_ => WriteLine(HelpText.RenderUsageText(results)));

static void FindWithJsonDom(Stream inStream, Stream outStream, string category)
{
    var writerOptions = new JsonWriterOptions { Indented = true }; //options pattern, turning it off puts all text on one line
    
    //using makes the stream and writer dispose by default
    using var writer = new Utf8JsonWriter(outStream, writerOptions); //flushes stream but does not dispose of it
    writer.WriteStartArray(); //root element in array
    
    using var jsonDoc = JsonDocument.Parse(inStream);

    foreach (var jokeElement in jsonDoc.RootElement.EnumerateArray()) //treats the json as an array so we can loop through it
    {
        string? type = jokeElement.GetProperty("type").GetString();
        if (string.Equals(category, type, StringComparison.OrdinalIgnoreCase))
        {
            string? setup = jokeElement.GetProperty("setup").GetString();
            string? punchline = jokeElement.GetProperty("punchline").GetString();
            writer.WriteStartObject();
            writer.WriteString("setup", setup);
            writer.WriteString("punchline", punchline);
            writer.WriteEndObject();
            //jokeElement.WriteTo(writer);
        }
    }
    writer.WriteEndArray(); //indicates end of array
}

static void FindWithSerialization(Stream inStream, Stream outStream, string category)
{
    var serialOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };
    var jokes = JsonSerializer.Deserialize<Joke[]>(inStream, serialOptions); //deserializes JSON into an array of Joke Objects

    JsonSerializer.Serialize(
        outStream,
        jokes
            ?.Where(j => string.Equals(category, j.Type, StringComparison.OrdinalIgnoreCase))
            .Select(j => new { setup = j.Setup, punchline = j.Punchline }) //serializes jokes that match the category
            .ToArray(), //array needs a type
        serialOptions
    );
}
