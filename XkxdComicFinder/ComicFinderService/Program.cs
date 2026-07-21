using XkcdComicFinder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;

namespace ComicFinderService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var cfg = builder.Configuration; //base config comes from appsettings.json
        var connStr = cfg.GetConnectionString("Sqlite"); //connection strings have their own section
        var baseAddr = cfg.GetValue<string>("BaseAddr"); //makes the base address configurable

        services.AddScoped<IComicRepository, //scoped to request
        ComicRepository>(); //register interface's implementation

        //AddScoped indicates an object is scoped to a request 
        //aka the object will be created once per request and cleaned up after the request is complete
        //will not affect the perfomance of DbContexts as they use connection pooling
        services.AddScoped<IXkcdClient, XkcdClient>();
        services.AddScoped<ComicFinder>();
        services.AddControllers();
        services.AddDbContext<ComicDbContext>(option => option.UseSqlite(connStr));

        //using AddHttpClient before AddScoped will throw an error about an invalid URI
        services.AddHttpClient<IXkcdClient, XkcdClient>(client => client.BaseAddress = new Uri(baseAddr!)); 

        var app = builder.Build();
        app.MapControllers(); //map routes

        using var keepAliveConn = new SqliteConnection(connStr);
        keepAliveConn.Open();

        using (var scope = app.Services.CreateScope()) //DbContext is scoped
        {
            var dbCtxt= scope.ServiceProvider.GetRequiredService<ComicDbContext>(); //required means throw exception if not found
            dbCtxt.Database.EnsureCreated(); //creates database and applies schema

        }
        app.Run(); //runs until app is killed

    }
}
