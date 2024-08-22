namespace Restful_API_Development_Challenge.Data;

using Microsoft.EntityFrameworkCore;



public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Book { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the model if needed
        modelBuilder.Entity<Book>().ToTable("book");
    }
}

public class Book
{
    public int bookid { get; set; }
    public string title { get; set; }
    public string author { get; set; }
    public string isbn { get; set; }
    public DateTime publishdate { get; set; }
    public string status { get; set; }
    public DateTime? borroweduntil { get; set; }
}
