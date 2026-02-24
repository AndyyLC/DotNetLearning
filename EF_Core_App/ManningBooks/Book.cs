namespace ManningBooks;
//code-first model
public class Book
{
    internal string title;

    public int Id { get; set; }
    public string Title { get; set;}
    public List<Rating> Ratings { get; } = new();

    public Book() {}
    public Book(string title)
    {
        Title = title;
    }
}