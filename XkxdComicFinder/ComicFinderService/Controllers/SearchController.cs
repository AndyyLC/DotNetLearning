using Microsoft.AspNetCore.Mvc;
using XkcdComicFinder;

namespace ComicFinderService.Controllers;

[ApiController]
[Route("[controller]")] //removes "controller" from url, web address is then http://localhost:<port>/search?searchText=<something>
public class SearchController : ControllerBase
{
    private readonly ComicFinder _comicFinder;

    public SearchController(ComicFinder comicFinder) => _comicFinder = comicFinder; //dependency injection

    [HttpGet]
    public Task<IAsyncEnumerable<Comic>> FindAsync(string searchText) => _comicFinder.FindAsync(searchText);
    //expects a searchText query
}