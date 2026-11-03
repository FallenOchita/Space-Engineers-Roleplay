namespace Nexus.Contracts;

public enum SpaceEngineersEdition
{
    SpaceEngineers1,
    SpaceEngineers2
}

public sealed record GameInstallation(
    SpaceEngineersEdition Edition,
    string DisplayName,
    string InstallPath,
    string DetectionSource);

public sealed record ServerHealth(
    string Service,
    string Version,
    DateTimeOffset UtcTime);
