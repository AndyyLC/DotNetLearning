using FakeItEasy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Net;
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
}