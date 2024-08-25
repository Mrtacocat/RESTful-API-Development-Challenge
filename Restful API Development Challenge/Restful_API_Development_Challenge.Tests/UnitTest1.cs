using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restful_API_Development_Challenge.Controllers;
using Restful_API_Development_Challenge.Data;
using Xunit;

namespace Restful_API_Development_Challenge.Tests
{
    public class UnitTest1
    {
        private readonly BooksController _controller;
        private readonly LibraryContext _context;

        public UnitTest1()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryContext(options);
            _controller = new BooksController(_context);

            // Seed the database with fresh test data
            _context.Book.AddRange(new List<Book>
            {
                new Book { title = "book 1", author = "author 1", isbn = "12345678910" },
                new Book { title = "book 2", author = "author 2", isbn = "10987654321" },
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetBooks_ReturnsAllBooks()
        {
            // Act
            var result = await _controller.GetBooks();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var returnValue = Assert.IsType<List<Book>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task PostBook_AddsNewBook()
        {
            // Arrange
            var newBook = new Book
            {
                title = "New Test Book",
                author = "New Author",
                isbn = "789",
                publishdate = System.DateTime.UtcNow,
                status = "available"
            };

            // Act
            var result = await _controller.PostBook(newBook);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Book>(actionResult.Value);
            Assert.Equal("New Test Book", returnValue.title);
            Assert.Equal(3, await _context.Book.CountAsync());  // Ensure the book count has increased
        }

        [Fact]
        public async Task PutBookStatus_UpdatesStatus()
        {
            // Arrange
            var updateRequest = new Book
            {
                status = "borrowed"
            };

            // Act
            var result = await _controller.PutBookStatus(1, updateRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedBook = await _context.Book.FindAsync(1);
            Assert.Equal("borrowed", updatedBook?.status);
        }
    }
}
