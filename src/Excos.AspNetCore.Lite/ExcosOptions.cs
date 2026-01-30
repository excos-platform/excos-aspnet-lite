namespace Excos.AspNetCore.Lite;

/// <summary>
/// Configuration options for the Excos plugin.
/// </summary>
public class ExcosOptions
{
    private string _pathPrefix = "/excos";

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
}
