using Excos.AspNetCore.Lite;

var builder = WebApplication.CreateBuilder(args);

// Add Excos services
builder.Services.AddExcos(options =>
{
    options.PathPrefix = "/excos";
});

// Add authentication services (for demonstration of applying policies to plugin endpoints)
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Add a simple root endpoint
app.MapGet("/", () => "Test server is running. Visit /excos to see the Excos plugin.");

// Map the Excos plugin - returns API route group for further customization
var excosApi = app.MapExcos();

// Example: Apply authorization to all plugin API endpoints (commented out for demo)
// excosApi.RequireAuthorization();

// Example: Add custom endpoint to the plugin API
excosApi.MapGet("/custom", () => Results.Json(new { message = "Custom endpoint added by host" }));

// Example: Add an authenticated endpoint
excosApi.MapGet("/secure", () => Results.Json(new { message = "This endpoint requires authentication" }))
    .RequireAuthorization();

app.Run();
