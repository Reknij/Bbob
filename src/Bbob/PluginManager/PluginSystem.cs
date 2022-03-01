using Bbob.Main.BuildInPlugin;
using Bbob.Plugin;
using System.Reflection;
using System.Text.Json;
using static Bbob.Plugin.ConfigJson;

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
        LoadBuildInPlugins();
        LoadThirdPlugins();
        if ((BuildInPluginCount + ThirdPluginCount) > 0) System.Console.WriteLine($"Loaded {AllPluginCount} plugins. ({BuildInPluginCount}|{ThirdPluginCount})");
        else System.Console.WriteLine("Warning: plugins are loaded.");
    }

    private static Type[] GetBuildInPlugins()
    {
        var config = Configuration.ConfigManager.GetConfigManager().MainConfig;
        string buildInTypesPath = "Bbob.Main.BuildInPlugin.";
        Assembly main = Assembly.GetExecutingAssembly();
        Type[] types = main.GetTypes();
        List<Type> bip = new List<Type>();
        foreach (var type in types)
        {
            if (type.FullName == null) continue;
            if (type.FullName.StartsWith(buildInTypesPath) && type.GetInterface("IPlugin") == typeof(IPlugin))
            {
                bip.Add(type);
            }
        }
        return bip.ToArray();
    }
    private static List<KeyValuePair<string, PluginJson>> GetThirdPluginsInfo()
    {
        List<KeyValuePair<string, PluginJson>> infos = new ();
        Directory.CreateDirectory(pluginDirectory);
        thirdPlugins.Clear();
        Action<string> addInfo = (folder) =>
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
                        return;
                    }
                    try
                    {
                        var a = pluginInfo.name;
                    }
                    catch (NullReferenceException)
                    {
                        pluginInfo.name = Path.GetDirectoryName(folder) ?? $"UnknownPluginName.{Path.GetRandomFileName()}";
                    }
                    infos.Add(new KeyValuePair<string, PluginJson>(folder, pluginInfo));
                }
            }
        };
        string[] folders = Directory.GetDirectories(pluginDirectory);
        foreach (string folder in folders)
        {
            addInfo(folder);
        }
        string nugetPackages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        string[] packages = Directory.GetDirectories(nugetPackages);
        Func<string, string> getVersion = (package) =>
        {
            List<string> directories = new List<string>(Directory.GetDirectories(package));
            directories.Sort();
            directories.Reverse();
            return directories.Count > 0 ? directories[0] : string.Empty;
        };
        foreach (var package in packages)
        {
            if (!package.StartsWith("bbob-plugin-")) continue;
            string version = getVersion(package);
            string tar = Path.Combine(version, "lib");
            if (version == string.Empty || !Directory.Exists(tar)) continue;
            string[] frameworks = Directory.GetDirectories(tar);
            string[] FIXS =
            {
                "net6",
                "netcoreapp",
                "netstandard"
            };
            string fix = "";
            List<string> fs = new List<string>();
            foreach (var framework in frameworks)
            {
                string frameworkName = new DirectoryInfo(framework).Name ?? "";
                foreach (var f in FIXS)
                {
                    if (frameworkName.StartsWith(f) && (fix == "" || f == fix))
                    {
                        fs.Add(framework);
                        fix = f;
                        break;
                    }
                }
            }
            if (fs.Count > 0)
            {
                fs.Sort();
                fs.Reverse();
                addInfo(fs[0]);
            }
        }
        return infos;
    }

    private static void LoadBuildInPlugins()
    {
        var config = Configuration.ConfigManager.GetConfigManager().MainConfig;
        buildInPlugins.Clear();

        Type[] types = GetBuildInPlugins();
        foreach (var type in types)
        {
            var buildInPlugin = getPluginInfo(type);
            if (!config.isPluginEnable(buildInPlugin))
            {
                System.Console.WriteLine($"Disable build-in plugin <{buildInPlugin.name}>");
                continue;
            }
            InitializeExecutingPlugin(getPluginInfo(type));
            var p = (IPlugin?)Activator.CreateInstance(type);
            if (p == null) continue;
            buildInPlugins.Add(p);
            System.Console.WriteLine($"Loaded build-in plugin <{buildInPlugin.name}>");
        }
    }
    private static void LoadThirdPlugins()
    {
        List<KeyValuePair<string, PluginJson>> thirdPluginsInfo = GetThirdPluginsInfo();
        foreach (var third in thirdPluginsInfo)
        {
            string pluginDll = Path.Combine(third.Key, third.Value.entry);
            InitializeExecutingPlugin(third.Value);
            var mainPlugin = new PluginAssemblyLoadContext(pluginDll, third.Value);
            if (mainPlugin.havePlugin)
            {
                System.Console.WriteLine($"Loaded third plugin <{third.Value.name}>");
                thirdPlugins.Add(mainPlugin);
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

    private static void InitializeExecutingPlugin(IPlugin plugin) => InitializeExecutingPlugin(getPluginInfo(plugin));
    private static void InitializeExecutingPlugin(PluginAssemblyLoadContext pContent) => InitializeExecutingPlugin(pContent.PluginInfo);
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

    public static PluginJson getPluginInfo(IPlugin plugin) => getPluginInfo(plugin.GetType());
    public static PluginJson getPluginInfo(Type typeOfPlugin)
    {
        PluginJson info = new PluginJson();
        info.name = typeOfPlugin.Name;
        info.author = "Bbob";
        info.description = "This is build-in plugin.";
        info.repository = "No have repository";

        return info;
    }


    public static bool containPluginWithName(string name)
    {
        name = name.ToUpper();
        Type[] types = GetBuildInPlugins();
        foreach (var plugin in types)
        {
            if (getPluginInfo(plugin).name.ToUpper() == name) return true;
        }
        List<KeyValuePair<string, PluginJson>> infosThird = GetThirdPluginsInfo();
        foreach (var info in infosThird)
        {
            if (info.Value.name.ToUpper() == name) return true;
        }

        return false;
    }
    public static IPlugin GetBuildInPlugin(int index) => buildInPlugins[index];
    public static IPlugin GetThirdPlugin(int index) => thirdPlugins[index].Plugin;
    public static int AllPluginCount => buildInPlugins.Count + thirdPlugins.Count;
    public static int BuildInPluginCount => buildInPlugins.Count;
    public static int ThirdPluginCount => thirdPlugins.Count;
}