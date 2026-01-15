using CommandLine;

namespace CmdArgsTemplate;

public record Options
{
    [Value(0, Required = true, HelpText = "Some Text")]
    public string? Text {get; init;}
    
}