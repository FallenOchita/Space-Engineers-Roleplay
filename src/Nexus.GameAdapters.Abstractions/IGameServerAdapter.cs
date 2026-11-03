namespace Nexus.GameAdapters.Abstractions;

public interface IGameServerAdapter
{
    string AdapterId { get; }
    string DisplayName { get; }
    bool SupportsDedicatedServer { get; }

    Task<AdapterValidationResult> ValidateInstallationAsync(
        string installationPath, CancellationToken cancellationToken = default);
}

public sealed record AdapterValidationResult(bool IsValid, string Message);
