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

        MainConfig = new ConfigJson();
        DefaultConfig = new ConfigJson();
        if (File.Exists(ConfigPath))
            using (FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read))
            {

                ConfigJson? targetConfig = JsonSerializer.Deserialize<ConfigJson>(fs);
                if (targetConfig != null)
                {
                    MainConfig = targetConfig;
                    MainConfig.Recheck();
                    System.Console.WriteLine("Loaded config file.");
                }
            }
        else
        {
            Console.WriteLine("Not found app config file. Will create and load default config.");
            using (FileStream fs = File.OpenWrite(ConfigPath))
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                JsonSerializer.Serialize(fs, MainConfig, options);
            }
        }
    }

    public class ConfigJson
    {
        public string theme { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string about { get; set; }
        public string blogName { get; set; }

        public int blogCountOneTime { get; set; }
        public string allLink { get; set; }
        public bool recursion { get; set; }
        public string publicPath {get;set;}

        public string[] buildInPlugins { get; set; }

        public ConfigJson()
        {
            theme = "default";
            author = "Jinker";
            description = "Nothing description...";
            about = "Nothing about...";
            blogName = "Bbob - Serverless Blog Framework";
            blogCountOneTime = 10;
            allLink = "false";
            recursion = false;
            publicPath = "/";
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
            if (allLink != "current" && allLink != "next")
            {
                System.Console.WriteLine("Warning: config.allLink value is not `current` or `next`.");
                System.Console.WriteLine("Auto set to `false` now.");
                allLink = "false";
            }
            if (publicPath == "")
            {
                System.Console.WriteLine("Warning: config.publicPath value is null.");
                System.Console.WriteLine("Auto set to '/'");
                publicPath = "/";
            }
            else if (publicPath.First() != '/')
            {
                System.Console.WriteLine("Warning: config.publicPath value start character is not '/'.");
                System.Console.WriteLine("Auto added the '/'");
                publicPath = $"/{publicPath}";
            }
            if (publicPath.Last() != '/')
            {
                publicPath = $"{publicPath}/";
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