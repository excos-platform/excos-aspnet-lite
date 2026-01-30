using Microsoft.AspNetCore.Http;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Default status endpoint for the Excos plugin.
/// </summary>
internal class StatusEndpoint : IApiEndpoint
{
    /// <inheritdoc/>
    public string Route => "/status";

    /// <inheritdoc/>
    public async Task HandleAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"running\",\"version\":\"1.0.0\"}");
    }
}
