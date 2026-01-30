# Excos.AspNetCore.Lite

A lightweight plugin system for ASP.NET Core applications that provides both API middleware and Single Page Application (SPA) hosting capabilities, similar to Hangfire or Swagger UI.

![Excos Plugin UI](https://github.com/user-attachments/assets/2bc15c5d-776e-434b-896b-8b4af7e8ef49)

## Features

- **Embedded Static File Serving**: Serve SPA assets directly from embedded assembly resources
- **API Middleware**: Handle custom API endpoints with built-in middleware
- **Configurable Route Prefix**: Register the plugin at any route in your application
- **SPA Routing Support**: Fallback to index.html for client-side routing
- **Easy Integration**: Simple extension methods for ASP.NET Core applications
- **In-Memory Testing**: Includes a test server project for development and testing

## Project Structure

- **Excos.AspNetCore.Lite**: Main library containing the plugin infrastructure
- **Excos.AspNetCore.Lite.TestServer**: Test server for demonstrating and testing the plugin

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
    options.PathPrefix = "/excos";      // Plugin will be available at /excos
    options.ApiRoutePrefix = "/api";    // API endpoints at /excos/api
    options.DefaultDocument = "index.html";
});

var app = builder.Build();

// Register the Excos plugin middleware
app.UseExcos();

app.Run();
```

### Configuration Options

The `ExcosOptions` class provides the following configuration properties:

- **PathPrefix**: The path where the plugin will be hosted (default: `/excos`)
- **ApiRoutePrefix**: The API route prefix relative to PathPrefix (default: `/api`)
- **DefaultDocument**: The default document name for the SPA (default: `index.html`)

### Accessing the Plugin

Once registered, the plugin provides:

- **SPA Interface**: `http://localhost:5000/excos/`
- **API Endpoints**: `http://localhost:5000/excos/api/status`

![API Status Response](https://github.com/user-attachments/assets/c23d6ba7-10d8-4aaf-989c-1bfc9950c348)

## Architecture

### Middleware Components

1. **ExcosStaticFilesMiddleware**: Serves static files from embedded resources
   - Handles requests for HTML, CSS, JavaScript, and other static assets
   - Implements SPA routing fallback to the default document
   
2. **ExcosApiMiddleware**: Processes API requests
   - Routes API calls to appropriate handlers
   - Returns JSON responses
   - Currently includes a `/status` endpoint example

### Static Assets

Static assets are embedded in the assembly and served from the `wwwroot` directory:
- `index.html`: Main SPA interface
- `styles.css`: Styling
- `app.js`: Client-side JavaScript

## Development

### Building the Solution

```bash
cd /path/to/excos-aspnet-lite
dotnet build
```

### Running the Test Server

```bash
cd src/Excos.AspNetCore.Lite.TestServer
dotnet run
```

Then navigate to `http://localhost:5000/excos/` in your browser.

### Adding Custom API Endpoints

To add custom API endpoints, modify the `ExcosApiMiddleware.cs` file:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var path = context.Request.Path.Value ?? string.Empty;
    var apiPath = $"{_options.PathPrefix}{_options.ApiRoutePrefix}";

    if (path.StartsWith(apiPath, StringComparison.OrdinalIgnoreCase))
    {
        var apiRoute = path.Substring(apiPath.Length);
        
        // Add your custom endpoint
        if (apiRoute.Equals("/your-endpoint", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\":\"Hello from custom endpoint\"}");
            return;
        }
    }

    await _next(context);
}
```

### Customizing the SPA

The SPA interface can be customized by modifying files in the `src/Excos.AspNetCore.Lite/wwwroot/` directory:

- `index.html`: Structure and content
- `styles.css`: Styling and layout
- `app.js`: Client-side behavior and API interactions

## License

This project is part of the Excos platform.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

