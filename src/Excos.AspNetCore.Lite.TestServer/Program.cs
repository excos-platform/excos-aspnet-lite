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

// Login page (simple HTML form) - served by test server, not part of plugin
app.MapGet("/login", (HttpContext context) =>
{
    var returnUrl = context.Request.Query["ReturnUrl"].ToString() ?? "/excos";
    var html = LoginPageHtml.GetHtml(returnUrl);
    return Results.Content(html, "text/html");
}).AllowAnonymous();

// Login endpoint (API)
app.MapPost("/login", async (LoginRequest request, HttpContext context) =>
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
}).AllowAnonymous();

// Logout endpoint
app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

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

// Request DTOs
public record LoginRequest(string Username, string Password);

public static class LoginPageHtml
{
    public static string GetHtml(string returnUrl) => @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Login - Excos Test Server</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
            margin: 0;
        }
        .login-card {
            background: white;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            max-width: 400px;
            width: 100%;
        }
        .login-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }
        .login-header h1 {
            margin: 0 0 5px 0;
            font-size: 1.8em;
        }
        .login-header p {
            margin: 0;
            opacity: 0.9;
        }
        .login-form {
            padding: 30px;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
            color: #333;
            font-weight: 500;
        }
        .form-group input {
            width: 100%;
            padding: 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 1em;
            box-sizing: border-box;
        }
        .form-group input:focus {
            outline: none;
            border-color: #667eea;
        }
        .btn {
            width: 100%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 1em;
            font-weight: 500;
        }
        .btn:hover {
            opacity: 0.9;
        }
        .login-hint {
            text-align: center;
            padding: 0 30px 30px;
            color: #666;
            font-size: 0.9em;
        }
        .error {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 15px;
            display: none;
        }
        .error.show {
            display: block;
        }
    </style>
</head>
<body>
    <div class=""login-card"">
        <div class=""login-header"">
            <h1>Excos Test Server</h1>
            <p>Please log in to continue</p>
        </div>
        <form class=""login-form"" id=""loginForm"" action=""/login"" method=""post"">
            <input type=""hidden"" name=""returnUrl"" value=""" + returnUrl + @""">
            <div class=""error"" id=""error""></div>
            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""username"" required autofocus>
            </div>
            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""password"" id=""password"" name=""password"" required>
            </div>
            <button type=""submit"" class=""btn"">Log In</button>
        </form>
        <div class=""login-hint"">
            <p>Default credentials: <strong>user</strong> / <strong>password</strong></p>
        </div>
    </div>
    <script>
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const username = formData.get('username');
            const password = formData.get('password');
            const returnUrl = formData.get('returnUrl');
            
            try {
                const response = await fetch('/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username, password })
                });
                
                if (response.ok) {
                    window.location.href = returnUrl;
                } else {
                    const error = document.getElementById('error');
                    error.textContent = 'Invalid username or password';
                    error.classList.add('show');
                }
            } catch (err) {
                const error = document.getElementById('error');
                error.textContent = 'Failed to connect to server';
                error.classList.add('show');
            }
        });
    </script>
</body>
</html>";
}
