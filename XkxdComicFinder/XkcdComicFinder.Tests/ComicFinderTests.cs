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
        //create a different database since xUnit Tests run in parallel and tests accessing the same in-memory database is bad
        
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
        var responses = comics.ToDictionary(GetUri, c => JsonSerializer.Serialize(comics[0]));

        A.CallTo(fakeMsgHandler).WithReturnType<Task<HttpResponseMessage>>().Where(c => c.Method.Name == "Send Async")
        .Returns(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.NotFound, //default is 404 not found
        });

        foreach (var responsePair in responses)
        {
            A.CallTo(fakeMsgHandler).WithReturnType<Task<HttpResponseMessage>>().Where(c => c.Method.Name == "Send Async")
            .WhenArgumentsMatch(args => //get arguments list
            args.First() is HttpRequestMessage req //request is first arguments
            && req.RequestUri == responsePair.Key)
            .Returns(new HttpResponseMessage() 
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responsePair.Value), //returns comic
            });
        }
    }


}