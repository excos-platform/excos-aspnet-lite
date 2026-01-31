using Excos.AspNetCore.Lite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace Excos.AspNetCore.Lite.UITests;

/// <summary>
/// Fixture for managing the test server lifecycle for UI tests.
/// Uses a real Kestrel server so Playwright can connect to it.
/// Also manages Playwright installation and initialization.
/// </summary>
public class TestServerFixture : IAsyncLifetime
{
    private IHost? _host;
    private static bool _playwrightInstalled = false;
    private static readonly object _installLock = new object();

    /// <summary>
    /// Gets the base URL for the test server.
    /// </summary>
    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the username for basic authentication.
    /// </summary>
    public string Username => "user";

    /// <summary>
    /// Gets the password for basic authentication.
    /// </summary>
    public string Password => "password";

    public async Task InitializeAsync()
    {
        // Install Playwright browsers if not already installed (only once per test run)
        lock (_installLock)
        {
            if (!_playwrightInstalled)
            {
                Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
                _playwrightInstalled = true;
            }
        }

        // Use a fixed port to avoid issues
        var port = 5123; // Use a non-standard port to avoid conflicts
        BaseUrl = $"http://localhost:{port}";

        // Create a real web host directly (not using WebApplicationFactory which uses TestServer)
        var builder = WebApplication.CreateBuilder();

        // Add Excos services
        builder.Services.AddExcos();

        // Add authentication services
        builder.Services.AddAuthentication("BasicAuthentication")
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
        builder.Services.AddAuthorization();

        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(BaseUrl);

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        // Add a simple root endpoint
        app.MapGet("/", () => "Test server is running. Visit /excos to see the Excos plugin.");

        // Map the Excos plugin at /excos
        var excosApi = app.MapExcos("/excos");
        excosApi.RequireAuthorization();

        _host = app;
        await _host.StartAsync();

        // Give the server a moment to fully start
        await Task.Delay(500);
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
