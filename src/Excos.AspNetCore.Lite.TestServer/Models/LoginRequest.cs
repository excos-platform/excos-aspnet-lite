namespace Excos.AspNetCore.Lite.TestServer.Models;

/// <summary>
/// Request model for login endpoint.
/// </summary>
public record LoginRequest(string Username, string Password);
