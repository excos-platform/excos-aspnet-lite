# Skill: UI Testing with Playwright

## Overview
This skill focuses on writing effective UI tests using Playwright with C# and xUnit for testing browser-based functionality.

## Testing Philosophy

### Test User Journeys, Not Implementation
- Focus on **what users do and see**, not how the code works
- Test through the user interface as a user would interact with it
- Avoid coupling tests to implementation details like CSS classes or DOM structure

### Use Semantic Selectors
Prioritize selectors that won't break when styling or HTML structure changes:

1. **Test IDs** (most stable) - `data-testid` attributes
2. **ARIA Roles** (semantic) - Button, link, heading roles
3. **Visible Text** (user-facing) - Text users actually see
4. **Labels** (semantic) - Form field labels

❌ **Never use:** CSS classes, element hierarchy, xpath for layout

## Test Infrastructure

### Project Setup
- UI tests are in `tests/Excos.AspNetCore.Lite.UITests/`
- Uses **C# Playwright API** with **xUnit**
- Tests run against real Kestrel server (not TestServer)
- Uses **Testcontainers** to run actual TestServer Docker container

### Test Server with Testcontainers

We use Testcontainers to run the actual TestServer Docker image:

```csharp
public class TestServerFixture : IAsyncLifetime
{
    private IContainer? _container;
    public string BaseUrl { get; private set; } = string.Empty;
    
    public async Task InitializeAsync()
    {
        _container = new ContainerBuilder()
            .WithImage("excos-lite-test-server:latest")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Now listening on"))
            .Build();
        
        await _container.StartAsync();
        
        var port = _container.GetMappedPublicPort(8080);
        BaseUrl = $"http://localhost:{port}";
    }
    
    public async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }
}
```

**Benefits:**
- No code duplication - uses same container as CI
- Automatic lifecycle management
- Dynamic port mapping to avoid conflicts
- Realistic testing environment

### Test Class Structure

```csharp
[Collection("TestServer")] // Share fixture across tests
public class ExcosPluginUITests : IAsyncLifetime
{
    private readonly TestServerFixture _fixture;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    
    public ExcosPluginUITests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }
    
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        
        var context = await _browser.NewContextAsync(new()
        {
            // Set auth headers if needed
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = "Basic " + 
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("user:password"))
            }
        });
        
        _page = await context.NewPageAsync();
    }
    
    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
```

## Writing Playwright Tests

### Test Naming Convention

Format: `ComponentOrFeature_Scenario_ExpectedOutcome`

```csharp
[Fact]
public async Task PluginUI_Loads_Successfully()

[Fact]
public async Task ApiStatus_Button_Fetches_And_Displays_Status()

[Fact]
public async Task ErrorMessage_Displays_When_Api_Fails()
```

### Selector Patterns

#### ✅ Good: Test IDs (Most Stable)

```csharp
var button = _page!.GetByTestId("check-status-button");
await button.ClickAsync();

var result = _page!.GetByTestId("status-result");
await Assertions.Expect(result).ToBeVisibleAsync();
```

**Frontend:**
```tsx
<button data-testid="check-status-button" onClick={handleClick}>
  Check Status
</button>
```

#### ✅ Good: ARIA Roles (Semantic)

```csharp
var button = _page!.GetByRole(AriaRole.Button, new() { Name = "Check API Status" });
await button.ClickAsync();

var heading = _page!.GetByRole(AriaRole.Heading, new() { Name = "Welcome to Excos" });
await Assertions.Expect(heading).ToBeVisibleAsync();
```

#### ✅ Good: Visible Text

```csharp
var element = _page!.GetByText("Welcome to Excos");
await Assertions.Expect(element).ToBeVisibleAsync();
```

#### ❌ Bad: CSS Classes or DOM Structure

```csharp
// DON'T DO THIS - Brittle and breaks with styling changes
var button = _page!.Locator("div.card > div > button.btn.btn-primary");

// DON'T DO THIS - Breaks if HTML structure changes
var element = _page!.Locator(".container .row .col-md-6 p");
```

## Assertions

Use Playwright's async assertions with built-in retries and waiting.

### Common Assertions

