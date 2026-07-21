using System.Net;
using System.Text.Json;
using ComicFinderService;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection; //for ServiceDescriptor

namespace XkcdComicFinder.Tests;

public class SearchControllerTests
{
    private const string BaseAddress = "https://xkcd.com";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpMessageHandler _fakeMsgHandler;

    public SearchControllerTests()
    {
        _fakeMsgHandler = A.Fake<HttpMessageHandler>();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Integration");
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor sd = services.First(
                    s => s.ServiceType == typeof(HttpClient)); //finds HttpClient
                    services.Remove(sd); //removed from dependency injection
                    services.AddHttpClient<IXkcdClient, XkcdClient>( //replaced by test version
                        h => h.BaseAddress = new Uri(BaseAddress))
                        .ConfigurePrimaryHttpMessageHandler(() => _fakeMsgHandler); // fake message handler 
            });
            
        });
    }

    [Fact]
    public async Task Found()
    {
        ComicFinderTests.SetResponseComics(
            _fakeMsgHandler, 
            new Comic() { Number = 12, Title = "b"}, //First is latest
            new Comic() { Number = 1, Title = "a"},
            new Comic() {Number = 4, Title = "c"});

        HttpClient client = _factory.CreateClient(); 
        var response = await client.GetAsync( //allows us to call our service
            "/search?searchText=b");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        var comics = JsonSerializer.Deserialize<Comic[]>(content); //IAsyncEnumerable is turned into a JSON array
        Assert.NotNull(comics);
        Assert.Single(comics);
        Assert.Single(comics, c => c.Number == 12);
    }
}