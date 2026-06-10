using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace XkcdComicFinder.Tests;

public class ComicRepositoryTests : IDisposable //xUnit will dispose of test object
{
    internal const string ConnStrTemplate = "Datasource={0}; mode=memory; cache=shared"; //used by other test classes
    private readonly ComicDbContext _comicDbContext;
    private readonly ComicRepository _comicRepo;
    private readonly SqliteConnection _keepAliveConn;

    public ComicRepositoryTests()
    {
        (_comicDbContext, _keepAliveConn) = SetupSqlite("comics"); //gets value from tuple
        _comicRepo = new(_comicDbContext);
    }

    internal static (ComicDbContext, SqliteConnection) SetupSqlite(string dbName) //returns tuple
    {
        var connStr = string.Format(ConnStrTemplate, dbName); //creates connection string by replace {0} with dbName
        SqliteConnection keepAlive = new(connStr); //creates and opens keep alive connection
        keepAlive.Open();
 
        var optionsBuilder = new DbContextOptionsBuilder(); //Options built with OptionsBuilder
        optionsBuilder.UseSqlite(connStr); //set sqlite as database
        var options = optionsBuilder.Options; //property gets build options
        ComicDbContext dbContext = new(options);

        Assert.True(dbContext.Database.EnsureCreated()); //ensure database is new
        //EnureCreated returns false if database exists and has tables because it doesn't need to apply the schema
        //if false then another test is using the same connection string and modifying the same database
        //multiple tests working on same database can cause whacky test results
        //xUnit creates new ComicRepositoryTests object fir each test and doesn't run test methods in the same class simultaneously
        //Because test class implements IDisposable, xUnit disposes the object after each test
        //By Asserting that EnsureCreated returns true, we make sure that no lingering database is being used, which would fail the test

        return (dbContext, keepAlive); //returns tuple
    }

    public void Dispose() //cleans up DbContext and closes connection
    {
        _keepAliveConn.Close();
        _comicDbContext.Dispose();
    }

    //Dispose Pattern
    //protects against multople calls to dispose
    //handles freeing of managed and native resources, suprpresses finalization;

    // private bool _disposed;

    // ~ComicRepositoryTests() => Dispose(false);

    // public void Dispose()
    // {
    //     Dispose(true);
    //     GC.SuppressFinalize(this); //tell GC not to finalize
    //     //GC.SuppressFinalize is only needed if a finalizer is used
    // }

    // private void Dispose(bool disposing)
    // {
    //     if (!_disposed)
    //     {
    //         if(disposing)
    //         {
    //             _keepAliveConn.Close();
    //             _comicDbContext.Dispose();
    //         }
    //         else
    //         {
    //             //some other cleaup
    //         }
    //     }
    //     _disposed = true;
    // }

    
    [Fact]
    public async Task NoComics_GetLatest() //When database is empty, GetLatestNumberAsync should be 0
    {
        var latest = await _comicRepo.GetLatestNumberAsync();
        Assert.Equal(0, latest);
    }
    [Fact]
    public async Task Comics_GetLatest() //If database contains list of comics, GetLatestNumberAsync should return highest number
    {
        _comicDbContext.AddRange(
            new Comic() {Number = 1},
            new Comic() {Number = 12},
            new Comic() {Number = 4}
        );
        await _comicDbContext.SaveChangesAsync();

        var latest = await _comicRepo.GetLatestNumberAsync();
        Assert.Equal(12, latest);
    }
    [Fact]
    public async Task Add() //Should add Comic to database
    {
        await _comicRepo.AddComicAsync(new Comic() {Number = 3});
        var addedComic = _comicDbContext.Find<Comic>(3);
        Assert.NotNull(addedComic);
    }

    [Fact]
    public async Task Found() //Should match record in database
    {
        _comicDbContext.AddRange(
            new Comic()
            {
                Number = 1,
                Title = "a",
                Alt = "Test Alt",
            },
            new Comic() { Number = 12, Title = "b" },
            new Comic() { Number = 4, Title = "c" }
        );
        await _comicDbContext.SaveChangesAsync();

        var foundComics = _comicRepo.Find("a").ToBlockingEnumerable(); //coverts to regular IEnumerable

        Assert.Single(foundComics);
        Assert.Single(foundComics, c => c.Alt == "Test Alt");
    }
}
