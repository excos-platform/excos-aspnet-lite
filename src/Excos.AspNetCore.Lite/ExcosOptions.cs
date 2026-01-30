namespace Excos.AspNetCore.Lite;

/// <summary>
/// Configuration options for the Excos plugin.
/// </summary>
public class ExcosOptions
{
    private string _pathPrefix = "/excos";
    private string _apiRoutePrefix = "/api";

    /// <summary>
    /// Gets or sets the path prefix where the plugin will be hosted.
    /// Default is "/excos".
    /// </summary>
    public string PathPrefix
    {
        get => _pathPrefix;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("PathPrefix cannot be null or empty", nameof(value));
            if (!value.StartsWith("/"))
                throw new ArgumentException("PathPrefix must start with '/'", nameof(value));
            _pathPrefix = value.TrimEnd('/');
        }
    }

    /// <summary>
    /// Gets or sets the API route prefix relative to PathPrefix.
    /// Default is "/api".
    /// </summary>
    public string ApiRoutePrefix
    {
        get => _apiRoutePrefix;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ApiRoutePrefix cannot be null or empty", nameof(value));
            if (!value.StartsWith("/"))
                throw new ArgumentException("ApiRoutePrefix must start with '/'", nameof(value));
            _apiRoutePrefix = value.TrimEnd('/');
        }
    }

    /// <summary>
    /// Gets or sets the default document name served for the SPA.
    /// Default is "index.html".
    /// </summary>
    public string DefaultDocument { get; set; } = "index.html";
}
