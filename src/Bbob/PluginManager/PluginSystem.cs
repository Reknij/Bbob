using Bbob.Plugin;
using Bbob.Plugin.Cores;
using System.Reflection;
using System.Text.Json;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.PluginManager;

public static class PluginSystem
{
    public static readonly string pluginCurrentDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
    public static readonly string pluginBaseDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
    public static readonly string configsFolder = Path.Combine(Environment.CurrentDirectory, "configs");
    static List<PluginAssemblyLoadContext> thirdPlugins = new List<PluginAssemblyLoadContext>();
    static List<IPlugin> buildInPlugins = new List<IPlugin>();
    static List<PluginContext> allPlugin = new List<PluginContext>();

    public static bool ShowLoadedMessage { get; set; } = true;
    public static void LoadAllPlugins(string themePlugin = "")
    {
        Directory.CreateDirectory(configsFolder);
        if (ShowLoadedMessage) System.Console.WriteLine("Loading Plugin System...");
        PluginHelperCore.currentDirectory = Environment.CurrentDirectory;
        PluginHelperCore.baseDirectory = AppContext.BaseDirectory;
        PluginHelperCore.pluginsLoaded.Clear();
        LoadBuildInPlugins();
        LoadThirdPlugins(themePlugin);
        if (ShowLoadedMessage)
        {
            if ((BuildInPluginCount + ThirdPluginCount) > 0) ConsoleHelper.printSuccess($"Loaded {AllPluginCount} plugins. [{BuildInPluginCount}|{ThirdPluginCount}]\n");
            else ConsoleHelper.printWarning("Warning: plugins are not loaded.");
        }
        foreach (var p in buildInPlugins)
        {
            allPlugin.Add(new PluginContext(p, getPluginInfo(p)));
        }
        foreach (var p in thirdPlugins)
        {
            allPlugin.Add(new PluginContext(p.Plugin, p.PluginInfo));
        }
        processPlugins();
        Console.CancelKeyPress += (sender, e) => PluginHelper.Events.InvokeEvent(sender, new PluginHelper.Events.ProgramExitedEventArgs(), PluginHelper.Events.EventName.Exited);
    }

