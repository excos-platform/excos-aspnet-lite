using Microsoft.AspNetCore.StaticFiles;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Internal interface for Excos content type provider to prevent service leakage.
/// </summary>
public interface IExcosContentTypeProvider
{
    /// <summary>
    /// Tries to get the content type for a file path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="contentType">The content type if found.</param>
    /// <returns>True if content type was found, false otherwise.</returns>
    bool TryGetContentType(string path, out string? contentType);
}
