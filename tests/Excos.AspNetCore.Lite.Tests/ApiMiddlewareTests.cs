using System.Net;
using System.Net.Http.Json;

namespace Excos.AspNetCore.Lite.Tests;

/// <summary>
/// Tests for the Excos API middleware.
/// </summary>
public class ApiMiddlewareTests : IClassFixture<ExcosWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiMiddlewareTests(ExcosWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StatusEndpoint_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/excos/api/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StatusEndpoint_ReturnsExpectedJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/excos/api/status");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("\"status\"", content);
        Assert.Contains("\"version\"", content);
        Assert.Contains("running", content);
    }

    [Fact]
    public async Task NonExistentEndpoint_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/excos/api/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
