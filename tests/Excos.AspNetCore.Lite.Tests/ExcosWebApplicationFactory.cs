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
    }
}
