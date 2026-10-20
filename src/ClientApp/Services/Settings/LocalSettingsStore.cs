using System.Text.Json;

namespace eShop.ClientApp.Services.Settings;

// Avalonia has no built-in equivalent to MAUI's Preferences/SecureStorage (both backed by
// platform-native stores: Android SharedPreferences, iOS NSUserDefaults/Keychain, Windows
// DPAPI). Real, working persistence is only implemented here for Desktop (a JSON file under
// LocalApplicationData) -- Android/iOS/Browser fall back to in-memory-only (lost on app
// restart), a known, flagged gap rather than real per-platform native storage, to revisit when
// those platforms get real attention. SecureStorage's replacement reuses this same store too --
// NOT actually encrypted, a second flagged gap, not a silent security downgrade.
internal static class LocalSettingsStore
{
    private static readonly Dictionary<string, string> Cache = new();
    private static readonly string? FilePath = GetFilePath();
    private static bool _loaded;

    public static string? Get(string key)
    {
        EnsureLoaded();
        return Cache.GetValueOrDefault(key);
    }

    public static void Set(string key, string? value)
    {
        EnsureLoaded();

        if (value is null)
        {
            Cache.Remove(key);
        }
        else
        {
            Cache[key] = value;
        }

        Save();
    }

    private static string? GetFilePath()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsLinux())
        {
            return null;
        }

        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eShop");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "settings.json");
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        if (FilePath is null || !File.Exists(FilePath))
        {
            return;
        }

        var json = File.ReadAllText(FilePath);
        var values = JsonSerializer.Deserialize(json, EShopJsonSerializerContext.Default.DictionaryStringString);

        if (values is not null)
        {
            foreach (var (key, value) in values)
            {
                Cache[key] = value;
            }
        }
    }

    private static void Save()
    {
        if (FilePath is null)
        {
            return;
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(Cache, EShopJsonSerializerContext.Default.DictionaryStringString));
    }
}
