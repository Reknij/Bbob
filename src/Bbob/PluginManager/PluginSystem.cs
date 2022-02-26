using Bbob.Main.BuildInPlugin;
using Bbob.Plugin;
using System.Reflection;
using System.Text.Json;

namespace Bbob.Main.PluginManager;

public static class PluginSystem
{
    public static readonly string pluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins"); //plugins in base of Bbob directory.
    public static readonly string configsFolder = Path.Combine(Environment.CurrentDirectory, "configs");
    static List<PluginAssemblyLoadContext> thirdPlugins = new List<PluginAssemblyLoadContext>();

    static List<IPlugin> buildInPlugins = new List<IPlugin>();

    public static void LoadAllPlugins()
    {
        Directory.CreateDirectory(configsFolder);
        System.Console.WriteLine("Loading Plugin System...");
        PluginHelper.CurrentDirectory = Environment.CurrentDirectory;
        PluginHelper.BaseDirectory = AppContext.BaseDirectory;
        PluginHelper.clearAllObject();
        LoadBuildInPlugins();
        LoadThirdPlugins();
        System.Console.WriteLine($"Loaded {AllPluginCount} plugins. ({BuildInPluginCount}|{ThirdPluginCount})");
    }

    private static void LoadBuildInPlugins()
    {
        var config = Configuration.ConfigManager.GetConfigManager().MainConfig;
        buildInPlugins.Clear();

        string buildInTypesPath = "Bbob.Main.BuildInPlugin.";
        Assembly main = Assembly.GetExecutingAssembly();
        Type[] types = main.GetTypes();
        foreach (var plugin in config.buildInPlugins)
        {
            foreach (var type in types)
            {
                if (type.FullName == null) continue;
                if (type.FullName.StartsWith(buildInTypesPath) && plugin == type.Name)
                {
                    var p = (IPlugin?)Activator.CreateInstance(type);
                    if (p == null) continue;
                    buildInPlugins.Add(p);
                    System.Console.WriteLine($"Loaded build-in plugin <{type.Name}>");
                }
            }
        }

        if (buildInPlugins.Count == 0) System.Console.WriteLine("Warning: No build-in plugins are loaded.");
    }
    private static void LoadThirdPlugins()
    {
        Directory.CreateDirectory(pluginDirectory);
        thirdPlugins.Clear();
        string[] folders = Directory.GetDirectories(pluginDirectory);
        foreach (string folder in folders)
        {
            string pluginJsonPath = Path.Combine(folder, "plugin.json");
            if (File.Exists(pluginJsonPath))
            {
                using (FileStream fs = new FileStream(pluginJsonPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    PluginJson? pluginInfo = JsonSerializer.Deserialize<PluginJson>(fs);
                    if (pluginInfo == null)
                    {
                        System.Console.WriteLine("Plugin json file can't loaded.");
                        break;
                    }
                    try
                    {
                        var a = pluginInfo.name;
                    }
                    catch (NullReferenceException)
                    {
                        pluginInfo.name = Path.GetDirectoryName(folder) ?? $"UnknownPluginName.{Path.GetRandomFileName()}";
                    }
                    string pluginDll = Path.Combine(folder, pluginInfo.entry);
                    var mainPlugin = new PluginAssemblyLoadContext(pluginDll, pluginInfo);
                    if (mainPlugin.Plugin != null)
                    {
                        System.Console.WriteLine($"Loaded third plugin <{pluginInfo.name}>");
                        thirdPlugins.Add(mainPlugin);
                    }
                }
            }
        }
    }

    public delegate void CyclePluginDelegate(IPlugin plugin);
    public static void cyclePlugins(CyclePluginDelegate cyclePluginDelegate)
    {
        List<IPlugin> requireRunAgain = new List<IPlugin>();
        foreach (var p in buildInPlugins)
        {
            InitializeExecutingPlugin(p);
            cyclePluginDelegate?.Invoke(p);
            if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.RunMeAgain) requireRunAgain.Add(p);
            if (!checkCommandResult()) return;
        }
        foreach (var p in thirdPlugins)
        {
            if (p.Plugin == null) continue;
            InitializeExecutingPlugin(p);
            cyclePluginDelegate?.Invoke(p.Plugin);
            if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.RunMeAgain) requireRunAgain.Add(p.Plugin);
            if (!checkCommandResult()) return;
        }
        while (requireRunAgain.Count > 0)
        {
            for (int i = requireRunAgain.Count; i >= 0; i--)
            {
                PluginHelper.ExecutingPlugin = getPluginInfo(requireRunAgain[i]);
                System.Console.WriteLine($"Run again <{PluginHelper.ExecutingPlugin.name}>");
                System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                cyclePluginDelegate?.Invoke(requireRunAgain[i]);
                if (PluginHelper.ExecutingCommandResult.Operation != CommandOperation.RunMeAgain) requireRunAgain.RemoveAt(i);
                if (!checkCommandResult()) return;
            }
        }
    }

    private static void InitializeExecutingPlugin(IPlugin plugin)=>InitializeExecutingPlugin(getPluginInfo(plugin));
    private static void InitializeExecutingPlugin(PluginAssemblyLoadContext pContent)=>InitializeExecutingPlugin(pContent.PluginInfo);
    private static void InitializeExecutingPlugin(PluginJson info)
    {
        PluginHelper.ExecutingPlugin = info;
        PluginHelper.ExecutingCommandResult = new CommandResult();
    }
    
    private static bool checkCommandResult()
    {
        if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Stop ||
            PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Skip)
            return false;
        return true;
    }

    public static PluginJson getPluginInfo(IPlugin plugin)
    {
        Type type = plugin.GetType();
        PluginJson info = new PluginJson();
        info.name = type.Name;
        info.author = "Bbob";
        info.description = "This is build-in plugin.";
        info.repository = "No have repository";

        return info;
    }

    public static IPlugin? GetThirdPlugin(int index) => thirdPlugins?[index].Plugin;
    public static int AllPluginCount => buildInPlugins.Count + thirdPlugins.Count;
    public static int BuildInPluginCount => buildInPlugins.Count;
    public static int ThirdPluginCount => thirdPlugins.Count;
}