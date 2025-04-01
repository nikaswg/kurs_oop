using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DataLayer;
using Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ���������� ��������� ���� ������
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����������� ������� AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LibraryService>();

// ���������� ��������� ������
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ����� ����� ������
    options.Cookie.HttpOnly = true; // ������ �� XSS
    options.Cookie.IsEssential = true; // ������������ cookies
});

// ���������� �������� � ���������
builder.Services.AddControllersWithViews();

// ��������� Identity, ���� ������������
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<EFDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// ��������� ��������� HTTP ��������
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ��������� middleware ��� ������
app.UseSession();

app.UseAuthentication(); // ��������� ��������������
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();