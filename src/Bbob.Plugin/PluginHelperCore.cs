using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bbob")]
[assembly: InternalsVisibleTo("Bbob.Tests")]
namespace Bbob.Plugin.Cores;

internal static class PluginHelperCore
{
    internal static Dictionary<string, object?> pluginsObject = new Dictionary<string, object?>();
    internal static Dictionary<string, object> metas = new Dictionary<string, object>();
    internal static HashSet<string> pluginsDone = new HashSet<string>();
    internal static Dictionary<string, PluginJson> pluginsLoaded = new();
    internal static Dictionary<string, Dictionary<string, Action<string[]>>> customCommands = new();
    internal static string customGlobalCommandKey = "";
    internal static ConfigJson? configBbob;
    internal static string themePath = "null";
    internal static string currentDirectory = "null";
    internal static string baseDirectory = "null";
    internal static PluginJson? executingPlugin;
}