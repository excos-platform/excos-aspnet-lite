using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Excos.AspNetCore.Lite;

/// <summary>
/// Handler for serving static files for the Excos SPA.
/// </summary>
internal static class ExcosStaticFilesHandler
{
    /// <summary>
    /// Handles requests for static files.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="contentTypeProvider">The content type provider.</param>
    /// <param name="pathPrefix">The path prefix for the plugin.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task HandleAsync(
        HttpContext context,
        IExcosFileProvider fileProvider,
        IExcosContentTypeProvider contentTypeProvider,
        string pathPrefix)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Extract the request path relative to the plugin prefix
        var requestPath = path.Substring(pathPrefix.Length);
        if (string.IsNullOrEmpty(requestPath) || requestPath == "/")
        {
            requestPath = "/" + ExcosConstants.DefaultDocument;
        }

        var fileInfo = fileProvider.FileProvider.GetFileInfo(requestPath.TrimStart('/'));

        if (fileInfo.Exists)
        {
            context.Response.ContentType = GetContentType(requestPath, contentTypeProvider);
            using var stream = fileInfo.CreateReadStream();
            await stream.CopyToAsync(context.Response.Body);
            return;
        }

        // If file not found, serve default document (SPA routing)
        fileInfo = fileProvider.FileProvider.GetFileInfo(ExcosConstants.DefaultDocument);
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
    }

    private static string GetContentType(string path, IExcosContentTypeProvider contentTypeProvider)
    {
        if (contentTypeProvider.TryGetContentType(path, out var contentType))
        {
            return contentType!;
        }
        return "application/octet-stream";
    }
}
