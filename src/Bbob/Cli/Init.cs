using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Init : Command
{
    public Init()
    {
    }
    public override bool Process()
    {
        Configuration.ConfigManager.GetConfigManager();
        Directory.CreateDirectory(JSApi.JSAPiHelper.metasFolder);
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.InitCommand();
            });
            PluginSystem.cyclePlugins((plugin)=>{
                plugin.CommandComplete(Commands.InitCommand);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine("Executing init command error:\n" + msg);
            return false;
        }
        return true;
    }
}