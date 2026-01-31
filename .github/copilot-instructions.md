# Copilot Instructions for Excos.AspNetCore.Lite

This document provides guidelines for working on the Excos.AspNetCore.Lite codebase.

## Project Overview

Excos.AspNetCore.Lite is a lightweight plugin system for ASP.NET Core that provides both API endpoints and SPA hosting capabilities, similar to Hangfire or Swagger UI. The project uses **pure endpoint routing** (100% endpoint routing, zero middleware) for optimal performance.

## Target Framework

- Always target **.NET 10** (`net10.0`) for all projects
- Use the latest .NET 10 features and APIs

## Architecture Principles

### Pure Endpoint Routing
- **Never use middleware** - use endpoint routing exclusively via `MapGet`, `MapGroup`, etc.
- All static file serving and API routing must be done through endpoints, not middleware
- Use catch-all endpoint patterns like `{prefix}/{**path}` for static file serving
- Return `RouteGroupBuilder` from mapping methods to enable host control

### Host Control Pattern
- Extension methods that map plugin endpoints should return `RouteGroupBuilder`
- This allows the host application to apply policies like `.RequireAuthorization()`
- Example pattern:
  ```csharp
  var excosApi = app.MapExcos("/excos");
  excosApi.RequireAuthorization();  // Host can apply policies
  ```

### Internal Implementation
- Keep implementation details **internal** - only expose what's necessary for consumers
- Public API surface should be minimal and well-defined
- Use internal interfaces to wrap framework types (e.g., `IExcosFileProvider`, `IExcosContentTypeProvider`)
- This prevents "leaking" framework services into consumer's DI container

### Service Registration
- Wrap singleton services like `EmbeddedFileProvider` and `FileExtensionContentTypeProvider` in internal interfaces
- Register wrapped services to avoid polluting consumer's DI container
- Services should be registered once and injected where needed

### Constants
- Create dedicated constants classes (e.g., `ExcosConstants`) to centralize hardcoded values
- Avoid duplicating constants across multiple files
- Keep API routes, default documents, and other fixed values as constants
- Configuration that shouldn't change (like `/api` suffix or `index.html`) should be hardcoded constants, not options

## Code Organization

### Options Pattern
- Keep options classes even if currently empty - they're reserved for future configuration
- Don't add options for things that:
  - The embedded frontend needs to know at compile time (e.g., API route)
  - Are fundamental to the plugin's operation (e.g., default document)
- Path prefixes and other host-level concerns should be method parameters, not options

### File Structure
- Main library: `src/Excos.AspNetCore.Lite/`
- Test server (demo): `src/Excos.AspNetCore.Lite.TestServer/`
- Tests: `tests/Excos.AspNetCore.Lite.Tests/`

## Testing

### Test Framework
- Use **xUnit** for all tests
- Use `WebApplicationFactory<T>` for in-memory integration testing
- For purely endpoint-based scenarios, use `HostBuilder` with `UseEndpoints` pattern

### Test Organization
- Group related tests in dedicated test classes
- Create separate test classes for:
  - WebApplication-based tests (using `WebApplicationFactory`)
  - Pure endpoint routing tests (using `HostBuilder` and `UseEndpoints`)
- Aim for at least 8-12 tests covering all major scenarios

### Test Helper Methods
- **Extract repeated code into helper methods** to reduce duplication
- Tests should be short and easy to read
- Use lambda parameters in helpers to allow test-specific customization
- Example helper pattern:
  ```csharp
  private static HttpClient CreateTestClient(Action<RouteGroupBuilder>? configureEndpoints = null)
  {
      // Setup code...
      var excosApi = endpoints.MapExcos("/excos");
      configureEndpoints?.Invoke(excosApi);
      // Return client...
  }
  ```

### Test Coverage
- Test both successful and failure scenarios
- Test authentication/authorization when `RequireAuthorization()` is applied
- Test SPA routing fallback to `index.html`
- Test static file serving for various content types
- Test API endpoints with proper status codes and responses
- Ensure tests work with purely endpoint-based consumers (no `IApplicationBuilder` dependency)

### Assertions
- Remove unnecessary API calls if assertions are already made earlier in the test
- Keep assertions focused and clear
- Use meaningful assertion messages when helpful

## Code Quality

### Code Style
- Follow standard C# conventions
- Use file-scoped namespaces
- Prefer expression-bodied members for simple properties and accessors
- Use implicit object creation when type is apparent

### Comments
- Add XML documentation comments for public APIs
- Internal implementations don't need extensive comments if code is self-documenting
- Comment complex logic or non-obvious decisions

### Performance
- Prefer singleton services over per-request instantiation
- Use native ASP.NET Core features for optimal performance
- Avoid custom abstraction layers when framework features suffice

## Building and Testing

### Commands
```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Check code formatting
dotnet format --verify-no-changes
```

### CI Requirements
All pull requests must pass:
1. **Build** - `dotnet build` must succeed with no errors
2. **Tests** - All tests must pass with `dotnet test`
3. **Formatting** - Code must be properly formatted per `.editorconfig`

## Specific Code Patterns

### Static File Handling
- Use static methods with explicit service parameters
- Example: `ExcosStaticFilesHandler.HandleAsync(HttpContext, IExcosFileProvider, IExcosContentTypeProvider)`
- Map as endpoint: `app.MapGet("{prefix}/{**path}", (HttpContext ctx) => Handler.HandleAsync(...))`

### API Endpoints
- Use native `MapGet`, `MapPost`, etc. with `Results.Json()` and other result types
- Group related endpoints with `MapGroup`
- No custom endpoint abstractions - use framework features directly

### Authentication Demo
- Test server can demonstrate basic authentication
- Use `RequireAuthorization()` on the returned `RouteGroupBuilder` to protect endpoints

## Common Pitfalls to Avoid

1. ❌ Creating middleware when endpoints would suffice
2. ❌ Making configuration options for things that shouldn't be configurable
3. ❌ Exposing framework types directly in public API
4. ❌ Duplicating constants across files
5. ❌ Creating per-request instances of services that could be singletons
6. ❌ Repeating code in tests instead of using helper methods
7. ❌ Adding custom abstractions when native ASP.NET Core features work fine
8. ❌ Relying on `IApplicationBuilder` when targeting purely endpoint-based consumers

## Questions?

When in doubt:
- Prefer simplicity and native framework features
- Keep the public API surface minimal
- Use endpoint routing over middleware
- Add tests to verify your changes work correctly
