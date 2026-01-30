using Microsoft.AspNetCore.Http;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Defines an API endpoint handler for the Excos plugin.
/// </summary>
public interface IApiEndpoint
{
    /// <summary>
    /// Gets the route pattern for this endpoint (e.g., "/status").
    /// </summary>
    string Route { get; }

    /// <summary>
    /// Handles the API request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(HttpContext context);
}
