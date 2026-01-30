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
    private readonly EmbeddedFileProvider _embeddedProvider;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

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
        
        // Set up embedded file provider once
        var assembly = typeof(ExcosStaticFilesMiddleware).Assembly;
        _embeddedProvider = new EmbeddedFileProvider(assembly, "Excos.AspNetCore.Lite.wwwroot");
        _contentTypeProvider = new FileExtensionContentTypeProvider();
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
            var requestPath = path.Substring(_options.PathPrefix.Length);
            if (string.IsNullOrEmpty(requestPath) || requestPath == "/")
            {
                requestPath = "/" + _options.DefaultDocument;
            }

            var fileInfo = _embeddedProvider.GetFileInfo(requestPath.TrimStart('/'));
            
            if (fileInfo.Exists)
            {
                context.Response.ContentType = GetContentType(requestPath);
                using var stream = fileInfo.CreateReadStream();
                await stream.CopyToAsync(context.Response.Body);
                return;
            }

            // If file not found and it's not an API request, serve default document (SPA routing)
            fileInfo = _embeddedProvider.GetFileInfo(_options.DefaultDocument);
            if (fileInfo.Exists)
            {
                context.Response.ContentType = "text/html";
                using var stream = fileInfo.CreateReadStream();
                await stream.CopyToAsync(context.Response.Body);
                return;
            }

            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Not found\"}");
            return;
        }

        await _next(context);
    }

    private string GetContentType(string path)
    {
        if (_contentTypeProvider.TryGetContentType(path, out var contentType))
        {
            return contentType;
        }
        return "application/octet-stream";
    }
}
