using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;

namespace Excos.AspNetCore.Lite.UITests;

/// <summary>
/// Fixture for managing the test server lifecycle for UI tests.
/// Uses Testcontainers to run the actual TestServer Docker container.
/// Also manages Playwright installation and initialization.
/// </summary>
public class TestServerFixture : IAsyncLifetime
{
    private IContainer? _container;
    private static bool _playwrightInstalled = false;
    private static readonly object _installLock = new object();

    private static void EnsurePlaywrightInstalled()
    {
        lock (_installLock)
        {
            if (!_playwrightInstalled)
            {
                Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
                _playwrightInstalled = true;
            }
        }
    }

    /// <summary>
    /// Gets the base URL for the test server.
    /// </summary>
    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Install Playwright browsers if not already installed (only once per test run)
        EnsurePlaywrightInstalled();

        // Build and start the TestServer container with auth disabled for testing
        _container = new ContainerBuilder()
            .WithImage("excos-lite-test-server:latest")
            .WithPortBinding(8080, true)
            .WithEnvironment("DisableAuth", "true")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Now listening on"))
            .Build();

        await _container.StartAsync();

        // Get the mapped port
        var port = _container.GetMappedPublicPort(8080);
        BaseUrl = $"http://localhost:{port}";

        // Give the server a moment to fully start
        await Task.Delay(500);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}
