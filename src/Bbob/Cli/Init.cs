using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Init : ICommand
{
    public Init()
    {
    }
    public bool Process()
    {
        Configuration.ConfigManager.GetConfigManager();
        PluginSystem.LoadAllPlugins();
        ThemeProcessor.LoadAllTheme();
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