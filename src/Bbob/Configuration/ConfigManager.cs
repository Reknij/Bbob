using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Plugin;
using Bbob.Plugin.Cores;

namespace Bbob.Main.Configuration;

public static class ConfigManager
{
    public static string ConfigPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "config.json");
    public static bool ShowLoadedMessage {get;set;} = true;
    private static ConfigJson? mConfig;
    private static ConfigJson? dConfig;
    public static ConfigJson MainConfig
    {
        get
        {
            if (mConfig == null) LoadConfigs();
            return mConfig ?? throw new NullReferenceException("MainConfig is null");
        }
        set => mConfig = value;
    }
    public static ConfigJson DefaultConfig
    {
        get
        {
            if (dConfig == null) LoadConfigs();
            return dConfig ?? throw new NullReferenceException("MainConfig is null");
        }
        set => dConfig = value;
    }

    public static void LoadConfigs()
    {
        MainConfig = new ConfigJsonFix();
        DefaultConfig = new ConfigJsonFix();
        if (File.Exists(ConfigPath))
        {
            using (FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read))
            {
                bool loaded = false;
                try
                {
                    ConfigJsonFix? targetConfig = JsonSerializer.Deserialize<ConfigJsonFix>(fs);
                    if (targetConfig != null)
                    {
                        MainConfig = targetConfig;
                        loaded = true;
                    }
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Error load config, reset config now? (y/n): ");
                    var key = Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        MainConfig = DefaultConfig;
                        System.Console.WriteLine("Reset to default config.");
                    }
                    else
                    {
                        System.Console.WriteLine("Error config can't continue load.");
                        Environment.Exit(-1);
                    }
                }
                if (loaded)
                {
                    ((ConfigJsonFix)MainConfig).Recheck();
                    if (ShowLoadedMessage) System.Console.WriteLine("Loaded config file.");
                }
                else
                {
                    System.Console.WriteLine("Config can't deserialize from the file. Load default config");
                }
            }
        }
        else
        {
            Console.WriteLine("Not found app config file. Will create and load default config.");
        }
        SaveConfig(MainConfig, ConfigPath); //always save as the config user may be old.
    }

    public static void SaveConfig(ConfigJson target, string savePath)
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        using (FileStream fs = File.OpenWrite(savePath))
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            JsonSerializer.Serialize(fs, target, options);
        }
    }
    public static void SaveConfig(ConfigJson target) => SaveConfig(target, ConfigPath);
    public static void SaveConfig(string configPath) => SaveConfig(MainConfig, configPath);
    public static void SaveConfig() => SaveConfig(MainConfig, ConfigPath);

    public static void registerConfigToPluginSystem() => registerConfigToPluginSystem(MainConfig);
    public static void registerConfigToPluginSystem(ConfigJson config) => PluginHelperCore.configBbob = config;

    public class ConfigJsonFix : ConfigJson
    {
        public ConfigJsonFix()
        {
            theme = "default";
            author = "Unknown author";
            description = "Nothing description...";
            about = "Nothing about...";
            blogName = "Bbob - Serverless Blog Framework";
            blogCountOneTime = 10;
            allLink = "false";
            recursion = true;
            baseUrl = "/";
            previewPort = 3000;
            var main = Assembly.GetExecutingAssembly();
            var types = main?.GetTypes();
            pluginsDisable = new List<string>();
            distributionPath = "./dist";
        }

        public override string ToString()
        {
            System.Reflection.PropertyInfo[] propertyInfos = typeof(ConfigJson).GetProperties();
            string result = "ConfigJson<";
            foreach (var info in propertyInfos)
            {
                result += $"{info.Name}: {info.GetValue(this)}, ";
            }
            result = result.Remove(result.Length - 2, 2);
            result += '>';
            return result;
        }

        public void Recheck()
        {
            if (blogCountOneTime < 3)
            {
                System.Console.WriteLine("Warning: config.blogCountOneTime value minimun value is 3.");
                System.Console.WriteLine("Auto set to 3 now.");
                blogCountOneTime = 3;
            }
            if (allLink != "current" && allLink != "next" && allLink != "false")
            {
                System.Console.WriteLine("Warning: config.allLink value is not `current`, `next` or 'false'.");
                System.Console.WriteLine("Auto set to `false` now.");
                allLink = "false";
            }
            var domainMatch = Regex.Match(domain, @"^(?:\w+://)?([^/?]*)");
            if (!domainMatch.Success) System.Console.WriteLine("Warning: config.domain is invalid!");
            else if (domainMatch.Value != domain)
            {
                System.Console.WriteLine("Warning: domain is have invalid sub url.");
                domain = domainMatch.Value;
                System.Console.WriteLine($"Auto repair it to '{domain}'");
            }
            if (baseUrl == "")
            {
                System.Console.WriteLine("Warning: config.baseUrl value is null.");
                System.Console.WriteLine("Auto set to '/'");
                baseUrl = "/";
            }
            if (baseUrl.Last() != '/')
            {
                baseUrl = $"{baseUrl}/";
            }
            if (previewPort < 1024 || previewPort > 49151)
            {
                System.Console.WriteLine("Warning: config.previewPort value is not 1024 - 49151");
                System.Console.WriteLine("Auto set to default port '3000'");
                previewPort = 3000;
            }
        }

        public override bool Equals(object? obj)
        {
            return this.ToString() == obj?.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}