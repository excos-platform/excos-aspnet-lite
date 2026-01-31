# Skill: ASP.NET Core Integration

## Overview
This skill covers how to integrate with ASP.NET Core, focusing on endpoint routing patterns and avoiding unnecessary middleware.

## Core Philosophy: Pure Endpoint Routing

**Excos.AspNetCore.Lite uses 100% endpoint routing, zero middleware.**

This design choice provides:
- **Better performance** - No middleware pipeline overhead for routes that don't need it
- **Explicit routing** - Clear, declarative route definitions
- **Composability** - Easy to apply policies (auth, CORS) to specific routes
- **Testability** - Endpoints can be tested without full middleware pipeline

### Endpoint Routing vs Middleware

**When to use endpoint routing (preferred):**
- Handling HTTP requests for specific paths
- Serving static files from specific routes
- Creating API endpoints
- Returning JSON, HTML, or file responses

**When middleware is actually needed (rare):**
- Cross-cutting concerns that apply to ALL requests (logging, exception handling)
- Request/response transformation that must happen before routing
- Authentication/authorization that can't be applied at route level

**Decision tree:**
```
Need to handle a request?
├─ For a specific path/pattern?
│  └─ ✅ Use endpoint routing (MapGet, MapPost, MapGroup, etc.)
└─ For ALL requests regardless of path?
   └─ ⚠️ Consider middleware (but check if endpoint routing + policies can work)
```

### Example: The Right Way

```csharp
// ✅ Good - Pure endpoint routing
public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string pathPrefix = "/excos")
{
    var group = endpoints.MapGroup(pathPrefix);
    
    // API endpoints
    var apiGroup = group.MapGroup("/api");
    apiGroup.MapGet("/status", async (IExcosStatusService status) => 
    {
        var result = await status.GetStatusAsync();
        return Results.Json(result);
    });
    
    // Static files for SPA
    group.MapGet("/app/{**path}", async (string path, IExcosFileProvider files) =>
    {
        var file = files.GetFileInfo(path);
        if (!file.Exists)
            return Results.NotFound();
            
        return Results.File(file.CreateReadStream(), GetContentType(path));
    });
    
    return group; // Allow host to apply policies
}

// Host can now apply authorization:
app.MapExcos("/admin")
   .RequireAuthorization("AdminPolicy");
```

## Endpoint Patterns

### Grouping Related Endpoints

Use `MapGroup` to organize related endpoints and avoid path duplication.

```csharp
// ✅ Good - Grouped endpoints
var apiGroup = group.MapGroup("/api");
apiGroup.MapGet("/status", HandleStatus);
apiGroup.MapGet("/health", HandleHealth);
apiGroup.MapPost("/command", HandleCommand);

// ❌ Bad - Duplicated path prefix
group.MapGet("/api/status", HandleStatus);
group.MapGet("/api/health", HandleHealth);
group.MapPost("/api/command", HandleCommand);
```

### Handler Methods

For complex endpoint logic, extract to static handler methods.

```csharp
// ✅ Good - Extracted handler with explicit dependencies
internal static class StatusHandlers
{
    public static async Task<IResult> GetStatus(
        IExcosStatusService statusService,
        ILogger<StatusHandlers> logger)
    {
        try
        {
            var status = await statusService.GetStatusAsync();
            return Results.Json(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get status");
            return Results.Problem("Failed to retrieve status");
        }
    }
}

// Registration
apiGroup.MapGet("/status", StatusHandlers.GetStatus);
```

**Benefits:**
- Dependencies are explicit (no hidden DI dependencies)
- Easy to test (can call handler directly with test doubles)
- Keeps endpoint registration code clean
- Handlers can be reused across different routes

### Using Native Result Types

Always use native ASP.NET Core result types - don't create custom abstractions.

```csharp
// ✅ Good - Native ASP.NET Core results
return Results.Json(data);
return Results.Ok();
return Results.NotFound();
return Results.Problem("Error message");
return Results.File(stream, contentType);
return Results.Redirect(url);

// ❌ Bad - Custom result abstraction
return new ExcosResult(data); // Unnecessary abstraction!
```

## Service Registration

### Prefer Singleton Lifetime

For services that don't hold per-request state, use singleton lifetime.

```csharp
// ✅ Good - Singleton services
public static IServiceCollection AddExcos(this IServiceCollection services)
{
    services.AddSingleton<IExcosFileProvider, ExcosEmbeddedFileProvider>();
    services.AddSingleton<IExcosContentTypeProvider, ExcosContentTypeProvider>();
    return services;
}

// ❌ Bad - Unnecessary scoped lifetime
services.AddScoped<IExcosFileProvider, ExcosEmbeddedFileProvider>(); // Creates new instance per request!
```

**When to use each lifetime:**
- `Singleton` - Stateless services, file providers, content type providers, configuration
- `Scoped` - Services that hold per-request state (DbContext, user context)
- `Transient` - Rarely needed; only for lightweight services created frequently

### Wrapping Framework Types

When registering framework types, wrap them in internal interfaces.

