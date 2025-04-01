using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;
        private readonly LibraryService _libraryService;
        private readonly EFDbContext _context;

        // Оставляем только один конструктор
        public HomeController(ILogger<HomeController> logger, AuthService authService, LibraryService libraryService, EFDbContext context)
        {
            _logger = logger;
            _authService = authService;
            _libraryService = libraryService;
            _context = context;
        }

        [HttpPost("Home/Register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid registration data.");
            }

            // Регистрация пользователя
            var user = await _authService.RegisterAsync(model);

            // Если регистрация успешна, редирект на страницу Index
            if (user != null)
            {
                return RedirectToAction("Index");
            }

            return BadRequest("Registration failed.");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Логин или пароль не могут быть пустыми.";
                return View("Index");
            }

            try
            {
                var user = await _authService.LoginAsync(new LoginModel { Username = usernameOrEmail, Password = password });
                var token = _authService.GenerateJwtToken(user.Email, user.Id);

                // Сохранение информации о пользователе в сессии
                HttpContext.Session.SetString("Username", user.UserName);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Main");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Login");
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            var books = _context.Books.ToList();

            // Преобразование List<Book> в List<BookModel>
            var bookModels = books.Select(b => new BookModel
            {
                ISBN = b.ISBN, 
                Book_Name = b.Book_Name,
                Book_Author = b.Book_Author,
                Book_Description = b.Book_Description,
                Publication_Date = b.Publication_Date,
                Number_Of_Pages = b.Number_Of_Pages,
                Image = b.Image
            }).ToList();

            return View(bookModels);
        }

        [HttpGet("Home/Book/{ISBN}")]
        public async Task<IActionResult> BookDetail(string ISBN)
        {
            var book = await _libraryService.GetBookByISBN(ISBN);

            if (book == null)
            {
                return NotFound();
            }

            var bookDetail = new BookModel
            {
                ISBN = book.ISBN,
                Book_Name = book.Book_Name,
                Book_Author = book.Book_Author,
                Book_Description = book.Book_Description,
                Publication_Date = book.Publication_Date,
                Number_Of_Pages = book.Number_Of_Pages,
                Image = book.Image
            };



            return View(bookDetail);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}