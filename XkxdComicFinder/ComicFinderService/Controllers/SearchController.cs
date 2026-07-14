
using Microsoft.AspNetCore.Mvc;
using XkcdComicFinder;

namespace ComicFinderService.Controllers;

[ApiController]
[Route("controller")]
public class SearchController : ControllerBase
{
    private readonly ComicFinderService _comicFinder;

    public SearchController(ComicsFinder comicFinder) => _comicFinder = ComicFinderService;

    [HttpGet]
    public Task<IAsyncEnumerable<Comic>> FindAsync(string searchText) => _comicFinder.FindAsync(searchText);
}