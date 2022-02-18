using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Deploy : ICommand
{
    string distribution;
    bool load;

    public Deploy(string dist, bool load)
    {
        distribution = dist;
        this.load = load;
    }
    public bool Process()
    {
        if (load)
        {
            PluginSystem.LoadAllPlugins();
            ThemeProcessor.LoadAllTheme();
        }
        PluginSystem.cyclePlugins((plugin) =>
        {
            plugin.DeployCommand(distribution);
        });
        return true;
    }
}