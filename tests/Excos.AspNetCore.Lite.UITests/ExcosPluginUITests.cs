using Microsoft.Playwright;

namespace Excos.AspNetCore.Lite.UITests;

/// <summary>
/// UI tests for the Excos plugin using Playwright.
/// Tests focus on functionality rather than specific HTML structure.
/// </summary>
[Collection("TestServer")]
public class ExcosPluginUITests : IAsyncLifetime
{
    private readonly TestServerFixture _fixture;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public ExcosPluginUITests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        // Create context with basic auth headers
        var authHeader = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{_fixture.Username}:{_fixture.Password}"));

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Basic {authHeader}"
            }
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task PluginUI_Loads_Successfully()
    {
        // Arrange & Act
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Assert - Check that the root React element is present and has content
        var rootElement = _page.Locator("#root > div");
        await Assertions.Expect(rootElement).ToBeVisibleAsync();

        // Verify main heading is present (functional check, not structure check)
        var heading = _page.GetByRole(AriaRole.Heading, new() { Name = "Excos ASP.NET Core Plugin" });
        await Assertions.Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Welcome_Section_Is_Displayed()
    {
        // Arrange & Act
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Assert - Check for welcome content (by text, not structure)
        var welcomeText = _page.GetByText("This is a demonstration of the Excos plugin");
        await Assertions.Expect(welcomeText).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ApiStatus_Button_Exists_And_Is_Clickable()
    {
        // Arrange
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Act - Find button by its test id (semantic identifier)
        var statusButton = _page.GetByTestId("check-status-button");

        // Assert
        await Assertions.Expect(statusButton).ToBeVisibleAsync();
        await Assertions.Expect(statusButton).ToBeEnabledAsync();
    }

    [Fact]
    public async Task ApiStatus_Button_Fetches_And_Displays_Status()
    {
        // Arrange
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Act - Click the status check button
        var statusButton = _page.GetByTestId("check-status-button");
        await statusButton.ClickAsync();

        // Assert - Wait for and verify the status result appears
        var statusResult = _page.GetByTestId("status-result");
        await Assertions.Expect(statusResult).ToBeVisibleAsync();

        // Verify success state is displayed
        await Assertions.Expect(statusResult).ToContainTextAsync("API Status: Online");
        await Assertions.Expect(statusResult).ToContainTextAsync("Status: running");
        await Assertions.Expect(statusResult).ToContainTextAsync("Version: 1.0.0");
    }

    [Fact]
    public async Task Features_List_Is_Displayed()
    {
        // Arrange & Act
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Assert - Check for features section by heading
        var featuresHeading = _page.GetByRole(AriaRole.Heading, new() { Name = "Features" });
        await Assertions.Expect(featuresHeading).ToBeVisibleAsync();

        // Verify some key features are listed (content check, not structure)
        await Assertions.Expect(_page.GetByText("Embedded static file serving")).ToBeVisibleAsync();
        await Assertions.Expect(_page.GetByText("SPA routing support")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PathPrefix_Is_Detected_Correctly()
    {
        // Arrange & Act - Navigate to the plugin at /excos path
        await _page!.GotoAsync($"{_fixture.BaseUrl}/excos/");

        // Click the status button to trigger API call
        var statusButton = _page.GetByTestId("check-status-button");
        await statusButton.ClickAsync();

        // Assert - Verify the API call was successful (which confirms path prefix worked)
        var statusResult = _page.GetByTestId("status-result");
        await Assertions.Expect(statusResult).ToContainTextAsync("API Status: Online");
    }
}
