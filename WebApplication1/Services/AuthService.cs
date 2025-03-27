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
    public class AuthService
    {
        private readonly EFDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(EFDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateJwtToken(string email, int id)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Используйте Guid для уникальности
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, "reader")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetJwtKey()
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            return key;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<Users> RegisterAsync(RegisterModel registerModel)
        {
            var User = new Users()
            {
                UserName = registerModel.Username,
                Email = registerModel.Email,
                PasswordHash = HashPassword(registerModel.Password),
                Name = registerModel.Name,
                Role = "Reader"
            };
            _logger.LogInformation("Добавление пользователя в базу данных: {@User}", User);

            _context.Users.Add(User);
            await _context.SaveChangesAsync(); // Сохраняем пользователя

            return User; // Возвращаем зарегистрированного пользователя    
        }

        public async Task<Users> LoginAsync(LoginModel loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            // Ищем пользователя по email или нику
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Username || u.UserName == loginDto.Username);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                Console.WriteLine("Не вошел");
                throw new InvalidOperationException("Неверный логин или пароль.");
            }

            Console.WriteLine("Вошел");

            return user; // Возвращаем пользователя при успешном входе
        }
    }
}
