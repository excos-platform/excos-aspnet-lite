# Excos.AspNetCore.Lite

A lightweight plugin system for ASP.NET Core applications that provides both API endpoints and Single Page Application (SPA) hosting capabilities, similar to Hangfire or Swagger UI.

![Excos Plugin UI](https://github.com/user-attachments/assets/2bc15c5d-776e-434b-896b-8b4af7e8ef49)

## Features

- **Embedded Static File Serving**: Serve SPA assets directly from embedded assembly resources
- **Native Endpoint Routing**: Uses ASP.NET Core's native MapGroup and MapGet for optimal performance
- **Minimal Configuration**: Only the mount path is configurable
- **SPA Routing Support**: Fallback to index.html for client-side routing
- **Easy Integration**: Single extension method call
- **Comprehensive Testing**: xUnit test suite with in-memory WebApplicationFactory testing
- **Clean API Surface**: All implementations are internal
- **.NET 10**: Built on the latest .NET framework

## Project Structure

- **Excos.AspNetCore.Lite**: Main library containing the plugin infrastructure
- **Excos.AspNetCore.Lite.TestServer**: Demo server showing plugin integration
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

// Register the Excos plugin - automatically maps static files and API endpoints
app.UseExcos();

app.Run();
```

### Configuration Options

The `ExcosOptions` class provides minimal configuration:

- **PathPrefix**: The path where the plugin will be hosted (default: `/excos`)

Note: API route (`/api`) and default document (`index.html`) are hardcoded constants.

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

