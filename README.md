# Excos.AspNetCore.Lite

A lightweight plugin system for ASP.NET Core applications that provides both API endpoints and Single Page Application (SPA) hosting capabilities, similar to Hangfire or Swagger UI.

![Excos Plugin UI](https://github.com/user-attachments/assets/2bc15c5d-776e-434b-896b-8b4af7e8ef49)

## Features

- **Embedded Static File Serving**: Serve SPA assets directly from embedded assembly resources
- **Native Endpoint Routing**: Uses ASP.NET Core's native MapGroup for optimal performance
- **Host Control**: Returns RouteGroupBuilder allowing host to apply authorization and other policies
- **Extensible**: Add custom endpoints to the plugin's API group
- **SPA Routing Support**: Fallback to index.html for client-side routing
- **Easy Integration**: Single extension method call
- **Comprehensive Testing**: xUnit test suite with in-memory WebApplicationFactory testing
- **Clean API Surface**: All implementations are internal
- **.NET 10**: Built on the latest .NET framework

## Project Structure

- **Excos.AspNetCore.Lite**: Main library containing the plugin infrastructure
- **Excos.AspNetCore.Lite.TestServer**: Demo server showing plugin integration with authentication
- **Excos.AspNetCore.Lite.Tests**: xUnit test suite with 8 passing tests

## Getting Started

### Installation

Add a reference to the `Excos.AspNetCore.Lite` project in your ASP.NET Core application.

### Basic Usage

```csharp
using Excos.AspNetCore.Lite;

var builder = WebApplication.CreateBuilder(args);

// Add Excos services
builder.Services.AddExcos(options =>
{
    options.PathPrefix = "/excos";  // Plugin will be available at /excos
});

var app = builder.Build();

// Map the Excos plugin - returns API route group for customization
var excosApi = app.MapExcos();

app.Run();
```

### Applying Authorization

The `MapExcos` method returns a `RouteGroupBuilder`, allowing you to apply authorization or other policies:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExcos(options => options.PathPrefix = "/excos");
builder.Services.AddAuthentication(...).AddScheme(...);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map plugin and apply authorization to all API endpoints
var excosApi = app.MapExcos();
excosApi.RequireAuthorization();

app.Run();
```

### Adding Custom Endpoints

You can add custom endpoints to the plugin's API group:

```csharp
var excosApi = app.MapExcos();

// Add custom endpoint
excosApi.MapGet("/custom", () => Results.Json(new { message = "Custom data" }));

// Add authenticated endpoint
excosApi.MapGet("/secure", () => Results.Json(new { data = "secret" }))
    .RequireAuthorization();
```

### Configuration Options

The `ExcosOptions` class provides minimal configuration:

- **PathPrefix**: The path where the plugin will be hosted (default: `/excos`)

Note: API route (`/api`) and default document (`index.html`) are hardcoded constants.

### Accessing the Plugin

Once registered, the plugin provides:

- **SPA Interface**: `http://localhost:5202/excos/` (or your configured port)
- **API Endpoints**: 
  - `http://localhost:5202/excos/api/status` (built-in)
  - Any custom endpoints you add to the returned RouteGroupBuilder

![API Status Response](https://github.com/user-attachments/assets/c23d6ba7-10d8-4aaf-989c-1bfc9950c348)

## Architecture

### Components

1. **ExcosStaticFilesMiddleware** (internal): Serves static files from embedded resources
   - Handles requests for HTML, CSS, JavaScript, and other static assets
   - Implements SPA routing fallback to index.html
   - Uses wrapped singleton services to prevent DI container pollution
   
2. **Direct Endpoint Mapping**: Status endpoint mapped directly using native routing
   - Uses ASP.NET Core's `MapGet` with `Results.Json()`
   - No custom abstraction layer - just native framework features
   - Optimal performance

### Static Assets

Static assets are embedded in the assembly and served from the `wwwroot` directory:
- `index.html`: Main SPA interface
- `styles.css`: Styling
- `app.js`: Client-side JavaScript

- `styles.css`: Styling
- `app.js`: Client-side JavaScript

## Development

### Building the Solution

```bash
cd /path/to/excos-aspnet-lite
dotnet build
```

### Running Tests

```bash
dotnet test
```

All 8 tests use WebApplicationFactory for in-memory testing, ensuring the plugin works correctly without requiring a running server.

### Running the Test Server

```bash
cd src/Excos.AspNetCore.Lite.TestServer
dotnet run
```

Then navigate to `http://localhost:5202/excos/` in your browser (or the configured port from launchSettings.json).

### Customizing the SPA

The SPA interface can be customized by modifying files in the `src/Excos.AspNetCore.Lite/wwwroot/` directory:

- `index.html`: Structure and content
- `styles.css`: Styling and layout
- `app.js`: Client-side behavior and API interactions

## License

This project is part of the Excos platform.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

