using Crawl.Components;
using Crawl.Data;
using Crawl.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Crawl.Models.Login;
using Crawl.Services.Crawl.Video;
using Crawl.Services.Crawl.Articles;
using Crawl.Services.Crawl.Helpers;
using Crawl.Services.Crawl.GeneralMatches;
using Crawl.Services.Crawl.Videos;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddScoped<EmployeeAuthService>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddSingleton<TeamInformationGlobal>();
builder.Services.AddSingleton<CrawlService>();
builder.Services.AddSingleton<VideoPlayerService>();
builder.Services.AddSingleton<ArticlesCrawlService>();
builder.Services.AddSingleton<CrawlHelpersService>();
builder.Services.AddSingleton<VideoCrawlService>();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.ConfigureApplicationCookie(options =>

{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true; // <- JS cannot access this
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

/// ✅ LOGIN API ENDPOINT
app.MapPost("/api/login", async (
    LoginRequest login,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppDbContext db) =>
{
    var user = await userManager.FindByEmailAsync(login.Email);

    if (user == null)
        return Results.Unauthorized();

    var result = await signInManager.PasswordSignInAsync(
        user,
        login.Password,
        isPersistent: true,
        lockoutOnFailure: false);

    if (!result.Succeeded)
        return Results.Unauthorized();

    var employee = await db.Employees
        .FirstOrDefaultAsync(e => e.Email == login.Email);

    if (employee != null && employee.IsFirstLogin)
    {
        employee.IsFirstLogin = false;
        await db.SaveChangesAsync();
    }

    return Results.Ok();
});

app.MapPost("/api/logout", async (
    SignInManager<AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


/// ✅ DTO
public record LoginRequest(string Email, string Password);