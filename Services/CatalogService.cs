using Microsoft.Data.Sqlite;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SigmaLib.Services
{
    public class CatalogService
    {
        private readonly IDatabaseService _db;

        public CatalogService()
        {
            _db = new DatabaseService();
        }
        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();

            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT Id, Author, Year, Genre, Price, TotalCopies, AvailableCopies, Title, is_reservable
            FROM Books";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                books.Add(MapBook(reader));
            }
            return books;
        }

        //public List<Book> SearchBooks(string author = null, string title = null, int? year = null, string genre = null)
        //{
        //    var books = GetAllBooks();
        //    var query = books.AsQueryable();

        //    if (!string.IsNullOrEmpty(author))
        //        query = query.Where(b => b.Author.Contains(author));
        //    if (!string.IsNullOrEmpty(title))
        //        query = query.Where(b => b.Title.Contains(title));

        //    if (year.HasValue)
        //        query = query.Where(b => b.Year == year.Value);

        //    if (!string.IsNullOrEmpty(genre))
        //        query = query.Where(b => b.Genre.Contains(genre));

        //    return query.ToList();
        //    return books;
        //}
        public List<Book> SearchBooks(string author = null, string title = null, int? year = null, string genre = null)
        {
            var books = GetAllBooks();
            var query = books.AsQueryable();

            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => !string.IsNullOrEmpty(b.Author) &&
                                         b.Author.IndexOf(author, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => !string.IsNullOrEmpty(b.Title) &&
                                         b.Title.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0);

            if (year.HasValue)
                query = query.Where(b => b.Year == year.Value);

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => !string.IsNullOrEmpty(b.Genre) &&
                                         b.Genre.IndexOf(genre, StringComparison.OrdinalIgnoreCase) >= 0);

            return query.ToList();
        }
        private Book MapBook(SqliteDataReader reader)
        {
            return new Book
            {
                Id = reader.GetInt32(0),
                Author = reader.GetString(1),
                Year = reader.GetInt32(2),
                Genre = reader.GetString(3),
                Price = reader.GetDecimal(4),
                TotalCopies = reader.GetInt32(5),
                AvailableCopies = reader.GetInt32(6),
                Title = reader.GetString(7),
                IsReservable = reader.GetBoolean(8),
            };
        }

        private BookCopy MapBookCopy(SqliteDataReader reader)
        {
            return new BookCopy
            {
                Id = reader.GetInt32(0),
                IdBook = reader.GetInt32(1),
                Location = reader.GetString(2),
                Status = reader.GetString(3),
                Condition = reader.GetString(4)
            };
        }

        public void SetReservableStatusBook(Book book)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Books
            SET is_reservable = @isReservable WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@isReservable", book.IsReservable);
            cmd.Parameters.AddWithValue("@Id", book.Id);
            cmd.ExecuteNonQuery();
        }
        public OperationResult DeleteBook(int idBook)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var pragma = conn.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"SELECT TotalCopies, AvailableCopies 
                             FROM Books
                             WHERE Id = @BookId";
            checkCmd.Parameters.AddWithValue("@BookId", idBook);

            using var reader = checkCmd.ExecuteReader();
            if (reader.Read())
            {
                int total = reader.GetInt32(0);
                int available = reader.GetInt32(1);

                if (total != available)
                {
                    return new OperationResult
                    {
                        Message = "Нельзя удалить: книги ещё забронированы",
                        Success = false
                    };
                }
            }
            else
            {
                return new OperationResult
                {
                    Message = "Книга не найдена",
                    Success = false
                };
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM Books WHERE Id = @BookId";
            cmd.Parameters.AddWithValue("@BookId", idBook);
            cmd.ExecuteNonQuery();

            return new OperationResult { Message = "OK", Success = true };
        }

        public void AddBook(Book book, List<BookCopy> bookCopies)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Books (Title, Author, Year, Genre, Price, TotalCopies, is_reservable)
