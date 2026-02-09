using System.Text.Json;
using System.Text.Json.Serialization;

namespace XkcdSearch;
//This creates Comic record object to deserialize the Json to.
//It also contains methods to fetch and deserialize Json from the xkcd API
public record Comic //creates record object to deserialize the Json to
{
    [JsonPropertyName("num")] //use JsonPropertyName when not pulling all properties
    public int Number { get; init; }

    [JsonPropertyName("safe_title")]
    public string? Title { get; init; }

    [JsonPropertyName("month")]
    public string? Month { get; init; }

    [JsonPropertyName("day")]
    public string? Day { get; init; }

    [JsonPropertyName("year")]
    public string? Year { get; init; }

    [JsonIgnore]
    public DateOnly Date => DateOnly.Parse($"{Year}-{Month}-{Day}"); //makes comparing easier in local form

    public static HttpClient client = new HttpClient()
    {
        BaseAddress = new Uri("https://xkcd.com"),
    };

    public static Comic? GetComic(int number)
    {
        try
        {
            var path = number == 0 ? "info.0.json" : $"{number}/info.0.json"; //if number equals 0 then var = "info.0.json"
            var stream = client.GetStreamAsync(path).Result;
            return JsonSerializer.Deserialize<Comic>(stream);
        }
        catch (AggregateException e) when (e.InnerException is HttpRequestException) 
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public static async Task<Comic?> GetComicAsync(int number, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) //cancellation token lets us cancel early if comic is found
        {
            return null;
        }
        try
        {
            var path = number == 0 ? "info.0.json" : $"{number}/info.0.json"; //if number equals 0 then var = "info.0.json"
            var stream = await client.GetStreamAsync(path, cancellationToken);
            return await JsonSerializer.DeserializeAsync<Comic>(
                stream,
                cancellationToken: cancellationToken
            );
        }
        // catch (AggregateException e) when (e.InnerException is HttpRequestException)
        // {
        //     return null;
        // }
        // catch (HttpRequestException)
        // {
        //     return null;
        // }
        // catch (TaskCanceledException)
        // {
        //     return null;
        // }
        catch (Exception ex) when (
            (ex is AggregateException && ex.InnerException is HttpRequestException)
                || ex is HttpRequestException
                || ex is TaskCanceledException
            ) //shortened version of catching exceptions
        {
            return null;
        }
    }
}
