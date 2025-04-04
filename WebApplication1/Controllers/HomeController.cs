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

        [HttpPost]
        public async Task<IActionResult> ReserveBook([FromBody] BookIssuanceModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Неверные данные" });
                }

                var result = await _libraryService.BookReservationForDay(model);

                return Json(new
                {
                    success = true,
                    message = $"Книга успешно забронирована! Номер брони: {result.Issue_Number}",
                    issueNumber = result.Issue_Number
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Метод для проверки бронирования
        public async Task<IActionResult> CheckBookReservation(string isbn)
        {
            try
            {
                var reservation = await _context.Book_Issuances
                    .Where(b => b.ISBN == isbn && b.Return_Date > DateTime.Now)
                    .OrderByDescending(b => b.Issue_Date)
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    isReserved = reservation != null,
                    reservationInfo = reservation != null ? new
                    {
                        returnDate = reservation.Return_Date,
                        userName = reservation.Name_Reader,
                        issueNumber = reservation.Issue_Number
                    } : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { isReserved = false });
            }
        }

        // Метод для отмены бронирования
        [HttpPost]
        public async Task<IActionResult> CancelReservation([FromBody] BookIssuanceModel model)
        {
            try
            {
                var reservation = await _context.Book_Issuances
                    .FirstOrDefaultAsync(b => b.ISBN == model.ISBN &&
                                           b.Name_Reader == model.UserName &&
                                           b.Return_Date > DateTime.Now);

                if (reservation == null)
                {
                    return Json(new { success = false, message = "Бронирование не найдено или уже отменено" });
                }

                _context.Book_Issuances.Remove(reservation);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckFavoriteStatus(string isbn, string username)
        {
            try
            {
                var isFavorite = await _context.FavoriteBooks
                    .AnyAsync(f => f.ISBN == isbn && f.UserName == username);

                return Json(new { isFavorite });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] FavoriteRequest request)
        {
            try
            {
                // Проверяем, не добавлена ли уже книга
                var exists = await _context.FavoriteBooks
                    .AnyAsync(f => f.ISBN == request.isbn && f.UserName == request.userName);

                if (exists)
                {
                    return Json(new { success = false, message = "Книга уже в избранном" });
                }

                // Добавляем книгу в избранное
                var favorite = new FavoriteBook
                {
                    UserName = request.userName,
                    ISBN = request.isbn,
                    AddedDate = DateTime.Now
                };

                _context.FavoriteBooks.Add(favorite);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] FavoriteRequest request)
        {
            try
            {
                var favorite = await _context.FavoriteBooks
                    .FirstOrDefaultAsync(f => f.ISBN == request.isbn && f.UserName == request.userName);

                if (favorite != null)
                {
                    _context.FavoriteBooks.Remove(favorite);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Вспомогательный класс для запроса
        public class FavoriteRequest
        {
            public string isbn { get; set; }
            public string userName { get; set; }
        }
    }
}