VALUES (@Title, @Author, @Year, @Genre, @Price, @TotalCopies, @is_reservable); SELECT last_insert_rowid();
";
            cmd.Parameters.AddWithValue("@Title", book.Title);
            cmd.Parameters.AddWithValue("@Author", book.Author);
            cmd.Parameters.AddWithValue("@Year", book.Year);
            cmd.Parameters.AddWithValue("@Genre", book.Genre);
            cmd.Parameters.AddWithValue("@Price", book.Price);
            cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
            cmd.Parameters.AddWithValue("@is_reservable", book.IsReservable);
            long bookId = (long)cmd.ExecuteScalar();

            foreach (var copy in bookCopies)
            {
                using var copyCmd = conn.CreateCommand();
                copyCmd.CommandText = @"
            INSERT INTO BookCopies (IdBook, Location, Status, Condition)
            VALUES (@BookId, @Location, @Status, @Condition);";

                copyCmd.Parameters.AddWithValue("@BookId", bookId);
                copyCmd.Parameters.AddWithValue("@Location", copy.Location);
                copyCmd.Parameters.AddWithValue("@Status", copy.Status);
                copyCmd.Parameters.AddWithValue("@Condition", copy.Condition);

                copyCmd.ExecuteNonQuery();
            }
            using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"UPDATE Books
    SET AvailableCopies = (SELECT COUNT(*) FROM BookCopies
    WHERE IdBook = @BookId AND Status = 'Available') WHERE Id = @BookId;";
            updateCmd.Parameters.AddWithValue("@BookId", bookId);
            updateCmd.ExecuteNonQuery();
        }

        public List<BookCopy> GetBookCopies(int idBook)
        {
            List<BookCopy> bookCopies = new List<BookCopy>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, IdBook, Location, Status, Condition
            FROM BookCopies WHERE IdBook = @IdBook;";
            cmd.Parameters.AddWithValue("@IdBook", idBook);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    bookCopies.Add(MapBookCopy(reader));
                }
            }
            return bookCopies;
        }

        public BookCopy GetBookCopyById(int idBookCopy)
        {
            BookCopy bookCopy = new BookCopy();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, IdBook, Location, Status, Condition
            FROM BookCopies WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@Id", idBookCopy);
            using var reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                bookCopy = MapBookCopy(reader);
            }
            return bookCopy;
        }

        public void EditCurrentBook(Book book, List<BookCopy> oldBookCopy, List<BookCopy> newBookCopy, List<int> deletedBookCopyIds)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var copy in oldBookCopy)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;
                    if (deletedBookCopyIds.Contains(copy.Id))
                    {
                        cmd.CommandText = @"DELETE FROM BookCopies 
                            WHERE Id = @Id AND IdBook = @IdBook;";
                    }
                    else
                    {
                        cmd.CommandText = @"UPDATE BookCopies 
                            SET Location = @Location,
                                Status   = @Status,
                                Condition= @Condition
                            WHERE Id = @Id AND IdBook = @IdBook;";

                        cmd.Parameters.AddWithValue("@Location", copy.Location);
                        cmd.Parameters.AddWithValue("@Status", copy.Status);
                        cmd.Parameters.AddWithValue("@Condition", copy.Condition);
                    }

                    cmd.Parameters.AddWithValue("@Id", copy.Id);
                    cmd.Parameters.AddWithValue("@IdBook", book.Id);

                    cmd.ExecuteNonQuery();
                }
                foreach (var copy in newBookCopy)
                {
                    using var insertCmd = conn.CreateCommand();
                    insertCmd.Transaction = transaction;
                    insertCmd.CommandText = @"
                    INSERT INTO BookCopies (IdBook, Location, Status, Condition)
                    VALUES (@IdBook, @Location, @Status, @Condition);";

                    insertCmd.Parameters.AddWithValue("@IdBook", book.Id);
                    insertCmd.Parameters.AddWithValue("@Location", copy.Location);
                    insertCmd.Parameters.AddWithValue("@Status", copy.Status);
                    insertCmd.Parameters.AddWithValue("@Condition", copy.Condition);
                    insertCmd.ExecuteNonQuery();
                }
                using var updateBookCmd = conn.CreateCommand();
                updateBookCmd.Transaction = transaction;
                updateBookCmd.CommandText = @"
    UPDATE Books SET TotalCopies = (SELECT COUNT(*) FROM BookCopies WHERE IdBook = @IdBook),
    AvailableCopies = (SELECT COUNT(*) FROM BookCopies WHERE IdBook = @IdBook AND Status = 'Available'),
    is_reservable = @isReservable, Price = @fine
    WHERE Id = @IdBook;";
                updateBookCmd.Parameters.AddWithValue("@IdBook", book.Id);
                updateBookCmd.Parameters.AddWithValue("@isReservable", book.IsReservable);
                updateBookCmd.Parameters.AddWithValue("@fine", book.Price);
                updateBookCmd.ExecuteNonQuery();
                transaction.Commit();
            }
        }
        public Book GetBookByBookCopyId(int bookCopyId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            Book book = new Book();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IdBook FROM BookCopies WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", bookCopyId);

            int bookId = Convert.ToInt32(cmd.ExecuteScalar());
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"SELECT Id, Author, Year, Genre, Price, TotalCopies, AvailableCopies, Title, is_reservable
            FROM Books WHERE Id = $id";
            cmd2.Parameters.AddWithValue("$id", bookId);
            using var reader = cmd2.ExecuteReader();
            if (reader.Read())
            {
                book = MapBook(reader);
            }
            return book;
        }
    }
}