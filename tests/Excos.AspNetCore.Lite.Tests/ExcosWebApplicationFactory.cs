using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Excos.AspNetCore.Lite.Tests;

/// <summary>
/// Custom web application factory for testing the Excos plugin.
/// </summary>
public class ExcosWebApplicationFactory : WebApplicationFactory<TestWebApplication>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
        
        builder.ConfigureServices(services =>
        {
            // Add Excos services
            services.AddExcos(options =>
            {
                options.PathPrefix = "/excos";
            });
        });
        
        builder.Configure(app =>
        {
            // Register the Excos plugin middleware
            app.UseExcos();
        });
    }
}
