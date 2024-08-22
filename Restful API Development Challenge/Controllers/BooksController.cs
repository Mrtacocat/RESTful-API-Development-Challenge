using Microsoft.AspNetCore.Http;
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

        // CREATE / POST
        // api/Books
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            if (await _context.Book.AnyAsync(b => b.isbn == book.isbn &&
                                                   b.title == book.title &&
                                                   b.author == book.author &&
                                                   b.publishdate == book.publishdate))
            {
                return Conflict("A book with the same ISBN, title, author, and published date already exists.");
            }

            if (book.status == "borrowed" && book.borroweduntil == null)
            {
                book.borroweduntil = DateTime.UtcNow.AddDays(7); // Ensure UTC time
            }

            _context.Book.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBooks), new { id = book.bookid }, book);
        }

        // SELECT / GET
        // api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Book.ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.bookid)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;
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



        // DELETE
        // api/Books/{id}
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


    }
}
