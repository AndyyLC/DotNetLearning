using System.Collections.Concurrent;

namespace XkcdComicFinder;

public class ComicFinder //can make multiple HTTP calls during search but each call had a different url
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
        // parallel version
        await _repo.AddComicAsync(latestComic); //add latest to repo
        var numsToFetch = Enumerable.Range(latestInRepo + 1, latestComic.Number - 1 - latestInRepo);

        var fetchedComics = new ConcurrentBag<Comic>();

        await Parallel.ForEachAsync(
            numsToFetch,
            new ParallelOptions { MaxDegreeOfParallelism = 4 },
            async (number, ct) =>
            {
                var comic = await _xkcdClient.GetByNumberAsync(number);
                if (comic != null)
                {
                    fetchedComics.Add(comic);
                }
            }
        );

        foreach (var comic in fetchedComics)
        {
            await _repo.AddComicAsync(comic);
        }

        // Non-parallel version
        // await _repo.AddComicAsync(latestComic); //add latest to repo
        // int current = latestComic.Number - 1;
        // while (current > latestInRepo)
        // {
        //     var comic = await _xkcdClient.GetByNumberAsync(current);
        //     if (comic != null) //if comic exists, adds to repo
        //     {
        //         await _repo.AddComicAsync(comic);
        //     }
        //     current--;
        // }
    }
}