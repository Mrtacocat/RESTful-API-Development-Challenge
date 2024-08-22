# RESTful API Development Challenge

Objective: Build a simple RESTful API for managing a small library system.
Tech Stack: Choose from Java, C#, Python or RubyRails for backend; PostgreSQL for the database.
Requirements:
- Implement basic CRUD operations for books (Create, Read, Update, Delete).
- Each book should have fields such as title, author, ISBN, published_date, and status (available/borrowed).
- When a book is borrowed it defaults to 1 week, so also a field borrowed_until should exist.
  Optional: one can reserve a book for 1 day (so would need a field or logic for this)
- Implement one or two additional endpoints (e.g., search by author or book title).
- Include basic validation (e.g., no two books should have the same ISBN unless they are same title, author, published_date).
- Implement fancy search: someone wants to know whether 3 different books (eg. a series) is available to borrow on a given date.
  Optional: return first date when all 3 are available.
- Write unit tests for the API endpoints.
Optional: Implement basic user authentication for borrowing books (this would introduce them to authentication concepts used in Rails).
