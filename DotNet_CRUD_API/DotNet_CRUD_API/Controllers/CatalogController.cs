using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet_CRUD_API.Controllers;

[ApiController] //api controller
[Route("[controller]")] //uri route is controller name minus "Controller"
public class CatalogController : ControllerBase
{
    const StringComparison SCIC = StringComparison.OrdinalIgnoreCase;
    private readonly CatalogContext _dbContext;

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
}
