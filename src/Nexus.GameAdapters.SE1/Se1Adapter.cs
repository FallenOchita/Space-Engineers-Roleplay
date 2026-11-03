using Nexus.GameAdapters.Abstractions;

namespace Nexus.GameAdapters.SE1;

/// <summary>SE1 integration boundary; process control is not enabled until API validation is complete.</summary>
public sealed class Se1Adapter : IGameServerAdapter
{
    public string AdapterId => "space-engineers-1";
    public string DisplayName => "Space Engineers";
    public bool SupportsDedicatedServer => false;

    public Task<AdapterValidationResult> ValidateInstallationAsync(string installationPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var valid = Directory.Exists(installationPath) &&
            (File.Exists(Path.Combine(installationPath, "SpaceEngineers.exe")) ||
             File.Exists(Path.Combine(installationPath, "SpaceEngineersDedicated.exe")));
        return Task.FromResult(new AdapterValidationResult(valid,
            valid ? "Recognized SE1 installation." : "SE1 executable not found."));
    }
}