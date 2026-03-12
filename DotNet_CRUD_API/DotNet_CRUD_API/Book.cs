namespace DotNet_CRUD_API;
//code-first model
public class Book
{
    internal string title;

    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public List<Rating> Ratings { get; } = new();

    public Book(string title, string? description = null)
    {
        Title = title;
        Description = description;
    }
    // public Book(string title)
    // {
    //     Title = title;
    // }
}