using System.Text.Json;
using Nexus.Contracts;

namespace Nexus.Server.ServerInstances;

/// <summary>
/// Persistent local registry. This milestone registers instances but does not launch game processes.
/// </summary>
public sealed class ServerInstanceStore
{
    private readonly string _filePath;
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private List<ServerInstance> _instances;

    public ServerInstanceStore(IHostEnvironment environment)
    {
        var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = string.IsNullOrWhiteSpace(localData)
            ? Path.Combine(environment.ContentRootPath, "data")
            : Path.Combine(localData, "Nexus", "ServerManager");

        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "server-instances.json");
        _instances = Load();
    }

    public IReadOnlyList<ServerInstance> GetAll()
    {
        lock (_sync)
            return _instances.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public ServerInstance? Get(Guid id)
    {
        lock (_sync)
            return _instances.FirstOrDefault(x => x.Id == id);
    }

    public (ServerInstance? Instance, string? Error) Register(CreateServerInstanceRequest request)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 64)
            return (null, "Name is required and must be 64 characters or fewer.");

        if (!Enum.IsDefined(request.Edition))
            return (null, "Unsupported Space Engineers edition.");

        if (!TryNormalizeExistingDirectory(request.InstallationPath, out var installPath))
            return (null, "InstallationPath must point to an existing directory on this host.");

        if (!TryNormalizeDirectory(request.WorldPath, out var worldPath))
            return (null, "WorldPath must be a valid absolute directory path.");

        lock (_sync)
        {
            if (_instances.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                return (null, "An instance with that name already exists.");

            var instance = new ServerInstance(
                Guid.NewGuid(), name, request.Edition, installPath, worldPath,
                ServerInstanceState.Registered, DateTimeOffset.UtcNow);

            _instances.Add(instance);
            Save();
            return (instance, null);
        }
    }

    public bool Remove(Guid id)
    {
        lock (_sync)
        {
            var instance = _instances.FirstOrDefault(x => x.Id == id);
            if (instance is null)
                return false;

            _instances.Remove(instance);
            Save();
            return true;
        }
    }

    private List<ServerInstance> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<ServerInstance>>(
                File.ReadAllText(_filePath), _jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            var backup = _filePath + ".invalid-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            File.Move(_filePath, backup, overwrite: false);
            return [];
        }
    }

    private void Save()
    {
        var temporaryPath = _filePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(_instances, _jsonOptions));
        File.Move(temporaryPath, _filePath, overwrite: true);
    }

    private static bool TryNormalizeExistingDirectory(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (!TryNormalizeDirectory(value, out var candidate) || !Directory.Exists(candidate))
            return false;

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeDirectory(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value))
            return false;

        try
        {
            normalized = Path.GetFullPath(value);
            return true;
        }
        catch (ArgumentException) { return false; }
        catch (NotSupportedException) { return false; }
        catch (PathTooLongException) { return false; }
    }
}
