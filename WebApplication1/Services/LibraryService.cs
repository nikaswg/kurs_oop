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
    }
}
