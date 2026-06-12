namespace XkcdComicFinder;

public class XkcdComicFinder
{
    private readonly IXkcdClient _xkcdClient;
    private readonly IComicRepository _repo;

    public ComicFinder(IXkcdClient XkcdClient, IComicRepository repo)
    {
        _xkcdClient = XkcdClient;
        _repo = repo;
    }

    public async Task<IAsyncEnumerable<Comic>> FindAsync(string searchText) //Task of IAsyncEnumerable
    {
        var latestComic = await _xkcdClient.GetLatestAsync(); //Always checks for the latest comic
        int latestInRepo = await _repo.GetLatestNumberAsync(); //will be 0 if repo is empty
        if (latestComic.Number > latestInRepo)
        {
            await FetchAsync(latestComic, latestInRepo);
        }
        return _repo.Find(searchText);
    }

    private async Task FetchAsync(Comic latestComic, int latestInRepo)
    {
        await _repo.AddComicAsync(latestComic); //add latest  to repo
        int current = latestComic.Number - 1;
        while (current > latestInRepo)
        {
            var comic = await _xkcdClient.GetByNumberAsync(current);
            if (comic != null) //if comic exists, adds to repo
            {
                await _repo.AddComicAsync(comic);
            }
            current--;
        }
    }
}