```csharp
// Visibility
await Assertions.Expect(element).ToBeVisibleAsync();
await Assertions.Expect(element).ToBeHiddenAsync();

// Content
await Assertions.Expect(element).ToContainTextAsync("Expected text");
await Assertions.Expect(element).ToHaveTextAsync("Exact text");

// State
await Assertions.Expect(element).ToBeEnabledAsync();
await Assertions.Expect(element).ToBeDisabledAsync();
await Assertions.Expect(element).ToBeCheckedAsync();

// Count
await Assertions.Expect(_page!.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(5);
```

### ✅ Good Assertions

```csharp
// Specific and clear
await Assertions.Expect(heading).ToContainTextAsync("Welcome to Excos");
await Assertions.Expect(statusText).ToContainTextAsync("Status: OK");

// Wait for dynamic content
await Assertions.Expect(loadingSpinner).ToBeHiddenAsync();
await Assertions.Expect(resultContainer).ToBeVisibleAsync();
```

### ❌ Bad Assertions

```csharp
// Too vague
Assert.True(await element.IsVisibleAsync()); // Use Playwright assertions instead

// Not waiting for dynamic content
var text = await element.TextContentAsync(); // Might not be loaded yet!
Assert.Contains("OK", text);
```

## Waiting and Timing

### ✅ Use Playwright's Built-in Waiting

Playwright automatically waits for elements to be actionable:

```csharp
// Automatically waits for button to be visible and enabled
await button.ClickAsync();

// Automatically waits for element to be visible
await Assertions.Expect(element).ToBeVisibleAsync();

// Explicitly wait for element
await _page!.WaitForSelectorAsync("#root > div");
```

### ❌ Don't Use Arbitrary Delays

```csharp
// DON'T DO THIS
await Task.Delay(3000); // Fragile and slow!
await button.ClickAsync();

// DO THIS INSTEAD
await Assertions.Expect(button).ToBeVisibleAsync();
await button.ClickAsync();
```

## Handling Authentication

Set authentication headers in the browser context during setup:

```csharp
public async Task InitializeAsync()
{
    _playwright = await Playwright.CreateAsync();
    _browser = await _playwright.Chromium.LaunchAsync();
    
    var context = await _browser.NewContextAsync(new()
    {
        ExtraHTTPHeaders = new Dictionary<string, string>
        {
            ["Authorization"] = "Basic " + 
                Convert.ToBase64String(Encoding.UTF8.GetBytes("user:password"))
        }
    });
    
    _page = await context.NewPageAsync();
}
```

**Benefits:**
- Authentication is handled once per test
- No need to set headers in individual tests
- Works for all requests made by the page

## Page Object Pattern

For complex interactions, create helper methods:

```csharp
public class ExcosPluginUITests
{
    private async Task ClickStatusButtonAsync()
    {
        var button = _page!.GetByTestId("check-status-button");
        await button.ClickAsync();
    }
    
    private async Task WaitForStatusResultAsync()
    {
        var result = _page!.GetByTestId("status-result");
        await Assertions.Expect(result).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task ApiStatus_Button_Fetches_And_Displays_Status()
    {
        await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
        
        await ClickStatusButtonAsync();
        await WaitForStatusResultAsync();
        
        var statusText = _page!.GetByTestId("status-result");
        await Assertions.Expect(statusText).ToContainTextAsync("Status:");
    }
}
```

## Test Organization

### One Test Class Per Feature Area

```
ExcosPluginUITests.cs         - Basic plugin loading and display
ApiStatusTests.cs             - API status checking functionality
NavigationTests.cs            - Navigation and routing
ErrorHandlingTests.cs         - Error states and messages
```

### Test Independence

Each test should:
- Set up its own required state
- Not depend on other tests
- Clean up after itself (handled by IAsyncLifetime)

```csharp
[Fact]
public async Task Test1_Loads_Successfully()
{
    await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
    // Test is self-contained
}

[Fact]
public async Task Test2_Shows_Error_State()
{
    await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
    // Doesn't depend on Test1
}
```

## Testing Dynamic Content

### Loading States

Test that loading indicators appear and disappear:

```csharp
[Fact]
public async Task ApiCall_Shows_Loading_State()
{
    await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
    
    var button = _page!.GetByTestId("check-status-button");
    await button.ClickAsync();
    
    // Loading indicator should appear
    var loading = _page!.GetByTestId("loading-spinner");
    await Assertions.Expect(loading).ToBeVisibleAsync();
    
    // Then disappear when done
    await Assertions.Expect(loading).ToBeHiddenAsync();
    
    // Result should be visible
    var result = _page!.GetByTestId("status-result");
    await Assertions.Expect(result).ToBeVisibleAsync();
}
```

