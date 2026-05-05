// See https://aka.ms/new-console-template for more information

//Program that finds the number a comic by name. This is done by asynchronously making HTTP requests to xkcd API to and then processing JSON files 
//that are returned from the API

using XkcdSearch;
using CommandLine;
using CommandLine.Text;
using System.Diagnostics;

var results = await Parser.Default.ParseArguments<Options>(args).WithParsedAsync<Options>(
    async options =>
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var comic = await GetComicWithTitleAsync(options.Title!);
        stopwatch.Stop();
        WriteLine("Result returned in " + $"{stopwatch.ElapsedMilliseconds} ms");
        if (comic == null)
        {
            WriteLine($"xkcd comic with title " + $"\"{options.Title}\" not found");
        }
        else
        {
            WriteLine($"xkcd \"{options.Title}\" is number " + $"{comic.Number} published on {comic.Date}");
        }
    });

results.WithNotParsed(_ => WriteLine(HelpText.RenderUsageText(results)));

//synchronous version
// static Comic? GetComicWithTitle(string title) 
// {
//     var lastComic = Comic.GetComic(0);
//     for (int number = lastComic!.Number; number > 0; number--) //fetchs comic starting from the last one
//     {
//         var comic = Comic.GetComic(number); //fetchs comic object from HTTP REQ so we can use it in the IF statement
//         if (comic != null && string.Equals(title, comic!.Title, StringComparison.OrdinalIgnoreCase))
//         {
//             return comic;
//         }
//     }
//     return null;
// }

//Makes asynchronous tasks to make fetch and derserialize Json from xkcd API
static async Task<Comic?> GetComicWithTitleAsync(string title)
{
    var cancellationToken = new CancellationTokenSource();
    var lastComic = await Comic.GetComicAsync(0, cancellationToken.Token);
    var tasks = new List<Task>();
    Comic? foundComic = null;
    for (int number = lastComic!.Number; number > 0; number--)
    {
        var localNumber = number; 
        //use a different variable (localNumber) as tasks will make reference to the original number variable in its endstate
        // leading to a bunch of calls to number 0

        var getComicTask = Comic.GetComicAsync(localNumber, cancellationToken.Token);
        var continuationTask = getComicTask.ContinueWith(t => 
        {
            //instead of having to synchronously wait before starting to check a new comic. we asynchronously make multiple calls in the time we would need to wait
            try //requests to make a task. tasks will act as a queue in this block of code
            {
                var comic = t.Result;
                if (comic != null && string.Equals(title, comic!.Title, StringComparison.OrdinalIgnoreCase))
                {
                    cancellationToken.Cancel();
                    foundComic = comic;
                }
            }
            catch (TaskCanceledException){} //if too many tasks are ongoing then fail this new task request
        });
        tasks.Add(continuationTask); //adds tasks to lists
    }

    await Task.WhenAll(tasks); //Creates a task that will complete when all of the Task objects in an enumerable collection have completed.
    return foundComic;
}