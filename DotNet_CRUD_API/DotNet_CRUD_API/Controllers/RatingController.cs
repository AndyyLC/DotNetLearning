using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet_CRUD_API.Controllers;

[ApiController] //api controller
[Route("[controller]")] //uri route is controller name minus "Controller"
public class RatingController : ControllerBase
{
    const StringComparison SCIC = StringComparison.OrdinalIgnoreCase;
    private readonly CatalogContext _dbContext;

    public record RatingCreateCommand(int Stars, string? Comment) { }

    public record RatingUpdateCommand(int? Stars, string? Comment) { }

    public RatingController(CatalogContext dbContext) //dependency injection provides this object
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IAsyncEnumerable<Rating> GetRatings(
        string? titleFilter = null,
        int? skip = null,
        int? take = null,
        string? order = null
    )
    {
        IQueryable<Rating> query = _dbContext.Ratings.AsNoTracking(); //can't use var as query is composed in parts
        //AsNoTracking() means track no changes

        if (string.Equals("desc", order, SCIC))
        {
            query = query.OrderByDescending(r => r.Id);
        }
        else
        {
            query = query.OrderBy(r => r.Id);
        }

        return query.Skip(skip ?? 0).Take(take ?? 200).AsAsyncEnumerable(); //ASP.NET core responds with JSON by default
    }

    [HttpGet("{id}")]
    public Task<Rating?> GetRating(int id)
    {
        return _dbContext.Ratings.FirstOrDefaultAsync(r => r.Id == id); //returns task without await
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)] //tells swagger it can return these status codes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddRatingAsync(
        int id,
        RatingCreateCommand command,
        CancellationToken cancellationToken //ASP.NET auto interprets, and automatically adds the Http.RequestAborted token to this
    )
    {
        var book = await _dbContext.Books.Include(b => b.Ratings).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (book == null) {
            return NotFound();
        }
        var rating = new Rating(command.Stars, command.Comment);

        book.Ratings.Add(rating);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(book);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)] //tells swagger it can return these status codes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRatingAsync(
        int id,
        RatingUpdateCommand command,
        CancellationToken cancellationToken
    )
    //returns IActionResult to control status code
    {
        var rating = await _dbContext.FindAsync<Rating>( new object?[] {id}, cancellationToken); //tries tp find book in database
        if (rating == null)
        {
            return NotFound();
        }
        if (command.Stars != null)
        {
            rating.Stars = (int)command.Stars;
        }
        if (command.Comment != null)
        {
            rating.Comment = command.Comment;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        // return NoContent(); //indicate without returning object
        return Ok(rating); //returns updated Book object
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRatingAsync(int id, CancellationToken cancellationToken)
    {
        // var book = await _dbContext.FindAsync<Book>(id, cancellationToken); 
        // //version with cascade error ^

        var rating = await _dbContext.Ratings.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        //version that implements cascade delete properly ^

        if (rating == null)
        {
            return NotFound();
        }

        _dbContext.Remove(rating); 
        //EF Core will assume deleting a book with rating will also delete ratings (cascade delete),
        // but will cause an Error due to no translation for a SQLite delete statement
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
