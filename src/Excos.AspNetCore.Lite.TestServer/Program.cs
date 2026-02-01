using Excos.AspNetCore.Lite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add Excos services
builder.Services.AddExcos();

// Check if authentication should be disabled (for testing)
var disableAuth = builder.Configuration.GetValue<bool>("DisableAuth");

if (!disableAuth)
{
    // Add cookie authentication services
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.Cookie.Name = "ExcosAuth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
        });
    builder.Services.AddAuthorization();
}

var app = builder.Build();

if (!disableAuth)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Add a simple root endpoint
app.MapGet("/", () => "Test server is running. Visit /excos to see the Excos plugin.");

// Login endpoint
app.MapPost("/api/login", async (LoginRequest request, HttpContext context) =>
{
    // Simple validation: username:password
    if (request.Username == "user" && request.Password == "password")
    {
        var claims = new[] { new Claim(ClaimTypes.Name, request.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Results.Ok(new { success = true });
    }

    return Results.Unauthorized();
});

// Logout endpoint
app.MapPost("/api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { success = true });
});

// Check auth status endpoint
app.MapGet("/api/auth/status", (HttpContext context) =>
{
    var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
    return Results.Ok(new { isAuthenticated, username = context.User?.Identity?.Name });
});

// Map the Excos plugin at /excos - returns API route group for applying policies
var excosApi = app.MapExcos("/excos");

// Apply authorization to all plugin API endpoints (unless disabled)
if (!disableAuth)
{
    excosApi.RequireAuthorization();
}

app.Run();

// Make the implicit Program class public so it can be used by WebApplicationFactory
public partial class Program { }

// Request DTOs
public record LoginRequest(string Username, string Password);
