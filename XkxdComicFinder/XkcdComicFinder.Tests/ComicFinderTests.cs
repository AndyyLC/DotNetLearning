using FakeItEasy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace XkcdComicFinder.Tests;

public class ComicFinderTests: IDisposable
{
    private const string NumberLink = "https://xkcd.com/{0}/info.0.json";
    private const string LatestLink = "https://xkcd.com/info.0.json";
    private readonly ComicDbContext _comicDbContext; //lets test alter database
    private readonly SqliteConnection _keepAliveConn;
    private readonly HttpMessageHandler _fakeMsgHandler; //test can alter HTTP response
    private readonly ComicFinder _comicFinder;

    public ComicFinderTests()
    {
        (_comicDbContext, _keepAliveConn) = ComicRepositoryTests.SetupSqlite("comic_int"); 
        //create a different database since xUnit Tests run in parallel and tests accessing the same in-memory database is unsafe
        
        var comicRepo = new ComicRepository(_comicDbContext);

        _fakeMsgHandler = A.Fake<HttpMessageHandler>();
        var httpClient = XkcdClientTests.SetupHttpClient(_fakeMsgHandler);
        var xkcdClient = new XkcdClient(httpClient); //httpClient needed to creat xkcdClient

        _comicFinder = new ComicFinder(xkcdClient, comicRepo);
    }

    public void Dispose()
    {
        _keepAliveConn.Close();
        _comicDbContext.Dispose();
    }

     private static Uri GetUri(Comic c) => new(string.Format(NumberLink, c.Number)); //creates URL from Comic

     internal static void SetResponseComics(HttpMessageHandler fakeMsgHandler, params Comic[] comics) 
    {
        //uses LING helper to convert to dictionary
        var responses = comics.ToDictionary(GetUri, c => JsonSerializer.Serialize(c));
        responses.Add(new Uri(LatestLink), 
        JsonSerializer.Serialize(comics[0])); //assumes first in stack is the latest due to FakeItEasy applying fakes in a stack
        //likely added so we can test a response for calling the latest comic

        A.CallTo(fakeMsgHandler).WithReturnType<Task<HttpResponseMessage>>().Where(c => c.Method.Name == "SendAsync")
        .Returns(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.NotFound, //default is 404 not found
        });

        foreach (var responsePair in responses)
        {
            A.CallTo(fakeMsgHandler).WithReturnType<Task<HttpResponseMessage>>().Where(c => c.Method.Name == "SendAsync")
            .WhenArgumentsMatch(args => //get arguments list
            args.First() is HttpRequestMessage req 
            //checks if first arguement is a HttpRequestMessage and then creates a variable called req if there is with the values of args.First
            && req.RequestUri == responsePair.Key)
            .Returns(new HttpResponseMessage() 
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responsePair.Value), //returns comic
            });
        }
    }

    [Fact]
    public async Task StartWithEmptyRepo()
    {
        SetResponseComics(_fakeMsgHandler, 
            new Comic() { Number = 12, Title = "b"},
            new Comic() { Number = 1, Title = "a"},
            new Comic() { Number = 4, Title = "c"});

        var foundComics = (await _comicFinder.FindAsync("b")) //returns IAsyncEnumerable
        .ToBlockingEnumerable(); //converts to regualr IEnumerable

        Assert.Single(foundComics);
        Assert.Single(foundComics, c => c.Number == 12);
    }

}