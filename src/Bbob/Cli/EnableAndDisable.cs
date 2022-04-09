using Bbob.Main.PluginManager;
using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class EnableAndDisable : Command
{
    public new static string Name => "enable/disable";
    public new static string Help => "Enable/Disable the plugin. If <pluginName> is '*b' corresponding all build-in plugin, '*t' corresponding all third plugin, if will not affect original disable config.\n" +
    "[option]:\n" +
    "-d : direct enable or disable plugin.\n\n" +
    "Use:\n" +
    "// enable [option] <pluginName>\n" +
    "// disable [option] <pluginName>";
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
        if (this.pluginName.StartsWith("PLUGIN-") || this.pluginName.StartsWith("THEME-")) this.pluginName = $"BBOB-{this.pluginName}";
        this.direct = direct;
    }
    private bool isAll(string name)
    {
        if (name.ToUpper() == "*B" || name.ToUpper() == "*T") return true;
        return false;
    }
    public override bool Process()
    {
        var config = Configuration.ConfigManager.MainConfig;

        if (!isAll(pluginName) && !direct && !PluginSystem.containPluginWithName(pluginName))
        {
            ConsoleHelper.printWarning($"Not exists plugin with name '{pluginName.ToUpper()}'");
            return true;
        }
        switch (option)
        {
            case Options.disable:
                if (isAll(pluginName))
                {
                    if (config.pluginsDisable.Count > 0)
                    {
                        if (isAll(config.pluginsDisable[0]))
                        {
                            if (config.pluginsDisable[0].ToUpper() != pluginName.ToUpper())
                            {
                                string i = char.ToUpper(config.pluginsDisable[0][1]) == 'B' ? "build-in" : "third";
                                System.Console.WriteLine($"Will enable all {i} plugin.");
                                config.pluginsDisable[0] = pluginName;
                            }

                        }
                        else config.pluginsDisable.Insert(0, pluginName);
                    }
                    else config.pluginsDisable.Add(pluginName);

                    Configuration.ConfigManager.SaveConfig();
                    string info = char.ToUpper(pluginName[1]) == 'B' ? "build-in" : "third";
                    ConsoleHelper.printSuccess($"Disable all {info} plugin success");
                    return true;
                }
                if (!config.isPluginEnable(pluginName))
                {
                    ConsoleHelper.printWarning($"Already disable <{pluginName.ToUpper()}>");
                    return true;
                }
                config.pluginsDisable.Add(pluginName.ToUpper());
                Configuration.ConfigManager.SaveConfig();
                ConsoleHelper.printSuccess($"Disable <{pluginName.ToUpper()}> success");
                return true;

            case Options.enable:
                if (isAll(pluginName))
                {
                    if (config.pluginsDisable.Count > 0 && config.pluginsDisable.First() == "*")
                    {
                        config.pluginsDisable.RemoveAt(0);
                        Configuration.ConfigManager.SaveConfig();
                    }
                    string info = pluginName[1].ToString().ToUpper() == "B" ? "build-in" : "third";
                    ConsoleHelper.printSuccess($"Enable all {info} plugin success");
                    return true;
                }
                if (config.isPluginEnable(pluginName, out int index))
                {
                    ConsoleHelper.printWarning($"Already enable <{pluginName.ToUpper()}>");
                    return true;
                }
                config.pluginsDisable.RemoveAt(index);
                Configuration.ConfigManager.SaveConfig();
                ConsoleHelper.printSuccess($"Enable <{pluginName.ToUpper()}> success");
                return true;
            default:
                break;
        }

        return true;
    }
}