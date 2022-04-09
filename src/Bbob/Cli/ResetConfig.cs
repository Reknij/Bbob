using Bbob.Main.PluginManager;
using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class ResetConfig : Command
{
    public new static string Name => "reset-config";
    public new static string Help => "Reset target config to default value.\n"+
    "Use:\n"+
    "// reset-config <configName>\n"+
    "// rc <configName>";
    string configName;
    public ResetConfig(string configName)
    {
        this.configName = configName;
    }
    public override bool Process()
    {
        const string SUCCESS = "Success reset: ";
        const string FAILED = "Failed reset: ";
        var properties = Configuration.ConfigManager.MainConfig.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.Name.ToLower() == configName.ToLower())
            {
                object? value = property.GetValue(Configuration.ConfigManager.DefaultConfig);
                property.SetValue(Configuration.ConfigManager.MainConfig, value);
                Configuration.ConfigManager.SaveConfig(Configuration.ConfigManager.MainConfig, Configuration.ConfigManager.ConfigPath ?? throw new NullReferenceException("configPath is null"));
                ConsoleHelper.printSuccess($"{SUCCESS}Reset '{property.Name}' to default config.");
                return true;
            }
        }
        ConsoleHelper.printError($"{FAILED}Not found property of given argument.");
        return false;
    }
}