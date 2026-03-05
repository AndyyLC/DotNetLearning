using Microsoft.EntityFrameworkCore;

namespace DotNet_CRUD_API;

public class CatalogContext: DbContext //inherit from DbContext class
{

    public const string ConnectionString = "DataSource=manningsbooks; mode=memory cache=shared";
    public DbSet<Book> Books { get; set;} //adds books to entity model

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(ConnectionString);
    //use sqlite

    //protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseInMemoryDatabase("ManningBooks");
    //use in-memory provider
    public static void SeedBooks() //create and seed database instead of doing it in Program.cs
    {
        Console.WriteLine("Database created and seeded");
        using var dbContext = new CatalogContext();
        if (!dbContext.Database.EnsureCreated()) //checks if database was just created if database already existed then skip the rest of the method
        {
            Console.WriteLine("Database ");
            return;
        }
        dbContext.Add(new Book("How make a mod"));
        dbContext.Add(new Book("API Design Patterns"));
        var efBook = new Book("EF Core in Action 2nd Edition");
        efBook.Ratings.Add(new Rating { Comment = "Amazing"});
        efBook.Ratings.Add(new Rating { Stars = 4 });
        dbContext.Add(efBook);
        dbContext.SaveChanges();
    }

    public static async Task WriteBookToConsoleAsync(string title)
    {
        using var dbContext = new CatalogContext();
        var book = await dbContext.Books.Include(b => b.Ratings).FirstOrDefaultAsync(b => b.Title == title);
        if (book == null)
        {
            Console.WriteLine(@$"""{title}"" not found.");
        }
        else
        {
            Console.WriteLine(@$"Book ""{book.Title})"" has id {book.Id}");
            book.Ratings.ForEach(r => Console.WriteLine($"\t{r.Stars} stars: {r.Comment ?? "-blank-"}"));

        }
    }
}

