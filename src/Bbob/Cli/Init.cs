using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Init : Command
{
    public new static string Name => "init";
    public new static string Help => "Initialize the blog. No do anything.\n"+
    "Use:\n"+
    "// init\n"+
    "// i";
    public override bool Process()
    {
        const string SUCCESS = "Success initialize: ";
        const string FAILED = "Failed initialize: ";
        Configuration.ConfigManager.GetConfigManager();
        Directory.CreateDirectory(JSApi.JSAPiHelper.metasFolder);
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.InitCommand();
            });
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.CommandComplete(Commands.InitCommand);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine($"{FAILED}Executing init command error:\n" + msg);
            return false;
        }
        System.Console.WriteLine($"{SUCCESS}Initialize has been run.");
        return true;
    }
}