### Error States

Test error handling and display:

```csharp
[Fact]
public async Task ApiError_Displays_Error_Message()
{
    // Simulate API failure (if your test setup supports it)
    await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
    
    // Trigger error condition...
    
    var errorMessage = _page!.GetByTestId("error-message");
    await Assertions.Expect(errorMessage).ToBeVisibleAsync();
    await Assertions.Expect(errorMessage).ToContainTextAsync("Failed to");
}
```

## Debugging Playwright Tests

### Take Screenshots on Failure

```csharp
[Fact]
public async Task SomeTest()
{
    try
    {
        // Test code...
    }
    catch
    {
        await _page!.ScreenshotAsync(new() { Path = "failure-screenshot.png" });
        throw;
    }
}
```

### Run in Headed Mode (Local Development)

```csharp
_browser = await _playwright.Chromium.LaunchAsync(new()
{
    Headless = false,  // Shows browser window
    SlowMo = 500       // Slows down actions for visibility
});
```

### Use Pause for Interactive Debugging

```csharp
await _page!.PauseAsync(); // Opens Playwright Inspector
```

## Common Pitfalls

### ❌ Testing HTML Structure
```csharp
// Bad - Breaks when structure changes
var button = page.Locator("div.card > div > button.btn.btn-primary");
```

### ❌ Using Arbitrary Delays
```csharp
// Bad - Slow and fragile
await Task.Delay(3000);
```

### ❌ Not Waiting for Dynamic Content
```csharp
// Bad - Might fail if content not loaded
var text = await element.TextContentAsync();
Assert.Contains("OK", text);

// Good - Waits for content
await Assertions.Expect(element).ToContainTextAsync("OK");
```

### ❌ Testing CSS Classes
```csharp
// Bad - Coupled to styling
await Assertions.Expect(element).ToHaveClassAsync("btn-primary");
```

### ❌ Depending on Test Execution Order
```csharp
// Bad - Test2 assumes Test1 ran first
[Fact] public async Task Test1() { /* setup state */ }
[Fact] public async Task Test2() { /* uses state from Test1 */ }
```

### ❌ Not Handling Authentication Properly
```csharp
// Bad - Setting auth in each test
[Fact]
public async Task Test1()
{
    await _page!.SetExtraHTTPHeadersAsync(...); // Repetitive!
}

// Good - Set auth once in browser context
```

## Best Practices Checklist

When writing Playwright tests:
- [ ] Do test names follow `Feature_Scenario_Outcome` format?
- [ ] Are semantic selectors used (test IDs, ARIA roles, visible text)?
- [ ] Are CSS classes and DOM structure avoided in selectors?
- [ ] Are Playwright's async assertions used (not manual delays)?
- [ ] Is authentication handled in browser context setup?
- [ ] Are tests independent and can run in any order?
- [ ] Is dynamic content properly waited for?
- [ ] Are both success and error states tested?
- [ ] Are loading states tested?
- [ ] Would tests survive HTML structure or styling changes?

## Example: Complete Test

```csharp
[Fact]
public async Task ApiStatus_Button_Fetches_And_Displays_Status()
{
    // Arrange - Navigate to page
    await _page!.GotoAsync(_fixture.BaseUrl + "/excos");
    
    // Verify initial state
    var welcomeText = _page!.GetByRole(AriaRole.Heading, new() { Name = "Welcome to Excos" });
    await Assertions.Expect(welcomeText).ToBeVisibleAsync();
    
    // Act - Click status button
    var statusButton = _page!.GetByTestId("check-status-button");
    await statusButton.ClickAsync();
    
    // Assert - Wait for result to appear
    var statusResult = _page!.GetByTestId("status-result");
    await Assertions.Expect(statusResult).ToBeVisibleAsync();
    
    // Assert - Verify content
    await Assertions.Expect(statusResult).ToContainTextAsync("Status:");
}
```

## References
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [Best Practices - Playwright](https://playwright.dev/dotnet/docs/best-practices)
- [Locators - Playwright](https://playwright.dev/dotnet/docs/locators)
- [Assertions - Playwright](https://playwright.dev/dotnet/docs/test-assertions)
