using Microsoft.Extensions.FileProviders;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Internal interface for Excos file provider to prevent service leakage.
/// </summary>
internal interface IExcosFileProvider
{
    /// <summary>
    /// Gets the underlying file provider.
    /// </summary>
    IFileProvider FileProvider { get; }
}
