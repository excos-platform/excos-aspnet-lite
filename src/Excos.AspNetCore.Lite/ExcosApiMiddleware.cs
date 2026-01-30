using Microsoft.AspNetCore.Http;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Middleware for handling API requests in the Excos plugin.
/// </summary>
public class ExcosApiMiddleware
{
    private const string ApiRoutePrefix = "/api";
    private readonly RequestDelegate _next;
    private readonly ExcosOptions _options;
    private readonly IEnumerable<IApiEndpoint> _endpoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcosApiMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The Excos plugin options.</param>
    /// <param name="endpoints">The registered API endpoints.</param>
    public ExcosApiMiddleware(RequestDelegate next, ExcosOptions options, IEnumerable<IApiEndpoint> endpoints)
    {
        _next = next;
        _options = options;
        _endpoints = endpoints;
    }

    /// <summary>
    /// Processes an HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var apiPath = $"{_options.PathPrefix}{ApiRoutePrefix}";

        if (path.StartsWith(apiPath, StringComparison.OrdinalIgnoreCase))
        {
            // Extract the API route
            var apiRoute = path.Substring(apiPath.Length);
            
            // Find matching endpoint
            var endpoint = _endpoints.FirstOrDefault(e => 
                e.Route.Equals(apiRoute, StringComparison.OrdinalIgnoreCase));

            if (endpoint != null)
            {
                await endpoint.HandleAsync(context);
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
