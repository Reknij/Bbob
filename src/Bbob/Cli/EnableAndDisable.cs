using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class EnableAndDisable : Command
{
    public enum Options
    {
        enable, disable
    }

    Options option;
    string pluginName;
    bool direct;
    public EnableAndDisable(Options options, string pluginName, bool direct = false)
    {
        this.option = options;
        this.pluginName = pluginName.ToUpper();
        this.direct = direct;
    }
    public override bool Process()
    {
        var config = Configuration.ConfigManager.GetConfigManager().MainConfig;

        if (!direct && !PluginSystem.containPluginWithName(pluginName))
        {
            System.Console.WriteLine($"Not exists plugin with name '{pluginName.ToUpper()}'");
            return true;
        }
        switch (option)
        {
            case Options.disable:
                if (!config.isPluginEnable(pluginName))
                {
                    System.Console.WriteLine($"Already disable <{pluginName.ToUpper()}>");
                    return true;
                }
                config.pluginsDisable.Add(pluginName.ToUpper());
                Configuration.ConfigManager.GetConfigManager().SaveConfig();
                System.Console.WriteLine($"Disable <{pluginName.ToUpper()}> success");
                return true;

            case Options.enable:
                if (config.isPluginEnable(pluginName, out int index))
                {
                    System.Console.WriteLine($"Already enable <{pluginName.ToUpper()}>");
                    return true;
                }
                config.pluginsDisable.RemoveAt(index);
                Configuration.ConfigManager.GetConfigManager().SaveConfig();
                System.Console.WriteLine($"Enable <{pluginName.ToUpper()}> success");
                return true;
            default:
                break;
        }

        return true;
    }
}