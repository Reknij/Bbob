using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Reflection;

[assembly: InternalsVisibleTo("Bbob")]
[assembly: InternalsVisibleTo("Bbob.Tests")]
namespace Bbob.Plugin.Cores;

internal static class PluginHelperCore
{
    internal static Dictionary<string, object?> pluginsObject = new Dictionary<string, object?>();
    internal static ConcurrentDictionary<string, JsonCache> pluginConfigCaches = new ();
    internal static JsonCache? themeInfoCache = null;
    internal static Dictionary<string, object> metas = new Dictionary<string, object>();
    internal static HashSet<string> pluginsDone = new HashSet<string>();
    internal static Dictionary<string, PluginJson> pluginsLoaded = new(StringComparer.OrdinalIgnoreCase);
    internal static string? hashPluginsLoaded = null;
    internal static Dictionary<string, Dictionary<string, Action<string[]>>> customCommands = new(StringComparer.OrdinalIgnoreCase);
    internal static string customGlobalCommandKey = "";
    internal static ConfigJson? configBbob;
    internal static string themePath = "null";
    internal static string currentDirectory = "null";
    internal static string baseDirectory = "null";
    internal volatile static PluginJson? executingPlugin;

    internal record class JsonCache(string content, DateTime lastModified);
}