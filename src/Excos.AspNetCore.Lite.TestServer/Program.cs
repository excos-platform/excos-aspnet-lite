using Excos.AspNetCore.Lite;

var builder = WebApplication.CreateBuilder(args);

// Add Excos services
builder.Services.AddExcos(options =>
{
    options.PathPrefix = "/excos";
});

var app = builder.Build();

// Add a simple root endpoint
app.MapGet("/", () => "Test server is running. Visit /excos to see the Excos plugin.");

// Register the Excos plugin - it will map the status endpoint automatically
app.UseExcos();

app.Run();