    public static void ProgramExited()
    {
        PluginHelper.Events.InvokeEvent(null, new PluginHelper.Events.ProgramExitedEventArgs(), PluginHelper.Events.EventName.Exited);
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

    private static Dictionary<PluginJson, Type> GetBuildInPluginsInfo()
    {
        var config = Configuration.ConfigManager.MainConfig;
        string buildInTypesPath = "Bbob.Main.BuildInPlugin.";
        Assembly main = Assembly.GetExecutingAssembly();
        Type[] types = main.GetTypes();
        Dictionary<PluginJson, Type> bip = new();
        foreach (var type in types)
        {
            if (type.FullName == null) continue;
            if (type.FullName.StartsWith(buildInTypesPath) && type.GetInterface("IPlugin") == typeof(IPlugin))
            {
                PluginJson info = type.GetCustomAttribute<PluginJson>() ?? throw new NullReferenceException("Build-in plugin must have PluginJson attribute!");

                bip.Add(info, type);
            }
        }
        return bip;
    }

    private static Dictionary<PluginJson, string> GetThirdPluginsInfo(string themePlugins = "")
    {
        Dictionary<string, PluginJson> addedPlugins = new();
        Dictionary<PluginJson, string> infos = new();
        thirdPlugins.Clear();
        Action<string> addInfo = (folder) =>
        {
            string pluginJsonPath = Path.Combine(folder, "plugin.json");
            PluginJson pluginInfo;
            if (File.Exists(pluginJsonPath))
            {
                try
                {
                    var info = JsonSerializer.Deserialize<PluginJson>(File.OpenRead(pluginJsonPath));
                    if (info == null)
                    {
                        ConsoleHelper.printError("Plugin json file can't loaded.");
                        return;
                    }
                    else pluginInfo = info;
                }
                catch (System.Exception ex)
                {
                    ConsoleHelper.printError("Load plugin json error: " + ex.Message);
                    return;
                }
            }
            else return;

            try
            {
                var a = pluginInfo.name;
            }
            catch (NullReferenceException)
            {
                pluginInfo.name = Path.GetDirectoryName(folder) ?? $"UnknownPluginName.{Path.GetRandomFileName()}";
            }
            if (!addedPlugins.ContainsKey(pluginInfo.name))
            {
                infos.Add(pluginInfo, folder);
                addedPlugins.Add(pluginInfo.name, pluginInfo);
            }
            else
            {
                Version.TryParse(addedPlugins[pluginInfo.name].version, out Version? added);
                Version.TryParse(pluginInfo.version, out Version? toAdd);
                if (added > toAdd) return;
                infos.Remove(addedPlugins[pluginInfo.name]);
                infos.Add(pluginInfo, folder);
                addedPlugins[pluginInfo.name] = pluginInfo;
            }
        };
        List<string> folders = new List<string>();
        if (Directory.Exists(pluginCurrentDirectory)) folders.AddRange(Directory.GetDirectories(pluginCurrentDirectory));

        string nugetPackages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        string[] packages = Directory.Exists(nugetPackages) ? Directory.GetDirectories(nugetPackages) : Array.Empty<string>();
        Func<string, string> getVersion = (package) =>
        {
            List<string> directories = new List<string>(Directory.GetDirectories(package));
            directories.Sort();
            directories.Reverse();
            return directories.Count > 0 ? directories[0] : string.Empty;
        };
        foreach (var package in packages)
        {
            if (!package.ToUpper().StartsWith("BBOB-PLUGIN-")) continue;
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

        if (Directory.Exists(themePlugins))
        {
            string[] plugins = Directory.GetDirectories(themePlugins);
            foreach (var folder in plugins)
            {
                addInfo(folder);
                infos.Last().Key.themePlugin = true;
            }
        }

        return infos;
    }

    private static void LoadBuildInPlugins()
    {
        var config = Configuration.ConfigManager.MainConfig;
        buildInPlugins.Clear();
        if (!config.isAllBuildInPluginEnable())
        {
            ConsoleHelper.printWarning("You disable all build-in plugin!");
            return;
        }
        Dictionary<PluginJson, Type> plugins = GetBuildInPluginsInfo();
        foreach (var plugin in plugins)
        {
            if (!config.isPluginEnable(plugin.Key))
            {
                if (ShowLoadedMessage)
                {
                    ConsoleHelper.print("Disable", false, ConsoleColor.Yellow);
                    System.Console.Write(" build-in plugin");
                    ConsoleHelper.print($"[{plugin.Key.name}]", color: ConsoleColor.Blue);
                }
                continue;
            }
            InitializeExecutingPlugin(plugin.Key);
            var p = (IPlugin?)Activator.CreateInstance(plugin.Value);
            if (p == null) continue;
            buildInPlugins.Add(p);
            if (ShowLoadedMessage)
            {
                ConsoleHelper.print("Loaded", false, ConsoleColor.Green);
                System.Console.Write(" build-in plugin");
                ConsoleHelper.print($"[{plugin.Key.name}]", color: ConsoleColor.Blue);
            }
        }
    }
    private static void LoadThirdPlugins(string themePlugins)
    {
        var config = Configuration.ConfigManager.MainConfig;
        if (!config.isAllThirdPluginEnable())
        {
            ConsoleHelper.printWarning("You disable all third plugin!");
            return;
        }
        Dictionary<PluginJson, string> thirdPluginsInfo = GetThirdPluginsInfo(themePlugins);
        foreach (var third in thirdPluginsInfo)
        {
            if (!config.isPluginEnable(third.Value))
            {
                if (ShowLoadedMessage)
                {
                    ConsoleHelper.print("Disable", false, ConsoleColor.Yellow);
                    System.Console.WriteLine(" third plugin");
                    ConsoleHelper.print($"[{third.Key.name}]", color: ConsoleColor.DarkCyan);
                }
                continue;
            }
            string pluginDll = Path.Combine(third.Value, third.Key.entry);
            InitializeExecutingPlugin(third.Key);
            try
            {
                var mainPlugin = new PluginAssemblyLoadContext(pluginDll, third.Key);
                if (mainPlugin.havePlugin && mainPlugin.Warning == string.Empty)
                {
                    if (ShowLoadedMessage)
                    {
                        ConsoleHelper.print("Loaded", false, ConsoleColor.Green);
                        if (third.Key.themePlugin)
                        {
                            ConsoleHelper.print(" theme build-in", false, ConsoleColor.Blue);
                            System.Console.Write(" plugin");
                        }
                        else System.Console.Write(" third plugin");
                        ConsoleHelper.print($"[{third.Key.name}]", color: ConsoleColor.DarkCyan);
                    }
                    thirdPlugins.Add(mainPlugin);
                }
                if (mainPlugin.Warning != string.Empty) ConsoleHelper.printWarning($"Warning: {mainPlugin.Warning}");
            }
            catch (System.Exception ex)
            {
                ConsoleHelper.printError("Load and initialize plugin has error:\n" + ex.Message);
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
                        if (!allPlugin.Any(pc => pc.info.name == condition.PluginName))
                        {
                            if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                            {
                                ConsoleHelper.printWarning($"Warning: Will no run <{info.name}> because require plugin <{condition.PluginName}> is no contain!");
                                showWarningSet.Add(info.name);
                            }
                            allPlugin.RemoveAt(i);
                            break;
                        }
                        if (!PluginHelper.isTargetPluginEnable(condition.PluginName))
                        {
                            if (condition.ShowWarning && !showWarningSet.Contains(info.name))
                            {
                                ConsoleHelper.printWarning($"Warning: Will no run <{info.name}> because require plugin <{condition.PluginName}> is disable.");
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
        int length = allPlugin.Count;
        PluginHelperCore.pluginsLoaded.EnsureCapacity(length);
        for (int i = 0; i < length; i++)
        {
            PluginHelperCore.pluginsLoaded.Add(allPlugin[i].info.name, allPlugin[i].info);
        }
    }

    public delegate void CyclePluginDelegate(IPlugin plugin);
    public static void cyclePlugins(CyclePluginDelegate cyclePluginDelegate)
    {
        PluginHelperCore.pluginsDone.Clear();
        List<PluginContext> requireRunAgain = new();

        Func<PluginContext, bool> runPlugin = (context) =>
        {
            InitializeExecutingPlugin(context.info);
            cyclePluginDelegate?.Invoke(context.main);
            switch (PluginHelper.ExecutingCommandResult.Operation)
            {
                default:
                case CommandOperation.None:
                    PluginHelperCore.pluginsDone.Add(context.info.name);
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
                    ConsoleHelper.printWarning($"Plugin <{PluginHelper.ExecutingPlugin.name}> has been run count more than {runCount.WarningCount}");
                    ConsoleHelper.printWarning($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                    runCount.WarningCount *= 2;
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
        PluginHelperCore.executingPlugin = info;
        PluginHelper.ExecutingCommandResult = new CommandResult();
        PluginHelper.printResetColor();
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
        var bip = GetBuildInPluginsInfo();
        var tp = GetThirdPluginsInfo();
        if (bip.Any(p => p.Key.name.ToUpper() == name)) return true;
        if (tp.Any(p => p.Key.name.ToUpper() == name)) return true;
        return false;
    }

    public static IPlugin GetBuildInPlugin(int index) => buildInPlugins[index];
    public static IPlugin GetThirdPlugin(int index) => thirdPlugins[index].Plugin;
    public static int AllPluginCount => buildInPlugins.Count + thirdPlugins.Count;
    public static int BuildInPluginCount => buildInPlugins.Count;
    public static int ThirdPluginCount => thirdPlugins.Count;
}