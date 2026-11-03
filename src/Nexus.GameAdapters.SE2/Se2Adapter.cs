using Nexus.GameAdapters.Abstractions;

namespace Nexus.GameAdapters.SE2;

/// <summary>SE2 integration boundary; executable/API validation remains a follow-up task.</summary>
public sealed class Se2Adapter : IGameServerAdapter
{
    public string AdapterId => "space-engineers-2";
    public string DisplayName => "Space Engineers 2";
    public bool SupportsDedicatedServer => false;

    public Task<AdapterValidationResult> ValidateInstallationAsync(string installationPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var valid = Directory.Exists(installationPath);
        return Task.FromResult(new AdapterValidationResult(valid,
            valid ? "SE2 directory found; executable validation is pending." : "SE2 installation directory not found."));
    }
}