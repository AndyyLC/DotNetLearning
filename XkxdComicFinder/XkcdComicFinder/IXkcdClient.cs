namespace XkcdComicFinder;

public interface IXcdClient
{
    Task<Comic> GetLatestAsync();
    Task<Comic?> GetNumberAsync(int number);
    
}