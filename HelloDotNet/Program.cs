// See https://aka.ms/new-console-template for more information
using HelloDotnet;
using CommandLine;

Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(AsciiArt.Write).WithNotParsed(_ => WriteLine("Usage: HelloDotNet <text> --font Big"));
// if (args.Length == 0)
// {
//     Console.WriteLine("Usage: HelloDotNet <text>");
//     Environment.Exit(1);
// }
// AsciiArt.Write(args[0]);

