using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Excos.AspNetCore.Lite.Tests;

/// <summary>
/// Tests for purely endpoint-based usage without IApplicationBuilder.
/// These tests ensure the plugin works with minimal endpoint routing configurations.
/// </summary>
public class PureEndpointRoutingTests
{
    private static async Task<HttpClient> CreateTestClient(
        string pathPrefix = "/excos",
        Action<IEndpointRouteBuilder>? configureEndpoints = null)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddExcos();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            var excosApi = endpoints.MapExcos(pathPrefix);
                            configureEndpoints?.Invoke(endpoints);
                        });
                    });
            })
            .StartAsync();

        return host.GetTestClient();
    }

    [Fact]
    public async Task MapExcos_WithMinimalEndpointRouting_MapsStaticFiles()
    {
        // Arrange
        var client = await CreateTestClient();

        // Act
        var response = await client.GetAsync("/excos/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task MapExcos_WithMinimalEndpointRouting_MapsApiStatus()
    {
        // Arrange
        var client = await CreateTestClient();

        // Act
        var response = await client.GetAsync("/excos/api/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\"", content);
        Assert.Contains("running", content);
    }

    [Fact]
    public async Task MapExcos_WithCustomPathPrefix_WorksCorrectly()
    {
        // Arrange
        var client = await CreateTestClient("/custom");

        // Act
        var staticResponse = await client.GetAsync("/custom/");
        var apiResponse = await client.GetAsync("/custom/api/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, staticResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
    }

    [Fact]
    public async Task MapExcos_ReturnsRouteGroupBuilder_AllowingPolicyApplication()
    {
        // Arrange & Act - This test verifies MapExcos returns a RouteGroupBuilder that supports policy methods
        var client = await CreateTestClient(configureEndpoints: endpoints =>
        {
            var excosApi = endpoints.MapExcos();

            // Verify the returned object is a RouteGroupBuilder by calling its methods
            // This ensures the pattern app.MapExcos().RequireAuthorization() compiles and registers
            Assert.NotNull(excosApi);
            Assert.IsAssignableFrom<RouteGroupBuilder>(excosApi);
        });

        // The assertion above already verified the type, so we're done
    }
}
