using Microsoft.Extensions.FileProviders;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Internal wrapper for embedded file provider.
/// </summary>
internal class ExcosFileProvider : IExcosFileProvider
{
    /// <inheritdoc/>
    public IFileProvider FileProvider { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcosFileProvider"/> class.
    /// </summary>
    public ExcosFileProvider()
    {
        var assembly = typeof(ExcosFileProvider).Assembly;
        FileProvider = new EmbeddedFileProvider(assembly, ExcosConstants.EmbeddedResourceNamespace);
    }
}
