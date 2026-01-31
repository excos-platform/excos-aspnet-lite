# Skill: Integration Testing with xUnit

## Overview
This skill covers how to write effective integration tests for ASP.NET Core plugins using xUnit, WebApplicationFactory, and alternative hosting patterns.

## Testing Philosophy

### Focus on Behavior, Not Implementation
- Test what the code **does**, not **how** it does it
- Test through public APIs, not internal implementation details
- Write tests that survive refactoring

### Test Both Success and Failure Paths
- Happy path: Does it work when everything goes right?
- Error cases: Does it fail gracefully when things go wrong?
- Edge cases: Boundary conditions, empty inputs, special characters

### Keep Tests Independent
- Each test should run in isolation
- Tests should pass regardless of execution order
- Avoid shared mutable state between tests

## Test Framework: xUnit

We use **xUnit** for all tests in this project.

### Why xUnit?
- No shared test context between tests (each test gets fresh instance)
- Built-in parallelization
- Clean, minimal syntax
- Industry standard for .NET

### Basic Test Structure

```csharp
public class ExcosApiTests
{
    [Fact]
    public async Task GetStatus_Returns_Ok()
    {
        // Arrange - Set up test conditions
        var client = CreateClient();
        
        // Act - Execute the operation being tested
        var response = await client.GetAsync("/excos/api/status");
        
        // Assert - Verify the results
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Theory]
    [InlineData("/excos")]
    [InlineData("/custom-path")]
    public async Task MapExcos_WorksWithDifferentPaths(string pathPrefix)
    {
        // Test the same behavior with different inputs
    }
}
```

## Integration Testing with WebApplicationFactory

`WebApplicationFactory<T>` provides in-memory integration testing without starting a real server.

### Basic Setup

```csharp
public class ExcosPluginTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ExcosPluginTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [Fact]
    public async Task ExcosPlugin_Serves_StaticFiles()
    {
        var response = await _client.GetAsync("/excos/app/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

### Customizing the Test Server

Override services or configuration for testing:

```csharp
private WebApplicationFactory<Program> CreateFactory(
    Action<IServiceCollection>? configureServices = null)
{
    return new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                configureServices?.Invoke(services);
            });
        });
}

[Fact]
public async Task ExcosPlugin_UsesCustomService()
{
    var factory = CreateFactory(services =>
    {
        services.AddSingleton<IExcosStatusService, TestStatusService>();
    });
    
    var client = factory.CreateClient();
    var response = await client.GetAsync("/excos/api/status");
    // Test with mock service...
}
```

## Testing Pure Endpoint Routing

For edge cases or when you want to test endpoints without full WebApplication:

```csharp
[Fact]
public async Task ExcosEndpoints_WorkWithoutWebApplication()
{
    var host = new HostBuilder()
        .ConfigureServices(services =>
        {
            services.AddRouting();
            services.AddExcos();
        })
        .ConfigureWebHost(webHost =>
        {
            webHost.UseTestServer();
            webHost.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapExcos());
            });
        })
        .Build();
    
    await host.StartAsync();
    
    var client = host.GetTestClient();
    var response = await client.GetAsync("/excos/api/status");
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Test Organization

### Group Related Tests in Dedicated Classes

Organize tests by feature or component:

```
ExcosApiTests.cs              - Tests for API endpoints
ExcosStaticFileTests.cs       - Tests for static file serving
ExcosAuthorizationTests.cs    - Tests for authorization scenarios
ExcosPathHandlingTests.cs     - Tests for different path configurations
```

### Use Descriptive Test Names

Test names should describe **what** is being tested and **what** the expected outcome is:

```csharp
// ✅ Good - Clear, descriptive names
[Fact]
public async Task GetStatus_Returns_Json_Response()

[Fact]
public async Task StaticFile_NotFound_Returns_404()

[Fact]
public async Task MapExcos_WithAuthorization_Requires_Authentication()

// ❌ Bad - Vague names
[Fact]
public async Task Test1()

[Fact]
public async Task StatusTest()
```

## Helper Methods

### Extract Common Setup Logic

When multiple tests share setup code, extract it into helper methods:

