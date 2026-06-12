using System.Text.Json;
namespace XkcdComicFinder;

public class XkcdClient : IXkcdClient
{
    public const string PageUrl = "info.0.Json"; //every URL ends with info.0.json
    private readonly HttpClient _httpClient;

    public XkcdClient(HttpClient httpClient) => _httpClient = httpClient; //Caller passes Https Clients
    
    public async Task<Comic> GetLatestAsync()
    {
        var stream = await _httpClient.GetStreamAsync(PageUrl);
        return JsonSerializer.Deserialize<Comic>(stream)!;
    }

    public async Task<Comic?> GetNumberAsync(int number)
    {
        try
        {
            var path = $"{number}.{PageUrl}";
            var stream = await _httpClient.GetStreamAsync(path);
            return JsonSerializer.Deserialize<Comic>(stream);
        }
        catch (AggregateException e) when (e.InnerException is HttpRequestException) //exceptions when no caller exists
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
