using Microsoft.AspNetCore.Http;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Middleware for handling API requests in the Excos plugin.
/// </summary>
public class ExcosApiMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExcosOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcosApiMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The Excos plugin options.</param>
    public ExcosApiMiddleware(RequestDelegate next, ExcosOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>
    /// Processes an HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var apiPath = $"{_options.PathPrefix}{_options.ApiRoutePrefix}";

        if (path.StartsWith(apiPath, StringComparison.OrdinalIgnoreCase))
        {
            // Extract the API route
            var apiRoute = path.Substring(apiPath.Length);
            
            // Simple example: handle a status endpoint
            if (apiRoute.Equals("/status", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"running\",\"version\":\"1.0.0\"}");
                return;
            }

            // If no handler found, return 404
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"API endpoint not found\"}");
            return;
        }

        await _next(context);
    }
}
