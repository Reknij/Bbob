using Bbob.Plugin;
using System.Reflection;
using System.Text.Json;

namespace Bbob.Main.PluginManager;

public static class PluginSystem
{
    public static readonly string pluginCurrentDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
    public static readonly string pluginBaseDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
    public static readonly string configsFolder = Path.Combine(Environment.CurrentDirectory, "configs");
    static List<PluginAssemblyLoadContext> thirdPlugins = new List<PluginAssemblyLoadContext>();
    static List<IPlugin> buildInPlugins = new List<IPlugin>();
    static List<PluginContext> allPlugin = new List<PluginContext>();

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
        foreach (var p in buildInPlugins)
        {
            allPlugin.Add(new PluginContext(p, getPluginInfo(p)));
        }
        foreach (var p in thirdPlugins)
        {
            allPlugin.Add(new PluginContext(p.Plugin, p.PluginInfo));
        }
        processPlugins();
    }

    public static void printAllPlugin()
    {
        foreach (var item in allPlugin)
        {
            System.Console.WriteLine($"// {item.info.name}");
        }
        if (allPlugin.Count == 0)
        {
            System.Console.WriteLine("// no plugins loaded.");
        }
    }

    private static Type[] GetBuildInPlugins()
    {
        var config = Configuration.ConfigManager.MainConfig;
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
        HashSet<string> addedPlugins = new();
        List<KeyValuePair<string, PluginJson>> infos = new();
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
                    if (!addedPlugins.Contains(pluginInfo.name))
                    {
                        infos.Add(new KeyValuePair<string, PluginJson>(folder, pluginInfo));
                        addedPlugins.Add(pluginInfo.name);
                    }
                }
            }
        };
        List<string> folders = new List<string>();
        if (Directory.Exists(pluginCurrentDirectory)) folders.AddRange(Directory.GetDirectories(pluginCurrentDirectory));

        string nugetPackages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        string[] packages = Directory.Exists(nugetPackages)? Directory.GetDirectories(nugetPackages): Array.Empty<string>();
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
                folders.Add(fs[0]);
            }
        }

        if (Directory.Exists(pluginBaseDirectory)) folders.AddRange(Directory.GetDirectories(pluginBaseDirectory));

        foreach (string folder in folders)
        {
            addInfo(folder);
        }

        return infos;
    }

    private static void LoadBuildInPlugins()
    {
        var config = Configuration.ConfigManager.MainConfig;
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
            InitializeExecutingPlugin(buildInPlugin);
            var p = (IPlugin?)Activator.CreateInstance(type);
            if (p == null) continue;
            buildInPlugins.Add(p);
            System.Console.WriteLine($"Loaded build-in plugin <{buildInPlugin.name}>");
        }
    }
    private static void LoadThirdPlugins()
    {
        List<KeyValuePair<string, PluginJson>> thirdPluginsInfo = GetThirdPluginsInfo();
        var config = Configuration.ConfigManager.MainConfig;
        foreach (var third in thirdPluginsInfo)
        {
            if (!config.isPluginEnable(third.Value))
            {
                System.Console.WriteLine($"Disable third plugin <{third.Value.name}>");
                continue;
            }
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
    private static void processPlugins()
    {
        for (int i = allPlugin.Count - 1; i >= 0; i--)
        {
            IPlugin plugin = allPlugin[i].main;
            PluginJson info = allPlugin[i].info;

            foreach (var attr in Attribute.GetCustomAttributes(plugin.GetType()))
            {
                if (attr is PluginCondition condition)
                {
                    if ((condition.ConditionType & ConditionType.Require) != 0 && condition.PluginName != "*")
                    {
                        if (!containPluginWithName(condition.PluginName))
                        {
                            if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                            {
                                System.Console.WriteLine($"Warning: Will no run <{info.name}> because require plugin <{condition.PluginName}> is no contain!");
                                showWarningSet.Add(info.name);
                            }
                            allPlugin.RemoveAt(i);
                            break;
                        }
                        if (!PluginHelper.isTargetPluginEnable(condition.PluginName))
                        {
                            if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                            {
                                System.Console.WriteLine($"Warning: Will no run <{info.name}> because require plugin <{condition.PluginName}> is disable.");
                                showWarningSet.Add(info.name);
                            }
                            allPlugin.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
        PluginRelation pluginRelation = new PluginRelation(allPlugin);
        allPlugin = pluginRelation.ProcessRelation();
    }

    public delegate void CyclePluginDelegate(IPlugin plugin);
    public static void cyclePlugins(CyclePluginDelegate cyclePluginDelegate)
    {
        PluginHelper._pluginsDone.Clear();
        List<PluginContext> requireRunAgain = new();

        Func<PluginContext, bool> runPlugin = (context) =>
        {
            InitializeExecutingPlugin(context.info);
            cyclePluginDelegate?.Invoke(context.main);
            switch (PluginHelper.ExecutingCommandResult.Operation)
            {
                default:
                case CommandOperation.None:
                    PluginHelper._pluginsDone.Add(context.info.name);
                    break;
                case CommandOperation.RunMeAgain:
                    requireRunAgain.Add(context);
                    break;
                case CommandOperation.Stop:
                case CommandOperation.Skip:
                    return false;
            }
            return true;
        };

        foreach (var p in allPlugin)
        {
            if (!runPlugin(p)) return;
        }
        Dictionary<PluginContext, RunCount> runCounts = new Dictionary<PluginContext, RunCount>();
        while (requireRunAgain.Count > 0)
        {
            for (int i = requireRunAgain.Count - 1; i >= 0; i--)
            {
                if (!runCounts.ContainsKey(requireRunAgain[i])) runCounts.Add(requireRunAgain[i], new RunCount());
                if (!runPlugin(requireRunAgain[i])) return;
                RunCount runCount = runCounts[requireRunAgain[i]];
                requireRunAgain.RemoveAt(i);
                runCount.Count++;
                if (runCount.Count > runCount.WarningCount)
                {
                    System.Console.WriteLine($"Plugin <{PluginHelper.ExecutingPlugin.name}> has been run count more than {runCount.WarningCount}");
                    System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                    runCount.WarningCount *= 2;
                }
            }
        }
    }

    private static bool runPlugin(IPlugin plugin, PluginJson info, CyclePluginDelegate cyclePluginDelegate)
    {

        return true;
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