```csharp
public class ExcosPluginTests
{
    private HttpClient CreateClient(
        string pathPrefix = "/excos",
        Action<IServiceCollection>? configureServices = null)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    configureServices?.Invoke(services);
                });
                
                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapExcos(pathPrefix));
                });
            });
        
        return factory.CreateClient();
    }
    
    [Theory]
    [InlineData("/excos")]
    [InlineData("/admin")]
    public async Task MapExcos_WorksWithDifferentPaths(string pathPrefix)
    {
        // Clean and focused - setup is in helper
        var client = CreateClient(pathPrefix);
        var response = await client.GetAsync($"{pathPrefix}/api/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

### Benefits of Helper Methods:
- **Reduce duplication** - Setup code written once
- **Improve readability** - Tests focus on what's being tested
- **Flexibility** - Optional parameters allow customization
- **Maintainability** - Changes to setup only need to be made in one place

### Guidelines for Helper Methods:
- Use optional parameters with sensible defaults
- Use `Action<T>` callbacks for test-specific customization
- Keep the test intent clear - what's being tested should be obvious
- Don't hide the arrangement - critical setup should be visible in the test

## Testing with Authorization

When testing features that support authorization:

```csharp
[Fact]
public async Task WithoutAuthorization_AllowsAnonymousAccess()
{
    var client = CreateClient();
    var response = await client.GetAsync("/excos/api/status");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

[Fact]
public async Task WithAuthorization_RequiresAuthentication()
{
    var client = CreateClient(configureApp: app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRouting();
        app.UseEndpoints(endpoints => 
        {
            endpoints.MapExcos()
                    .RequireAuthorization(); // Apply authorization
        });
    });
    
    var response = await client.GetAsync("/excos/api/status");
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

## Assertions

### Use Clear, Focused Assertions

```csharp
// ✅ Good - Clear assertions
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

var content = await response.Content.ReadAsStringAsync();
var status = JsonSerializer.Deserialize<StatusResponse>(content);
Assert.NotNull(status);
Assert.True(status.IsHealthy);

// ❌ Bad - Vague assertion
Assert.True(response.IsSuccessStatusCode); // What specific code are we expecting?
```

### Avoid Unnecessary API Calls

If you've already asserted something earlier, don't repeat the call:

```csharp
// ❌ Bad - Redundant calls
var response1 = await client.GetAsync("/excos/api/status");
Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

var response2 = await client.GetAsync("/excos/api/status"); // Why call again?
var content = await response2.Content.ReadAsStringAsync();

// ✅ Good - Reuse response
var response = await client.GetAsync("/excos/api/status");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

var content = await response.Content.ReadAsStringAsync();
var status = JsonSerializer.Deserialize<StatusResponse>(content);
Assert.NotNull(status);
```

## Test Coverage

### What to Test

✅ **Do test:**
- Public API behavior
- HTTP status codes for different scenarios
- Response content and format
- Error handling
- Different hosting patterns (WebApplication, pure endpoints)
- Different configuration options (path prefixes, with/without auth)
- Edge cases and boundary conditions

❌ **Don't test:**
- Private implementation details
- Framework functionality (ASP.NET Core itself is already tested)
- Things that are obviously working (unless they're critical paths)

### Example Test Coverage

```csharp
// API Endpoint Tests
[Fact] public async Task GetStatus_Returns_Ok()
[Fact] public async Task GetStatus_Returns_Json_Content()
[Fact] public async Task GetStatus_WithError_Returns_Problem()

// Static File Tests
[Fact] public async Task StaticFile_Exists_Returns_Ok()
[Fact] public async Task StaticFile_NotFound_Returns_404()
[Fact] public async Task StaticFile_DefaultFile_Serves_IndexHtml()
[Fact] public async Task StaticFile_HasCorrectContentType()

// Path Prefix Tests
[Theory]
[InlineData("/excos")]
[InlineData("/custom")]
public async Task MapExcos_WorksWithDifferentPaths(string path)

// Authorization Tests
[Fact] public async Task WithoutAuth_AllowsAnonymousAccess()
[Fact] public async Task WithAuth_RequiresAuthentication()

// Hosting Pattern Tests
[Fact] public async Task WorksWithWebApplication()
[Fact] public async Task WorksWithPureEndpoints()
```

## Common Pitfalls

### ❌ Testing Implementation Details
```csharp
// Bad - Testing how it works, not what it does
[Fact]
public void ExcosFileProvider_UsesEmbeddedFileProvider()
{
    var provider = new ExcosEmbeddedFileProvider();
    Assert.IsType<EmbeddedFileProvider>(provider.InternalProvider); // Testing internals!
}
```

### ❌ Shared Mutable State
```csharp
// Bad - Shared state between tests
private static HttpClient _sharedClient; // Don't do this!

[Fact] public async Task Test1() { /* uses _sharedClient */ }
[Fact] public async Task Test2() { /* uses _sharedClient */ }
```

### ❌ Tests That Depend on Execution Order
```csharp
// Bad - Test2 depends on Test1 running first
[Fact] public void Test1_CreatesData() { /* creates data */ }
[Fact] public void Test2_UsesData() { /* assumes data exists */ }
```

### ❌ Vague Test Names
```csharp
[Fact] public async Task TestApi() // What about the API?
[Fact] public async Task Test1() // What is being tested?
```

## Best Practices Checklist

When writing integration tests:
- [ ] Are tests independent and can run in any order?
- [ ] Do test names clearly describe what's being tested and the expected outcome?
- [ ] Is common setup extracted into helper methods?
- [ ] Are both success and failure scenarios tested?
- [ ] Are edge cases and boundary conditions tested?
- [ ] Do tests focus on behavior, not implementation?
- [ ] Are assertions clear and specific?
- [ ] Is the test arranged in Arrange-Act-Assert pattern?
- [ ] Are tests organized in logical groups/classes by feature?
- [ ] Do tests verify HTTP status codes and response formats?

## References
- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [xUnit documentation](https://xunit.net/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
