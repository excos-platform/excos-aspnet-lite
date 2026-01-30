namespace Excos.AspNetCore.Lite;

/// <summary>
/// Configuration options for the Excos plugin.
/// </summary>
public class ExcosOptions
{
    /// <summary>
    /// Gets or sets the path prefix where the plugin will be hosted.
    /// Default is "/excos".
    /// </summary>
    public string PathPrefix { get; set; } = "/excos";

    /// <summary>
    /// Gets or sets the API route prefix relative to PathPrefix.
    /// Default is "/api".
    /// </summary>
    public string ApiRoutePrefix { get; set; } = "/api";

    /// <summary>
    /// Gets or sets the default document name served for the SPA.
    /// Default is "index.html".
    /// </summary>
    public string DefaultDocument { get; set; } = "index.html";
}
