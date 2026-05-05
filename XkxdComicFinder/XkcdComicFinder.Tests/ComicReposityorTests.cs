using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace XkcdComicFinder.Tests;

public class ComicRepositoryTests : IDisposable //xUnit will dispose of test object
{
    internal const string ConnStrTemplate = "Datasource={0}; mod=memory; cache=shared"; //used by other test classes
    private readonly ComicDbContext _comicDbContext;
    private readonly ComicRepository _comicRepo;
    private readonly SqliteConnection _keepAliveConn;

    private bool _disposed;

    ~ComicRepositoryTests() => Dispose(false);


    public ComicRepositoryTests()
    {
        (_comicDbContext, _keepAliveConn) = SetupSqlite("comics"); //gets value from tuple
        _comicRepo = new(_comicDbContext);
    }

    internal static (ComicDbContext, SqliteConnection) SetupSqlite(string dbName) //returns tuple
    {
        var connStr = string.Format(ConnStrTemplate, dbName); //creates connection string
        SqliteConnection keepAlive = new(connStr); //creates and opens keep alive connection
        keepAlive.Open();
 
        var optionsBuilder = new DbContextOptionsBuilder(); //Options built with OptionsBuilder
        optionsBuilder.UseSqlite(connStr); //set sqlite as database
        var options = optionsBuilder.Options; //property gets build options
        ComicDbContext dbContext = new(options);

        Assert.True(dbContext.Database.EnsureCreated()); //ensure database is new
        //EnureCreated returns false if database exists and has tables because it doesn't need to apply the schema
        return (dbContext, keepAlive); //returns tuple
    }

    public void Dispose() //cleans up DbContext and closes connection
    {
        _keepAliveConn.Close();
        _comicDbContext.Dispose();
    }

    //Dispose Pattern
    //protects against multople calls to dispose
    //handles freeing of managed and native resources, suprpresses finalization

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

}
