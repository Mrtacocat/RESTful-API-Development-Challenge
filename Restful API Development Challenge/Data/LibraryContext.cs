namespace Restful_API_Development_Challenge.Data;

using Microsoft.EntityFrameworkCore;



public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
}

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Status { get; set; }
    public DateTime? BorrrowedUntil { get; set; }
}
