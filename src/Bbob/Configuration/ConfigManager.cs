using System.Reflection;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.Configuration;

public class ConfigManager
{
    public static string? ConfigPath { get; set; }
    private static ConfigManager mainConfigManager = new ConfigManager();
    public ConfigJson MainConfig { get; set; }
    public ConfigJson DefaultConfig { get; set; }
    public static ConfigManager GetConfigManager()
    {
        return mainConfigManager;
    }
    private ConfigManager()
    {
        ConfigPath = Path.Combine(Environment.CurrentDirectory, "config.json");

        MainConfig = new ConfigJsonFix();
        DefaultConfig = new ConfigJsonFix();
        if (File.Exists(ConfigPath))
        {
            using (FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read))
            {

                ConfigJsonFix? targetConfig = JsonSerializer.Deserialize<ConfigJsonFix>(fs);
                if (targetConfig != null)
                {
                    targetConfig.Recheck();
                    MainConfig = targetConfig;

                    System.Console.WriteLine("Loaded config file.");
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

    public void SaveConfig(ConfigJson target, string savePath)
    {
        using (FileStream fs = File.OpenWrite(savePath))
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            JsonSerializer.Serialize(fs, target, options);
        }
    }

    public void registerConfigToPluginSystem() => registerConfigToPluginSystem(MainConfig);
    public void registerConfigToPluginSystem(ConfigJson config) => PluginHelper.registerObject("config", config);

    public class ConfigJsonFix : ConfigJson
    {
        public ConfigJsonFix()
        {
            theme = "default";
            author = "Jinker";
            description = "Nothing description...";
            about = "Nothing about...";
            blogName = "Bbob - Serverless Blog Framework";
            blogCountOneTime = 10;
            allLink = "false";
            recursion = false;
            baseUrl = "/";
            previewPort = 3000;
            var main = Assembly.GetExecutingAssembly();
            var types = main?.GetTypes();
            List<string> buildInList = new List<string>();
            if (types != null)
                foreach (Type type in types)
                {
                    if (type.GetInterface("IPlugin") != null && type.FullName != null)
                    {
                        buildInList.Add(type.Name);
                    }
                }
            buildInPlugins = buildInList.ToArray();
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
            if (baseUrl == "")
            {
                System.Console.WriteLine("Warning: config.publicPath value is null.");
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