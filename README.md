# Excos.AspNetCore.Lite

A lightweight plugin system for ASP.NET Core applications that provides both API endpoints and Single Page Application (SPA) hosting capabilities, similar to Hangfire or Swagger UI.

![Excos Plugin UI](https://github.com/user-attachments/assets/2bc15c5d-776e-434b-896b-8b4af7e8ef49)

## Features

- **Embedded Static File Serving**: Serve SPA assets directly from embedded assembly resources
- **Native Endpoint Routing**: API endpoints use ASP.NET Core's native endpoint routing for optimal performance
- **Extensible API**: Register custom API endpoints through dependency injection
- **Configurable Route Prefix**: Mount the plugin at any route in your application
- **SPA Routing Support**: Fallback to index.html for client-side routing
- **Easy Integration**: Simple extension methods for ASP.NET Core applications
- **Comprehensive Testing**: xUnit test suite with in-memory WebApplicationFactory testing
- **Internal Implementation**: All middleware and implementations are internal to prevent API surface pollution
- **.NET 10**: Built on the latest .NET framework

## Project Structure

- **Excos.AspNetCore.Lite**: Main library containing the plugin infrastructure
- **Excos.AspNetCore.Lite.TestServer**: Demo server showing plugin integration
- **Excos.AspNetCore.Lite.Tests**: xUnit test suite with 9 passing tests

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

// Register the Excos plugin middleware and map API endpoints
app.UseExcos();

app.Run();
```

### Configuration Options

The `ExcosOptions` class provides the following configuration:

- **PathPrefix**: The path where the plugin will be hosted (default: `/excos`)

Note: API route (`/api`) and default document (`index.html`) are hardcoded for consistency with the embedded SPA.

### Accessing the Plugin

Once registered, the plugin provides:

- **SPA Interface**: `http://localhost:5202/excos/` (or your configured port)
- **API Endpoints**: `http://localhost:5202/excos/api/status`

![API Status Response](https://github.com/user-attachments/assets/c23d6ba7-10d8-4aaf-989c-1bfc9950c348)

## Architecture

### Components

1. **ExcosStaticFilesMiddleware** (internal): Serves static files from embedded resources
   - Handles requests for HTML, CSS, JavaScript, and other static assets
   - Implements SPA routing fallback to index.html
   - Uses wrapped singleton services to prevent DI container pollution
   
2. **Native Endpoint Routing**: API endpoints use ASP.NET Core's MapGroup
   - Routes API calls to registered `IApiEndpoint` implementations
   - Better performance than custom middleware
   - Follows ASP.NET Core conventions
   - Returns JSON responses
   - Extensible via dependency injection

### API Endpoint Extensibility

The plugin uses the `IApiEndpoint` interface to allow custom API endpoints:

```csharp
public interface IApiEndpoint
{
    string Route { get; }  // e.g., "/status"
    Task HandleAsync(HttpContext context);
}
```

### Static Assets

Static assets are embedded in the assembly and served from the `wwwroot` directory:
- `index.html`: Main SPA interface
- `styles.css`: Styling
- `app.js`: Client-side JavaScript

## Extending the Plugin

### Adding Custom API Endpoints

Create a class that implements `IApiEndpoint` and register it with dependency injection:

```csharp
using Excos.AspNetCore.Lite;
using Microsoft.AspNetCore.Http;

public class CustomEndpoint : IApiEndpoint
{
    public string Route => "/custom";

    public async Task HandleAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"Custom endpoint\"}");
    }
}

// In your Program.cs or Startup.cs:
builder.Services.AddExcos(options =>
{
    options.PathPrefix = "/excos";
});

// Register your custom endpoint
builder.Services.AddSingleton<IApiEndpoint, CustomEndpoint>();

app.UseExcos();
```

Your custom endpoint will be available at `/excos/api/custom`.

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

All 9 tests use WebApplicationFactory for in-memory testing, ensuring the plugin works correctly without requiring a running server.

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

