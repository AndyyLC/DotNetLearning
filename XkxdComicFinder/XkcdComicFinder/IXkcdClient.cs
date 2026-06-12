namespace XkcdComicFinder;

public interface IXkcdClient
{
    Task<Comic> GetLatestAsync();
    Task<Comic?> GetNumberAsync(int number);
    
}