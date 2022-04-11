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
        if (this.pluginName.StartsWith("PLUGIN-", true, null) || this.pluginName.StartsWith("THEME-", true, null)) this.pluginName = $"BBOB-{this.pluginName}";
        this.direct = direct;
    }
    private static bool isAll(HashSet<string> pluginsDisable)
    {
        return pluginsDisable.Contains("*b") || pluginsDisable.Contains("*t");
    }
    public bool isAll(string name)
    {
        return name.ToUpper() == "*B" || name.ToUpper() == "*T";
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
                    bool already = false;
                    if (config.pluginsDisable.Count > 0)
                    {
                        string other = pluginName == "*B" ? "*T" : "*B";
                        if (config.pluginsDisable.Contains(other))
                        {
                            string i = other[1] == 'B' ? "build-in" : "third";
                            ConsoleHelper.printWarning($"Will enable all {i} plugin.");
                            config.pluginsDisable.Remove(other);
                        }
                        already = !config.pluginsDisable.Add(pluginName);
                    }
                    else config.pluginsDisable.Add(pluginName);

                    Configuration.ConfigManager.SaveConfig();
                    string info = pluginName[1] == 'B' ? "build-in" : "third";
                    if (already) ConsoleHelper.printWarning($"Already disable all {info} plugin.");
                    else ConsoleHelper.printSuccess($"Disable all {info} plugin success");
                    return true;
                }
                if (!config.isPluginEnable(pluginName))
                {
                    ConsoleHelper.printWarning($"Already disable <{pluginName}>");
                    return true;
                }
                config.pluginsDisable.Add(pluginName);
                Configuration.ConfigManager.SaveConfig();
                ConsoleHelper.printSuccess($"Disable <{pluginName}> success");
                return true;

            case Options.enable:
                if (isAll(pluginName))
                {
                    bool already = false;
                    if (config.pluginsDisable.Count > 0 && !isAll(config.pluginsDisable))
                    {
                        if (!config.pluginsDisable.Contains(pluginName)) already = true;
                        else
                        {
                            config.pluginsDisable.Remove(pluginName);
                            Configuration.ConfigManager.SaveConfig();
                        }
                    }
                    else already = true;
                    string info = pluginName[1].ToString() == "B" ? "build-in" : "third";
                    if (already) ConsoleHelper.printWarning($"Already enable all {info} plugin.");
                    else ConsoleHelper.printSuccess($"Enable all {info} plugin success");
                    return true;
                }
                if (config.isPluginEnable(pluginName))
                {
                    ConsoleHelper.printWarning($"Already enable <{pluginName}>");
                    return true;
                }
                config.pluginsDisable.Remove(pluginName);
                Configuration.ConfigManager.SaveConfig();
                ConsoleHelper.printSuccess($"Enable <{pluginName}> success");
                return true;
            default:
                break;
        }

        return true;
    }
}