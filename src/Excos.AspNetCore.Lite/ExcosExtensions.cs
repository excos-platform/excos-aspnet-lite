using Microsoft.AspNetCore.Builder;
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
        
        return services;
    }

    /// <summary>
    /// Adds Excos middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// Ensure AddExcos is called during service configuration before calling this method.
    /// </remarks>
    public static IApplicationBuilder UseExcos(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<ExcosOptions>() ?? new ExcosOptions();
        
        // Add static files middleware first
        app.UseMiddleware<ExcosStaticFilesMiddleware>(options);
        
        // Add API middleware
        app.UseMiddleware<ExcosApiMiddleware>(options);
        
        return app;
    }
}
