using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet_CRUD_API.Controllers;

[ApiController] //api controller
[Route("[controller]")] //uri route is controller name minus "Controller"
public class CatalogController : ControllerBase
{
    const StringComparison SCIC = StringComparison.OrdinalIgnoreCase;
    private readonly CatalogContext _dbContext;

    public record BookCreateCommand(string Title, string? Description) { }

    public record BookUpdateCommand(string Title, string? Description) { }

    public CatalogController(CatalogContext dbContext) //dependency injection provides this object
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IAsyncEnumerable<Book> GetBooks(
        string? titleFilter = null,
        int? skip = null,
        int? take = null,
        string? order = null
    )
    {
        IQueryable<Book> query = _dbContext.Books.Include(b => b.Ratings).AsNoTracking(); //can't use var as query is composed in parts
        //AsNoTracking() means track no changes
        if (titleFilter != null)
        {
            query = query.Where(b => b.Title.ToLower().Contains(titleFilter.ToLower()));
        }

        if (string.Equals("desc", order, SCIC))
        {
            query = query.OrderByDescending(b => b.Id);
        }
        else
        {
            query = query.OrderBy(b => b.Id);
        }

        return query.Skip(skip ?? 0).Take(take ?? 200).AsAsyncEnumerable(); //ASP.NET core responds with JSON by default
    }

    [HttpGet("{id}")]
    public Task<Book?> GetBook(int id)
    {
        return _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id); //returns task without await
    }

    [HttpPost]
    public async Task<Book> CreateBookAsync(
        BookCreateCommand command,
        CancellationToken cancellationToken //ASP.NET auto interprets, and automatically adds the Http.RequestAborted token to this
    )
    {
        var book = new Book(command.Title, command.Description);

        var entity = _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Entity;
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)] //tells swagger it can return these status codes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateBookAsync(
        int id,
        BookUpdateCommand command,
        CancellationToken cancellationToken
    )
    //returns IActionResult to control status code
    {
        var book = await _dbContext.FindAsync<Book>( new object?[] {id}, cancellationToken); //tries tp find book in database
        if (book == null)
        {
            return NotFound();
        }
        if (command.Title != null)
        {
            book.Title = command.Title;
        }
        if (command.Description != null)
        {
            book.Description = command.Description;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // return NoContent(); //indicate without returning object
        return Ok(book); //returns updated Book object
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBookAsync(int id, CancellationToken cancellationToken)
    {
        // var book = await _dbContext.FindAsync<Book>(id, cancellationToken); 
        // //version with cascade error ^
        
        var book = await _dbContext.Books.Include(b => b.Ratings).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        //version that implements cascade delete properly ^

        if (book == null)
        {
            return NotFound();
        }

        _dbContext.Remove(book); 
        //EF Core will assume deleting a book with rating will also delete ratings (cascade delete),
        // but will cause an Error due to no translation for a SQLite delete statement
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
