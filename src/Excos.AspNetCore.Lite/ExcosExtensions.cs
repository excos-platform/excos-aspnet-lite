using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Extension methods for registering the Excos plugin.
/// </summary>
public static class ExcosExtensions
{
    /// <summary>
    /// Adds Excos services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional action to configure the Excos options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddExcos(
        this IServiceCollection services,
        Action<ExcosLiteOptions>? configure = null)
    {
        var options = new ExcosLiteOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        // Register internal services
        services.AddSingleton<IExcosFileProvider, ExcosFileProvider>();
        services.AddSingleton<IExcosContentTypeProvider, ExcosContentTypeProvider>();

        return services;
    }

    /// <summary>
    /// Maps Excos plugin endpoints including static files and API.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pathPrefix">The path prefix where the plugin will be mounted (default: "/excos").</param>
    /// <returns>A route group builder for all plugin endpoints, allowing the host to apply authorization or other policies.</returns>
    /// <remarks>
    /// This method maps both static file endpoints and API endpoints within a route group.
    /// The returned RouteGroupBuilder allows the host to apply policies like RequireAuthorization() to all plugin endpoints.
    /// </remarks>
    public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string pathPrefix = "/excos")
    {
        var fileProvider = endpoints.ServiceProvider.GetRequiredService<IExcosFileProvider>();
        var contentTypeProvider = endpoints.ServiceProvider.GetRequiredService<IExcosContentTypeProvider>();

        // Create a route group for the entire plugin
        var pluginGroup = endpoints.MapGroup(pathPrefix);

        // Map catch-all route for static files (non-API routes) within the plugin group
        pluginGroup.MapGet("/{**path}", async (HttpContext context, string? path) =>
        {
            // Skip API routes - they should not match this catch-all
            var requestPath = context.Request.Path.Value ?? string.Empty;
            if (requestPath.StartsWith($"{pathPrefix}{ExcosConstants.ApiRoutePrefix}", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 404;
                return;
            }

            await ExcosStaticFilesHandler.HandleAsync(context, fileProvider, contentTypeProvider, pathPrefix);
        });

        // Create API sub-group within the plugin group
        var apiGroup = pluginGroup.MapGroup(ExcosConstants.ApiRoutePrefix);

        // Map default status endpoint
        apiGroup.MapGet("/status", () => Results.Json(new { status = "running", version = "1.0.0" }));

        // Return the parent group so policies apply to all endpoints
        return pluginGroup;
    }
}
