using System.Security.Claims;
using Excos.AspNetCore.Lite.TestServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Excos.AspNetCore.Lite.TestServer;

/// <summary>
/// Extension methods for mapping authentication-related endpoints.
/// </summary>
public static class AuthenticationEndpoints
{
    /// <summary>
    /// Maps login, logout, and login page endpoints.
    /// </summary>
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
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
    }
}
