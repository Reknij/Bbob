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
        PluginSystem.cyclePlugins((plugin)=>{
            plugin.InitCommand();
        });
        return true;
    }
}