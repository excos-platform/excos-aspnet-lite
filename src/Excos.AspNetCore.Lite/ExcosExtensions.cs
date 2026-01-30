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
    private static readonly string[] HttpMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };

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

        // Register default status endpoint
        services.AddSingleton<IApiEndpoint, StatusEndpoint>();

        // Register internal services in a way that doesn't leak to consumer
        services.AddSingleton<IExcosFileProvider, ExcosFileProvider>();
        services.AddSingleton<IExcosContentTypeProvider, ExcosContentTypeProvider>();

        return services;
    }

    /// <summary>
    /// Adds Excos middleware and maps API endpoints to the application pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    /// <remarks>
    /// Ensure AddExcos is called during service configuration before calling this method.
    /// This method adds static file middleware and maps all registered IApiEndpoint implementations.
    /// </remarks>
    public static WebApplication UseExcos(this WebApplication app)
    {
        // Add static files middleware
        app.UseMiddleware<ExcosStaticFilesMiddleware>();

        // Map API endpoints using native routing
        MapApiEndpoints(app);

        return app;
    }

    /// <summary>
    /// Adds Excos middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// Ensure AddExcos is called during service configuration before calling this method.
    /// This overload is provided for compatibility with testing scenarios using WebApplicationFactory.
    /// Note: API endpoints will need to be mapped separately using endpoint routing.
    /// </remarks>
    public static IApplicationBuilder UseExcos(this IApplicationBuilder app)
    {
        // Add static files middleware
        app.UseMiddleware<ExcosStaticFilesMiddleware>();

        // If this is a WebApplication, also map endpoints
        if (app is WebApplication webApp)
        {
            MapApiEndpoints(webApp);
        }
        else
        {
            // Fall back to middleware for non-WebApplication scenarios
            app.UseMiddleware<ExcosApiMiddleware>();
        }

        return app;
    }

    private static void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<ExcosOptions>();
        var apiEndpoints = app.ServiceProvider.GetServices<IApiEndpoint>();

        var apiGroup = app.MapGroup($"{options.PathPrefix}{ExcosConstants.ApiRoutePrefix}");

        foreach (var endpoint in apiEndpoints)
        {
            // Use a factory method to avoid closure over loop variable
            var handler = CreateEndpointHandler(endpoint);
            apiGroup.MapMethods(endpoint.Route, HttpMethods, handler);
        }
    }

    private static Func<HttpContext, Task> CreateEndpointHandler(IApiEndpoint endpoint)
    {
        // Static delegate factory - avoids allocating a new closure for each endpoint
        return context => endpoint.HandleAsync(context);
    }
}
