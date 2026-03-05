using DotNet_CRUD_API;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CatalogContext>();
builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();

using var keepAliveConnection = new SqliteConnection(CatalogContext.ConnectionString);
keepAliveConnection.Open();

CatalogContext.SeedBooks();

app.MapGet("/", () => "Hello World!");

app.Run();
