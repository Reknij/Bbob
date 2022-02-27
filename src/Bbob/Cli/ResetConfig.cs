using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class ResetConfig : Command
{
    string configName;
    public ResetConfig(string configName)
    {
        this.configName = configName;
    }
    public override bool Process()
    {
        var configManager = Configuration.ConfigManager.GetConfigManager();
        var properties = configManager.MainConfig.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.Name.ToLower() == configName.ToLower())
            {
                object? value = property.GetValue(configManager.DefaultConfig);
                property.SetValue(configManager.MainConfig, value);
                System.Console.WriteLine($"Reset '{property.Name}' to default config.");
                configManager.SaveConfig(configManager.MainConfig, Configuration.ConfigManager.ConfigPath ?? throw new NullReferenceException("configPath is null"));
                return true;
            }
        }
        return false;
    }
}