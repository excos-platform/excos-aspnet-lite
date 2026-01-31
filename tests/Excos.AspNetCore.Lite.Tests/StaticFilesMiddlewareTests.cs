using System.Net;

namespace Excos.AspNetCore.Lite.Tests;

/// <summary>
/// Tests for the Excos static files middleware.
/// </summary>
public class StaticFilesMiddlewareTests : IClassFixture<ExcosWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StaticFilesMiddlewareTests(ExcosWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootPath_ReturnsIndexHtml()
    {
        // Act
        var response = await _client.GetAsync("/excos/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task IndexHtml_ContainsExpectedContent()
    {
        // Act
        var response = await _client.GetAsync("/excos/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Excos", content);
        Assert.Contains("<!DOCTYPE html>", content);
    }

    [Fact]
    public async Task JsFile_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/excos/app.js");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // JavaScript can be served as application/javascript or text/javascript
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.True(contentType == "application/javascript" || contentType == "text/javascript");
    }

    [Fact]
    public async Task SourceMapFile_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/excos/app.js.map");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Source maps can be served as application/json or text/plain
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.True(contentType == "application/json" || contentType == "text/plain");
    }

    [Fact]
    public async Task NonExistentFile_FallsBackToIndexHtml()
    {
        // Act - SPA routing should serve index.html for non-existent paths
        var response = await _client.GetAsync("/excos/some/route");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<!DOCTYPE html>", content);
    }
}
