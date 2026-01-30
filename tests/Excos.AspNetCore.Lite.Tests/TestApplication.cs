using Microsoft.AspNetCore.Builder;

namespace Excos.AspNetCore.Lite.Tests;

/// <summary>
/// Test application for in-memory testing of the Excos plugin.
/// </summary>
public class TestWebApplication
{
    public static void Main(string[] args)
    {
        var app = CreateApplication(args);
        app.Run();
    }

    public static WebApplication CreateApplication(string[]? args = null)
    {
        var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

        // Add Excos services
        builder.Services.AddExcos(options =>
        {
            options.PathPrefix = "/excos";
        });

        var app = builder.Build();

        // Register the Excos plugin - it will map the status endpoint automatically
        app.UseExcos();

        return app;
    }
}
