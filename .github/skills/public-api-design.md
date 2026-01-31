# Skill: Public API Design for Libraries

## Overview
This skill focuses on designing clean, minimal public APIs for .NET libraries that will be consumed by other applications.

## Core Principles

### Minimal API Surface
- **Keep the public API surface minimal** - only expose what consumers absolutely need
- Every public type, method, and property is a commitment to maintain forever
- Think carefully before making something public - can it be internal instead?
- Favor composition over inheritance in public APIs to reduce coupling

**Example:**
```csharp
// ❌ Bad - Exposing internal implementation
public class ExcosPlugin
{
    public EmbeddedFileProvider FileProvider { get; } // Internal detail leaked!
    public FileExtensionContentTypeProvider ContentTypeProvider { get; } // Internal detail leaked!
}

// ✅ Good - Minimal, focused API
public static class ExcosApplicationBuilderExtensions
{
    public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string pathPrefix = "/excos")
    {
        // Implementation details stay internal
    }
}
```

### Make Implementation Details Internal
- Use `internal` as your default access modifier for implementation classes
- Only make types `public` when they are part of the public contract
- This prevents consumers from depending on internal implementation details
- Makes refactoring easier since you can change internals without breaking consumers

**When to use each access modifier:**
- `public` - Core API that consumers interact with (extension methods, options classes, core abstractions)
- `internal` - Implementation classes, helpers, internal services
- `private` - Class-specific implementation details

### Prevent DI Container Pollution
When your library registers services in the DI container, avoid "leaking" framework or internal types into the consumer's container.

**Problem:** If you register framework types directly, they become available to the consumer's entire application, which can cause:
- Naming conflicts
- Confusion about service ownership
- Difficulty understanding what services come from where

**Solution:** Wrap framework types in internal interfaces

```csharp
// ❌ Bad - Leaks framework type into consumer's DI
public static IServiceCollection AddExcos(this IServiceCollection services)
{
    services.AddSingleton<EmbeddedFileProvider>(...); // Now available to entire app!
    return services;
}

// ✅ Good - Wraps in internal interface
internal interface IExcosFileProvider
{
    IFileInfo GetFileInfo(string subpath);
}

internal class ExcosEmbeddedFileProvider : IExcosFileProvider
{
    private readonly EmbeddedFileProvider _provider;
    // Implementation wraps EmbeddedFileProvider
}

public static IServiceCollection AddExcos(this IServiceCollection services)
{
    services.AddSingleton<IExcosFileProvider, ExcosEmbeddedFileProvider>();
    return services;
}
```

### Return Types that Enable Control
When creating extension methods that map endpoints, return types that enable the host application to apply additional configuration.

```csharp
// ✅ Good - Returns RouteGroupBuilder for host control
public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string pathPrefix = "/excos")
{
    var group = endpoints.MapGroup(pathPrefix);
    // Map endpoints to group...
    return group; // Caller can now do: .RequireAuthorization(), .WithMetadata(), etc.
}

// Usage by consumer:
app.MapExcos("/admin")
   .RequireAuthorization("AdminOnly"); // Host can add authorization
```

## Configuration Design

### Options Pattern
Use the Options pattern for configuration that should be customizable by consumers.

```csharp
public class ExcosOptions
{
    public bool EnableDetailedErrors { get; set; } = false;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
}
```

### When NOT to Make Things Configurable
Don't create options for:
1. **Fundamental behavior** - Things that define how the plugin works
2. **Frontend compile-time values** - Things the embedded SPA needs to know at compile time
3. **Breaking changes** - Settings that would break core functionality if changed

**Example:**
```csharp
// ❌ Bad - These shouldn't be configurable
public class ExcosOptions
{
    public string ApiPrefix { get; set; } = "/api"; // Frontend needs to know this at compile time!
    public string StaticFilesPrefix { get; set; } = "/app"; // Would break the SPA!
}

// ✅ Good - Make path prefix a parameter, not an option
public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string pathPrefix = "/excos")
{
    // Path prefix is set once at startup, both frontend and backend know it
}
```

### Constants vs Configuration
- **Use constants** for values that define the plugin's architecture
- **Use configuration** for values that users might reasonably want to change
- **Centralize constants** in dedicated constant classes to avoid duplication

```csharp
// ✅ Good - Centralized constants
internal static class ExcosConstants
{
    public const string ApiRoutePrefix = "api";
    public const string StaticFilesPrefix = "app";
    public const string DefaultPathPrefix = "/excos";
}

// ✅ Good - Truly configurable options
public class ExcosOptions
{
    public bool EnableMetrics { get; set; } = true;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}
```

## Common Pitfalls

### ❌ Exposing Internal Framework Types
```csharp
public class ExcosPlugin
{
    public EmbeddedFileProvider FileProvider { get; } // Consumer now depends on framework type
}
```

### ❌ Making Everything Configurable
```csharp
public class ExcosOptions
{
    public string AppTitle { get; set; } = "Excos"; // React needs this at compile time!
}
```

### ❌ Not Returning Control to Host
```csharp
public static void MapExcos(this IEndpointRouteBuilder endpoints) // Returns void!
{
    // Host can't apply authorization or other policies
}
```

### ❌ Public Implementation Classes
```csharp
public class ExcosApiHandler // Should be internal!
{
    public Task<IResult> HandleStatusAsync() { ... }
}
```

## Best Practices Checklist

When designing a public API:
- [ ] Is every public member necessary? Can any be internal?
- [ ] Are framework types wrapped in internal interfaces when registered in DI?
- [ ] Do extension methods return types that enable host control?
- [ ] Is the Options class reserved for truly configurable settings?
- [ ] Are architectural constants centralized and not configurable?
- [ ] Is the API intuitive and self-documenting?
- [ ] Does the API prevent common misuse patterns?
- [ ] Are public types documented with XML comments?

## References
- [ASP.NET Core Middleware vs Endpoint Routing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [.NET API Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
