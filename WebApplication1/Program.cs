using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DataLayer;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Добавление контекста базы данных
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация сервиса AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LibraryService>();

// Добавление поддержки сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
    options.Cookie.HttpOnly = true; // Защита от XSS
    options.Cookie.IsEssential = true; // Обязательные cookies
});

// Добавление сервисов в контейнер
builder.Services.AddControllersWithViews();

// Настройка Identity, если используется
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<EFDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Настройка конвейера HTTP запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Добавляем middleware для сессий
app.UseSession();

app.UseAuthentication(); // Добавляем аутентификацию
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();