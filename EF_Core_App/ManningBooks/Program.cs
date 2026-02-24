// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using ManningBooks;
using Microsoft.Data.Sqlite;

using var keepAliveConnections = new SqliteConnection(CatalogContext.ConnectionString); //connection to SQLite in-memory database
//keepAliveConnections.Open(); //leaves connection open because sqlite in-memory database will delete without a connection

CatalogContext.SeedBooks();

var userRequests = new[]
{
    "How make a mod",
    ".NET in Action",
    "API Design Patterns",
    "EF Core in Action",
};

foreach (var userRequest in userRequests)
{
    await CatalogContext.WriteBookToConsoleAsync(userRequest); //each call creates and disposes Catalog context object
}

// synchronos version of ^^^ 
// var tasks = new List<Task>();
// foreach (var userRequest in userRequests)
// {
//     tasks.Add(CatalogContext.WriteBookToConsoleAsync(userRequest));
// }
// Task.WaitAll(tasks.ToArray());

using var dbContext = new CatalogContext(); 
dbContext.Database.EnsureCreated();  //creates database in SQLite



// Depreciated block, replaced by Book.SeedBooks method
// var efBook = new Book("EF Core in Action");
// efBook.Ratings.Add(new Rating { Comment = "Great" });
// efBook.Ratings.Add(new Rating { Stars = 4 });
// dbContext.Add(efBook);
// dbContext.SaveChanges();

// dbContext.Add(new Book { Title = ".Net in Action"});
// dbContext.Add(new Book { Title = ".Net in Action 2"});
// dbContext.Add(new Book { Title = ".Net"});
// dbContext.Add(new Book { Title = "jjk"});
// dbContext.Add(new Book { Title = "csm"});

// old versions of C# don't have this shorthand and in order to create a new object it would be
//Book book = new Book();
//book.Title = ".NET in Action";
//database.Add(book);
//shorthand for creating a multiple values is using (new Book(item1, item2, ...)), (new Book{item1, item2, ...}), or (new Book() {item1, item2, ...})

dbContext.SaveChanges(); //save changes to database

// various ways to display database contents
// foreach (var book in dbContext.Books.OrderBy(b => b.Id))
// {
//     Console.WriteLine($"\"{book.Title}\" has id {book.Id}");
// }


// foreach (var book in dbContext.Books.Include(b => b.Ratings))
// {
//     Console.WriteLine($"\"{book.Title}\" has id {book.Id}");

//     book.Ratings.ForEach(r => Console.WriteLine($"\t{r.Stars} stars: {r.Comment ?? "-blanks-"}"));
// }

// var efRatings = (from b in dbContext.Books where b.Title == "EF Core in Action" select b.Ratings).FirstOrDefault();
// efRatings?.ForEach(r => Console.WriteLine($"{r.Stars} stars: {r.Comment ?? "-blanks-"}"));
