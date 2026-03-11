using DotNet_CRUD_API;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CatalogContext>(); //registers CatalogContext with dependency injection
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

using var keepAliveConnection = new SqliteConnection(CatalogContext.ConnectionString);
keepAliveConnection.Open();

CatalogContext.SeedBooks();

app.MapGet("/", () => "Hello World!");

app.Run();
