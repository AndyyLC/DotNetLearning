using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet_CRUD_API.Controllers;

[ApiController] //api controller
[Route("[controller]")] //uri route is controller name minus "Controller"

public class CatalogController : ControllerBase
{
    private readonly CatalogContext _dbContext;

    public CatalogController(CatalogContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IAsyncEnumerable<Book> GetBooks(string? titleFilter = null)
    {
        IQueryable<Book> query = _dbContext.Books.Include(b => b.Ratings).AsNoTracking();
        //AsNoTracking() means track no changes
        if (titleFilter != null)
        {
            query = query.Where(b => b.Title.ToLower().Contains(titleFilter.ToLower()));
        }

        return query.AsAsyncEnumerable();
    }
}
