namespace Restful_API_Development_Challenge.Data;

using Microsoft.EntityFrameworkCore;



#pragma warning disable CS1591
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
    public string title { get; set; } = string.Empty; // Have to default to empty to avoid null
    public string author { get; set; } = string.Empty;
    public string isbn { get; set; } = string.Empty;

    private DateTime _publishdate;
    public DateTime publishdate
    {
        get => _publishdate;
        set
        {
            // Have to enuse it converts to UTC
            _publishdate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }

    public string status { get; set; } = string.Empty;

    private DateTime? _borroweduntil;
    public DateTime? borroweduntil
    {
        get => _borroweduntil;
        set
        {
            if (value.HasValue)
            {
                // Have to enuse it converts to UTC
                _borroweduntil = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            }
            else
            {
                _borroweduntil = null;
            }
        }
    }
}

