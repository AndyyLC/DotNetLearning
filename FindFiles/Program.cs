// See https://aka.ms/new-console-template for more information
using FindFiles;
using CommandLine;
using CommandLine.Text;

var results = Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(
    options => {RecursiveFind(new DirectoryInfo(options.RootFolder), options.Filter, options.Recursive, options.Bare);}
    );

results.WithNotParsed(_ => WriteLine(HelpText.RenderUsageText(results)));

static void RecursiveFind(
    DirectoryInfo folder,
    string filter,
    bool recurse,
    bool bare)
{
    if (!folder.Exists)
    {
        WriteLine($"{folder.FullName} does not exist");
        return;
    }
    if (!bare)
    {
        WriteLine(folder.FullName);
    }
    try
    {

        foreach (var file in folder.EnumerateFiles(filter))
        {
            if (bare)
            {
                WriteLine(folder.FullName + "\\" +file.Name);
            }
            else
            {
                WriteLine($"\t{file.Name}");
            }
        }
    }
    catch (System.Security.SecurityException)
    {
        return;
    }
    if (recurse)
    {
        foreach (var subFolder in folder.GetDirectories())
        {
            RecursiveFind(subFolder, filter, recurse, bare);
        }
    }
}
