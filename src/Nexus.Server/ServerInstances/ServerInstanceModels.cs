using Nexus.Contracts;

namespace Nexus.Server.ServerInstances;

public enum ServerInstanceState
{
    Registered, Starting, Running, Stopping, Stopped, Faulted
}

public sealed record ServerInstance(
    Guid Id,
    string Name,
    SpaceEngineersEdition Edition,
    string InstallationPath,
    string WorldPath,
    ServerInstanceState State,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateServerInstanceRequest(
    string Name,
    SpaceEngineersEdition Edition,
    string InstallationPath,
    string WorldPath);
