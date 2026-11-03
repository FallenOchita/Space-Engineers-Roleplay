using Nexus.Contracts;
using Nexus.GameDetection;
using Nexus.Server.ServerInstances;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SpaceEngineersDetector>();
builder.Services.AddSingleton<ServerInstanceStore>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new ServerHealth(
    "Nexus.Server",
    typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.1.0",
    DateTimeOffset.UtcNow)));

app.MapGet("/api/v1/games/installed", (SpaceEngineersDetector detector) =>
    Results.Ok(new
    {
        machine = Environment.MachineName,
        platform = OperatingSystem.IsWindows() ? "windows" :
                   OperatingSystem.IsLinux() ? "linux" :
                   OperatingSystem.IsMacOS() ? "macos" : "unknown",
        installations = detector.Detect()
    }));

app.MapGet("/api/v1/server-instances", (ServerInstanceStore store) =>
    Results.Ok(store.GetAll()));

app.MapGet("/api/v1/server-instances/{id:guid}", (Guid id, ServerInstanceStore store) =>
{
    var instance = store.Get(id);
    return instance is null ? Results.NotFound() : Results.Ok(instance);
});

app.MapPost("/api/v1/server-instances", (CreateServerInstanceRequest request, ServerInstanceStore store) =>
{
    var (instance, error) = store.Register(request);
    return instance is null
        ? Results.BadRequest(new { error })
        : Results.Created($"/api/v1/server-instances/{instance.Id}", instance);
});

app.MapDelete("/api/v1/server-instances/{id:guid}", (Guid id, ServerInstanceStore store) =>
    store.Remove(id) ? Results.NoContent() : Results.NotFound());

app.Run();

public partial class Program { }
