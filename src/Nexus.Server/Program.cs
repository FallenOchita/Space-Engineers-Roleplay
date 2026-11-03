using Nexus.Contracts;
using Nexus.GameDetection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SpaceEngineersDetector>();

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

app.Run();

public partial class Program { }
