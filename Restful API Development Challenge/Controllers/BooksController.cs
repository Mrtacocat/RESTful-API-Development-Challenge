using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restful_API_Development_Challenge.Data;

namespace Restful_API_Development_Challenge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a book.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     CREATE /Book
        ///     
        ///     Add existing books or add a new book, make sure ISBN is not already in use
        ///     Cannot use different ISBN on same book
        ///     
        ///     You dont need BookId
        ///     {
        ///         "title": "Harry Potter and the Chamber of Secrets",
        ///         "author": "J.K. Rowling",
        ///         "isbn": "9780747538401",
        ///         "publishdate": "1998-07-02",
        ///         "status": "borrowed"
        ///     }
        ///
        /// </remarks>
        // CREATE / POST
        // api/Book
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Book is null.");
            }

            book.publishdate = DateTime.SpecifyKind(book.publishdate, DateTimeKind.Utc);

            // Check if ISBN is unique based on the conditions
            if (!await IsIsbnUnique(book))
            {
                return Conflict("A book with the same ISBN already exists with different title, author, or published date.");
            }

            // Check for duplicate books based on title, author, and publish date
            if (await IsDuplicateBook(book))
            {
                return Conflict("A book with the same title, author, and publish date already exists with a different ISBN.");
            }

            // If the book status is borrowed add 7 days
            if (book.status == "borrowed" && book.borroweduntil == null)
            {
                book.borroweduntil = DateTime.UtcNow.AddDays(7);
            }

            _context.Book.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooks), new { id = book.bookid }, book);
        }

        /// <summary>
        /// Get all books.
        /// </summary>
        // SELECT / GET
        // api/Book
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Book.ToListAsync();
        }

        /// <summary>
        /// Update a book.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     UPDATE /Book
        ///     {
        ///         "bookid": 3,
        ///         "title": "Harry Potter and the Chamber of Secrets",
        ///         "author": "J.K. Rowling",
        ///         "isbn": "9780747538401",
        ///         "publishdate": "1998-07-02",
        ///         "status": "borrowed"
        ///     }
        ///
        /// </remarks>
        // UPDATE / PUT
        // api/Book/id
        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.bookid)
            {
                return BadRequest("ID in URL and ID in body do not match.");
            }

            // Check if ISBN is unique based on the conditions
            if (!await IsIsbnUnique(book))
            {
                return Conflict("A book with the same ISBN already exists with different title, author, or published date.");
            }

            // Check if the book exists
            var existingBook = await _context.Book.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Update the necessary fields
            existingBook.title = book.title;
            existingBook.author = book.author;
            existingBook.isbn = book.isbn;
            existingBook.publishdate = book.publishdate;
            existingBook.status = book.status;

            // Handle the borrowing logic
            if (book.status == "borrowed")
            {
                existingBook.borroweduntil = DateTime.UtcNow.AddDays(7);
            }
            else
            {
                existingBook.borroweduntil = null; // Clear BorrowedUntil if status is not "borrowed"
            }

            _context.Entry(existingBook).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }



        /// <summary>
        /// Update status of a book.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     UPDATE /Book
        ///     {
        ///         "title": "The Fellowship of the Ring",
        ///         "author": "J.R.R. Tolkien",
        ///         "isbn": "9780547928210",
        ///         "status": "borrowed"
        ///     }
        ///
        /// </remarks>
        // UPDATE / PUT status
        // api/Book/id
        //[Authorize]
        [HttpPut("status/{id}")]
        public async Task<IActionResult> PutBookStatus(int id, Book updateRequest)
        {
            // Validate input
            if (string.IsNullOrEmpty(updateRequest.status))
            {
                return BadRequest("Status is required.");
            }


            // Retrieve the existing book
            var existingBook = await _context.Book.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Update only the status
            existingBook.status = updateRequest.status;

            // Handle the borrowing logic
            if (updateRequest.status == "borrowed")
            {
                existingBook.borroweduntil = DateTime.UtcNow.AddDays(7);
            }
            else
            {
                existingBook.borroweduntil = null; // Clear BorrowedUntil if status is not "borrowed"
            }

            // Save changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
       

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.bookid == id);
        }



        /// <summary>
        /// Deletes a specific book.
        /// </summary>
        // DELETE
        // api/Book/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            _context.Book.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Search for a book
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     SEARCH /Book
        ///     Do not need full description
        ///     
        ///     EXAMPLE:
        ///     Title: Harry
        ///     Author: Rowling
        ///
        /// </remarks>
        // SELECT search title
        // api/Book/search?title=someTitle
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(string? title = null, string? author = null)
        {
            var query = _context.Book.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                // Have to make it to lower case so postgreSQL understand the search
                query = query.Where(b => EF.Functions.Like(b.title.ToLower(), $"%{title.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(author))
            {
                // Have to make it to lower case so postgreSQL understand the search
                query = query.Where(b => EF.Functions.Like(b.author.ToLower(), $"%{author.ToLower()}%"));
            }
            return await query.ToListAsync();
        }

        private async Task<bool> IsIsbnUnique(Book book)
        {
            // Check if there's any book with the same ISBN but different title, author, or published date
            return !await _context.Book.AnyAsync(b => b.isbn == book.isbn &&
                                                      (b.title != book.title ||
                                                       b.author != book.author ||
                                                       b.publishdate != book.publishdate));
        }

        private async Task<bool> IsDuplicateBook(Book book)
        {
            // Check if there's any book with the same title, author, and publish date but a different ISBN
            return await _context.Book.AnyAsync(b => b.title == book.title &&
                                                b.author == book.author &&
                                                b.publishdate == book.publishdate &&
                                                b.isbn != book.isbn);
        }


        // SELECT 3 different books and search for date
        // api/Books/availability?title1=Title1&title2=Title2&title3=Title3&date=2024-08-30
        [HttpGet("availability")]
        public async Task<ActionResult<DateTime?>> CheckBooksAvailability(string title1, string title2, string title3, DateTime date)
        {
            var titles = new List<string?> { title1.ToLower(), title2.ToLower(), title3.ToLower() }.Where(t => !string.IsNullOrEmpty(t)).ToList();
            DateTime utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            if (!titles.Any())
            {
                return BadRequest("No valid titles provided.");
            }


            // Tried to debug problem with searching but I have not found a solution
            var query = _context.Book
                .Where(b => titles.Contains(b.title.ToLower()) &&
                            (b.borroweduntil == null || b.borroweduntil < utcDate));

            // Debug logging for query
            Console.WriteLine("Generated Query: " + query.ToQueryString());

            var availableBooks = await query.ToListAsync();

            Console.WriteLine("Available Books Count: " + availableBooks.Count);

            if (availableBooks.Count > 0)
            {
                return Ok(new
                {
                    Titles = titles,
                    QueryDate = utcDate,
                    AvailableBooks = availableBooks.Select(b => new
                    {
                        b.title,
                        b.borroweduntil
                    })
                });
            }

            // Check for the next available date
            DateTime futureDate = utcDate;
            int maxDaysToCheck = 365; // Limit to 1 year
            int daysChecked = 0;

            while (daysChecked < maxDaysToCheck)
            {
                futureDate = futureDate.AddDays(1);
                daysChecked++;

                var futureQuery = _context.Book
                    .Where(b => titles.Contains(b.title.ToLower()) &&
                                (b.borroweduntil == null || b.borroweduntil < futureDate));

                var futureAvailableBooks = await futureQuery.ToListAsync();

                if (futureAvailableBooks.Select(b => b.title.ToLower()).Distinct().Count() >= 2)
                {
                    return Ok(futureDate);
                }
            }

            return NotFound("No availability found within the next year.");
        }
    }
}
