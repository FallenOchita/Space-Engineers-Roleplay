using Microsoft.Win32;
using Nexus.Contracts;

namespace Nexus.GameDetection;

/// <summary>Read-only discovery of Space Engineers installs in local Steam libraries.</summary>
public sealed class SpaceEngineersDetector
{
    private const string Se1AppId = "244850";
    // Confirmed app-id/folder mapping will be verified against the supported SE2 release.
    private const string Se2AppId = "1133870";

    public IReadOnlyList<GameInstallation> Detect()
    {
        var results = new List<GameInstallation>();
        var roots = GetSteamLibraryRoots().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        AddIfPresent(results, SpaceEngineersEdition.SpaceEngineers1, "Space Engineers", roots, Se1AppId, "SpaceEngineers");
        AddIfPresent(results, SpaceEngineersEdition.SpaceEngineers2, "Space Engineers 2", roots, Se2AppId, "SpaceEngineers2");

        return results
            .GroupBy(x => (x.Edition, Path.GetFullPath(x.InstallPath)))
            .Select(g => g.First())
            .ToArray();
    }

    private static void AddIfPresent(
        ICollection<GameInstallation> results,
        SpaceEngineersEdition edition,
        string displayName,
        IEnumerable<string> roots,
        string appId,
        string folderName)
    {
        foreach (var root in roots)
        {
            var manifest = Path.Combine(root, "steamapps", $"appmanifest_{appId}.acf");
            var candidate = Path.Combine(root, "steamapps", "common", folderName);
            if (Directory.Exists(candidate))
                results.Add(new GameInstallation(edition, displayName, Path.GetFullPath(candidate),
                    File.Exists(manifest) ? "Steam library + app manifest" : "Steam library folder"));
        }
    }

    private static IEnumerable<string> GetSteamLibraryRoots()
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (OperatingSystem.IsWindows())
        {
            AddRegistryPath(candidates, Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath");
            AddRegistryPath(candidates, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");
            AddRegistryPath(candidates, Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath");
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrWhiteSpace(pf)) candidates.Add(Path.Combine(pf, "Steam"));
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates.Add(Path.Combine(home, ".steam", "steam"));
            candidates.Add(Path.Combine(home, ".local", "share", "Steam"));
            candidates.Add(Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"));
        }

        foreach (var steamRoot in candidates.ToArray())
        {
            if (!Directory.Exists(steamRoot)) continue;
            yield return steamRoot;

            var libraryFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile)) continue;
            foreach (var line in File.ReadLines(libraryFile))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("\"path\"", StringComparison.OrdinalIgnoreCase)) continue;
                var parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var path = parts[1].Replace("\\\\", "\\");
                    if (Directory.Exists(path)) yield return path;
                }
            }
        }
    }

    private static void AddRegistryPath(ISet<string> candidates, RegistryKey hive, string keyPath, string valueName)
    {
        try
        {
            using var key = hive.OpenSubKey(keyPath);
            var value = key?.GetValue(valueName)?.ToString();
            if (!string.IsNullOrWhiteSpace(value)) candidates.Add(value.Replace('/', Path.DirectorySeparatorChar));
        }
        catch (System.Security.SecurityException) { }
        catch (UnauthorizedAccessException) { }
    }
}
