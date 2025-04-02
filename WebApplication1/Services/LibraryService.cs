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
