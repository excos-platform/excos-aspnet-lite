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
        Action<ExcosOptions>? configure = null)
    {
        var options = new ExcosOptions();
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
    /// <param name="pathPrefix">The path prefix where the plugin will be mounted (e.g., "/excos"). If null, uses the configured PathPrefix from ExcosOptions.</param>
    /// <returns>A route group builder for the API endpoints, allowing the host to apply authorization or other policies.</returns>
    /// <remarks>
    /// This method adds static file middleware and returns a RouteGroupBuilder for the API endpoints.
    /// The returned builder allows the host to apply policies like RequireAuthorization() or add custom endpoints.
    /// </remarks>
    public static RouteGroupBuilder MapExcos(this IEndpointRouteBuilder endpoints, string? pathPrefix = null)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<ExcosOptions>();
        var prefix = pathPrefix ?? options.PathPrefix;

        // Add static files middleware if this is IApplicationBuilder
        if (endpoints is IApplicationBuilder app)
        {
            app.UseMiddleware<ExcosStaticFilesMiddleware>();
        }

        // Create and return the API route group
        var apiGroup = endpoints.MapGroup($"{prefix}{ExcosConstants.ApiRoutePrefix}");

        // Map default status endpoint
        apiGroup.MapGet("/status", () => Results.Json(new { status = "running", version = "1.0.0" }));

        return apiGroup;
    }
}
