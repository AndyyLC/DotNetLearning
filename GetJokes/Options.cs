using CommandLine;

namespace GetJokes;

public record Options
{
    [Value(0, Required = true, HelpText = "Input File")]
    public string? InputFile { get; init; }

    [Value(1, Required = false, HelpText = "Output File")]
    public string? OutputFile { get; init; }

    [Option('c', "category", Required = false, HelpText = "Joke Category")]
    public string? Category { get; init; } = "programming";
}
