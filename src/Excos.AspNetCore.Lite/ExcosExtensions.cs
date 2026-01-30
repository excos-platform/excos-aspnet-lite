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
    /// Adds Excos static files middleware and maps API endpoints.
    /// </summary>
    /// <param name="app">The application builder with endpoint routing.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// Ensure AddExcos is called during service configuration before calling this method.
    /// </remarks>
    public static IApplicationBuilder UseExcos(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<ExcosOptions>();

        // Add static files middleware
        app.UseMiddleware<ExcosStaticFilesMiddleware>();

        // Map API endpoints if endpoint routing is available
        if (app is IEndpointRouteBuilder endpointRouteBuilder)
        {
            var apiGroup = endpointRouteBuilder.MapGroup($"{options.PathPrefix}{ExcosConstants.ApiRoutePrefix}");
            apiGroup.MapGet("/status", () => Results.Json(new { status = "running", version = "1.0.0" }));
        }

        return app;
    }
}
