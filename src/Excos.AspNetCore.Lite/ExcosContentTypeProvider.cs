using Microsoft.AspNetCore.StaticFiles;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Internal wrapper for content type provider.
/// </summary>
internal class ExcosContentTypeProvider : IExcosContentTypeProvider
{
    private readonly FileExtensionContentTypeProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcosContentTypeProvider"/> class.
    /// </summary>
    public ExcosContentTypeProvider()
    {
        _provider = new FileExtensionContentTypeProvider();
    }

    /// <inheritdoc/>
    public bool TryGetContentType(string path, out string? contentType)
    {
        return _provider.TryGetContentType(path, out contentType);
    }
}
