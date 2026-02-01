using Excos.AspNetCore.Lite;
using Excos.AspNetCore.Lite.TestServer;
using Microsoft.AspNetCore.Authentication.Cookies;

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

// Map authentication endpoints (login, logout)
app.MapAuthenticationEndpoints();

// Map the Excos plugin at /excos - returns API route group for applying policies
var excosApi = app.MapExcos("/excos");

// Apply authorization to all plugin endpoints (unless auth is disabled)
if (!disableAuth)
{
    excosApi.RequireAuthorization();
}

app.Run();

// Make the implicit Program class public so it can be used by WebApplicationFactory
public partial class Program { }
