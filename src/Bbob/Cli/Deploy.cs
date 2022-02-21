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
        }
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.DeployCommand(distribution);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine("Error executing plugin deploy command:\n" + msg);
            return false;
        }
        return true;
    }
}