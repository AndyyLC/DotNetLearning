using Microsoft.EntityFrameworkCore;

namespace XkcdComicFinder;

public class ComicDbContext : DbContext
{
    public DbSet<Comic> Comics { get; set; } = null!; 
    //null! is like saying null for now since compiler will complain and think it has no value but EFCore will automatically populate DBSet
    //properties in DBContext classes
    public ComicDbContext(DbContextOptions options) : base(options) { } //only allows contructors with options
}