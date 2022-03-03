using Bbob.Main.BuildInPlugin;
using Bbob.Plugin;
using System.Reflection;
using System.Text.Json;
using static Bbob.Plugin.ConfigJson;

namespace Bbob.Main.PluginManager;

public static class PluginSystem
{
    public static readonly string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "plugins"); //plugins in base of Bbob directory.
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
        List<KeyValuePair<string, PluginJson>> infos = new();
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
    private static HashSet<string> showWarningSet = new HashSet<string>();
    private static bool isConditionSuccess(IPlugin plugin, PluginJson info)
    {
        foreach (var attr in Attribute.GetCustomAttributes(plugin.GetType()))
        {
            if (attr is PluginCondition condition)
            {
                if (condition.RequirePlugin == null) continue;
                if (!containPluginWithName(condition.RequirePlugin))
                {
                    if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                    {
                        System.Console.WriteLine($"Warning: <{info.name}> require plugin <{condition.RequirePlugin}> but it is no contain!");
                        showWarningSet.Add(info.name);
                    }
                    return false;
                }
                if (!PluginHelper.isTargetPluginEnable(condition.RequirePlugin))
                {
                    if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                    {
                        System.Console.WriteLine($"Warning: <{info.name}> require plugin <{condition.RequirePlugin}> but it is disable.");
                        showWarningSet.Add(info.name);
                    }
                    return false;
                }
                if (condition.RequirePluginStatus != null)
                {
                    switch (condition.RequirePluginStatus)
                    {
                        case PluginStatus.Waiting:
                            if (PluginHelper.isTargetPluginDone(condition.RequirePlugin)) return false;
                            break;
                        case PluginStatus.Done:
                            if (!PluginHelper.isTargetPluginDone(condition.RequirePlugin)) return false;
                            break;
                        default: break;
                    }
                }
            }
        }
        return true;
    }

    public delegate void CyclePluginDelegate(IPlugin plugin);
    public static void cyclePlugins(CyclePluginDelegate cyclePluginDelegate)
    {
        List<KeyValuePair<IPlugin, PluginJson>> requireRunAgain = new();
        List<KeyValuePair<IPlugin, PluginJson>> pluginRefs = new();
        PluginHelper._pluginsDone.Clear();
        foreach (var p in buildInPlugins)
        {
            pluginRefs.Add(new KeyValuePair<IPlugin, PluginJson>(p, getPluginInfo(p)));
        }
        foreach (var p in thirdPlugins)
        {
            pluginRefs.Add(new KeyValuePair<IPlugin, PluginJson>(p.Plugin, p.PluginInfo));
        }
        foreach (var p in pluginRefs)
        {
            if (!isConditionSuccess(p.Key, p.Value)) continue;
            InitializeExecutingPlugin(p.Value);
            cyclePluginDelegate?.Invoke(p.Key);
            switch (PluginHelper.ExecutingCommandResult.Operation)
            {
                default:
                case CommandOperation.None:
                    PluginHelper._pluginsDone.Add(p.Value.name);
                    break;
                case CommandOperation.RunMeAgain:
                    requireRunAgain.Add(p);
                    break;
                case CommandOperation.Stop:
                case CommandOperation.Skip:
                    return;
            }
        }
        RunCount[] runCounts = RunCount.InitializeArray(requireRunAgain.Count);
        while (requireRunAgain.Count > 0)
        {
            for (int i = requireRunAgain.Count - 1; i >= 0; i--)
            {
                PluginHelper.ExecutingPlugin = requireRunAgain[i].Value;
                cyclePluginDelegate?.Invoke(requireRunAgain[i].Key);
                runCounts[i].Count++;
                switch (PluginHelper.ExecutingCommandResult.Operation)
                {
                    case CommandOperation.None:
                        PluginHelper._pluginsDone.Add(requireRunAgain[i].Value.name);
                        requireRunAgain.RemoveAt(i); //dont use i because may be already remove.
                        break;
                    case CommandOperation.RunMeAgain:
                        break;
                    case CommandOperation.Stop:
                    case CommandOperation.Skip:
                        return;
                    default: break;
                }
                if (runCounts[i].Count > runCounts[i].WarningCount)
                {
                    System.Console.WriteLine($"Plugin <{PluginHelper.ExecutingPlugin.name}> has been run count more than {runCounts[i].WarningCount}");
                    System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                    runCounts[i].WarningCount *= 2;
                }
            }
        }
    }
    private class RunCount
    {
        public int Count { get; set; }
        public int WarningCount { get; set; }
        public RunCount(int c = 0, int wc = 20)
        {
            Count = 0;
            WarningCount = 20;
        }
        public static void SetWarningCount(IEnumerable<RunCount> runCounts, int wc)
        {
            foreach (RunCount rc in runCounts)
            {
                rc.WarningCount = wc;
            }
        }

        public static RunCount[] InitializeArray(int length, int c = 0, int wc = 20)
        {
            RunCount[] runCounts = new RunCount[length];
            for (int i = 0; i < runCounts.Count(); i++)
            {
                runCounts[i] = new RunCount(0, wc);
            }
            return runCounts;
        }
    }
    private static void InitializeExecutingPlugin(PluginJson info)
    {
        PluginHelper.ExecutingPlugin = info;
        PluginHelper.ExecutingCommandResult = new CommandResult();
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