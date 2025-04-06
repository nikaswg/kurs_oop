using DataLayer; // Убедитесь, что это ваше пространство имен
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace Services
{
    public class LibraryService
    {
        private readonly EFDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public LibraryService(EFDbContext context)
        {
            _context = context;
        }

        public async Task<Book> GetBookByISBN(string ISBN)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == ISBN);
        }

        public BookModel GetBookByIsbn(string isbn)
        {
            var book = _context.Books
                .Include(b => b.Genres)
                .FirstOrDefault(b => b.ISBN == isbn);

            if (book == null) return null;

            return new BookModel
            {
                ISBN = book.ISBN ?? string.Empty,
                Book_Name = book.Book_Name ?? string.Empty,
                Book_Author = book.Book_Author ?? string.Empty,
                Book_Description = book.Book_Description ?? string.Empty,
                Publication_Date = book.Publication_Date,
                Number_Of_Pages = book.Number_Of_Pages,
                Image = book.Image ?? "/images/default-cover.jpg",
                Genres = book.Genres ?? new List<string>()
            };
        }

        public List<BookModel> GetAllBooks()
        {
            return _context.Books.Select(b => new BookModel
            {
                ISBN = b.ISBN ?? string.Empty,
                Book_Name = b.Book_Name ?? string.Empty,
                Book_Author = b.Book_Author ?? string.Empty,
                Book_Description = b.Book_Description ?? string.Empty,
                Publication_Date = b.Publication_Date,
                Number_Of_Pages = b.Number_Of_Pages,
                Image = b.Image ?? string.Empty,
                Genres = b.Genres ?? new List<string>() // Инициализация пустым списком если null
            }).ToList();
        }

        public async Task<Book_Issuance> BookReservationForDay(BookIssuanceModel book)
        {
            Console.WriteLine("Метод вызван");
            var existingReservation = await _context.Book_Issuances
                .FirstOrDefaultAsync(b => b.ISBN == book.ISBN && b.Return_Date == null);

            if (existingReservation != null)
            {
                throw new Exception("Книга уже забронирована или выдана");
            }

            // Генерируем новый номер выдачи (можно использовать более сложную логику)
            var lastIssueNumber = await _context.Book_Issuances
                .OrderByDescending(b => b.Issue_Number)
                .Select(b => b.Issue_Number)
                .FirstOrDefaultAsync();

            var reservationBook = new Book_Issuance()
            {
                ISBN = book.ISBN,
                Issue_Number = lastIssueNumber + 1,
                Issue_Date = DateTime.Now,
                Return_Date = DateTime.Now.AddDays(1),
                Name_Reader = book.UserName,
            };

            _context.Book_Issuances.Add(reservationBook);
            await _context.SaveChangesAsync();

            return reservationBook;
        }
    }
}