```csharp
// Internal interface
internal interface IExcosFileProvider
{
    IFileInfo GetFileInfo(string subpath);
}

// Internal wrapper
internal class ExcosEmbeddedFileProvider : IExcosFileProvider
{
    private readonly EmbeddedFileProvider _provider;
    
    public ExcosEmbeddedFileProvider()
    {
        _provider = new EmbeddedFileProvider(
            typeof(ExcosEmbeddedFileProvider).Assembly,
            "Excos.AspNetCore.Lite.wwwroot");
    }
    
    public IFileInfo GetFileInfo(string subpath) => _provider.GetFileInfo(subpath);
}

// Registration
services.AddSingleton<IExcosFileProvider, ExcosEmbeddedFileProvider>();
```

## Path Handling

### Consistent Path Prefixes

Make path prefix a parameter, not a configuration option.

```csharp
// ✅ Good - Path prefix as parameter
public static RouteGroupBuilder MapExcos(
    this IEndpointRouteBuilder endpoints, 
    string pathPrefix = "/excos")
{
    var group = endpoints.MapGroup(pathPrefix);
    // ...
}

// Usage
app.MapExcos("/admin/dashboard");
app.MapExcos(); // Uses default "/excos"
```

### Sub-paths

Use constants for sub-paths to avoid duplication and ensure frontend/backend alignment.

```csharp
internal static class ExcosConstants
{
    public const string ApiPrefix = "api";
    public const string AppPrefix = "app";
}

// Backend
var apiGroup = group.MapGroup(ExcosConstants.ApiPrefix);
var appGroup = group.MapGroup(ExcosConstants.AppPrefix);

// Frontend matches these constants
// fetch(`${basePath}/${ApiPrefix}/status`)
```

## Performance Considerations

### Avoid Unnecessary Abstraction Layers

Use framework features directly rather than creating custom abstractions.

```csharp
// ✅ Good - Direct framework usage
apiGroup.MapGet("/status", async (IExcosStatusService service) =>
{
    var status = await service.GetStatusAsync();
    return Results.Json(status);
});

// ❌ Bad - Unnecessary abstraction
apiGroup.MapGet("/status", async (IExcosStatusService service, IResultFactory factory) =>
{
    var status = await service.GetStatusAsync();
    return factory.CreateJsonResult(status); // Why wrap Results.Json?
});
```

### Static File Handling

For embedded static files, use endpoint routing instead of static file middleware.

```csharp
// ✅ Good - Endpoint-based static files
group.MapGet("/app/{**path}", async (
    string path,
    IExcosFileProvider fileProvider,
    IExcosContentTypeProvider contentTypeProvider) =>
{
    // Handle default file
    if (string.IsNullOrEmpty(path) || path == "/")
        path = "index.html";
    
    var file = fileProvider.GetFileInfo(path);
    if (!file.Exists)
        return Results.NotFound();
    
    var contentType = contentTypeProvider.GetContentType(path);
    return Results.File(file.CreateReadStream(), contentType);
});

// ❌ Bad - Static file middleware (doesn't fit our pure routing philosophy)
app.UseStaticFiles(new StaticFileOptions { ... }); // Applies to all paths!
```

## Testing Endpoint Routing

Endpoints can be tested with or without `WebApplication`:

```csharp
// Option 1: With WebApplicationFactory (integration test)
var factory = new WebApplicationFactory<Program>();
var client = factory.CreateClient();
var response = await client.GetAsync("/excos/api/status");

// Option 2: Pure endpoint testing with HostBuilder
var host = new HostBuilder()
    .ConfigureServices(services => services.AddExcos())
    .ConfigureWebHost(webHost =>
    {
        webHost.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapExcos());
        });
    })
    .Build();
```

## Common Pitfalls

### ❌ Creating Middleware When Endpoint Routing Suffices
```csharp
// Bad - Unnecessary middleware
public class ExcosMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWith("/excos"))
        {
            // Handle request...
        }
    }
}

// Should be endpoint routing instead!
group.MapGet("/excos", handler);
```

### ❌ Not Returning RouteGroupBuilder
```csharp
public static void MapExcos(this IEndpointRouteBuilder endpoints) // Returns void!
{
    // Host can't apply authorization or other policies
}
```

### ❌ Using Scoped Services for Stateless Logic
```csharp
services.AddScoped<IExcosFileProvider, ...>(); // Wasteful - creates new instance per request
```

### ❌ Hardcoding Paths
```csharp
apiGroup.MapGet("/api/status", ...); // Should use constant or avoid duplication with MapGroup
```

## Best Practices Checklist

When integrating with ASP.NET Core:
- [ ] Is endpoint routing used instead of middleware?
- [ ] Do extension methods return `RouteGroupBuilder` for host control?
- [ ] Are related endpoints grouped with `MapGroup`?
- [ ] Are complex handlers extracted to static methods with explicit dependencies?
- [ ] Are native ASP.NET Core result types used (no custom abstractions)?
- [ ] Are services registered with appropriate lifetimes (prefer singleton)?
- [ ] Are framework types wrapped in internal interfaces when registered in DI?
- [ ] Are path constants centralized to avoid duplication?
- [ ] Can the implementation be tested with pure endpoint routing (no WebApplication required)?

## References
- [Endpoint routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Route groups in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups)
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
