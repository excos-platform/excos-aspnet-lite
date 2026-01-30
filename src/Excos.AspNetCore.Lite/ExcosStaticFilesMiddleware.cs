using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Middleware for serving static files for the Excos SPA.
/// </summary>
public class ExcosStaticFilesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExcosOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcosStaticFilesMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The Excos plugin options.</param>
    public ExcosStaticFilesMiddleware(
        RequestDelegate next, 
        ExcosOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>
    /// Processes an HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Check if the request is for the plugin path
        if (path.StartsWith(_options.PathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Skip API requests
            var apiPath = $"{_options.PathPrefix}{_options.ApiRoutePrefix}";
            if (path.StartsWith(apiPath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Serve static file or default document
            var assembly = typeof(ExcosStaticFilesMiddleware).Assembly;
            var embeddedProvider = new EmbeddedFileProvider(assembly, "Excos.AspNetCore.Lite.wwwroot");
            
            var requestPath = path.Substring(_options.PathPrefix.Length);
            if (string.IsNullOrEmpty(requestPath) || requestPath == "/")
            {
                requestPath = "/" + _options.DefaultDocument;
            }

            var fileInfo = embeddedProvider.GetFileInfo(requestPath.TrimStart('/'));
            
            if (fileInfo.Exists)
            {
                context.Response.ContentType = GetContentType(requestPath);
                using var stream = fileInfo.CreateReadStream();
                await stream.CopyToAsync(context.Response.Body);
                return;
            }

            // If file not found and it's not an API request, serve default document (SPA routing)
            fileInfo = embeddedProvider.GetFileInfo(_options.DefaultDocument);
            if (fileInfo.Exists)
            {
                context.Response.ContentType = "text/html";
                using var stream = fileInfo.CreateReadStream();
                await stream.CopyToAsync(context.Response.Body);
                return;
            }

            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not found");
            return;
        }

        await _next(context);
    }

    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType(path, out var contentType))
        {
            return contentType;
        }
        return "application/octet-stream";
    }
}
