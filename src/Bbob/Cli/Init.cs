using Bbob.Main.PluginManager;
using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

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
        Configuration.ConfigManager.LoadConfigs();
        Directory.CreateDirectory(JSApi.JSAPiHelper.metasFolder);
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.InitCommand();
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            ConsoleHelper.printError($"{FAILED}Error run init command plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        List<Action> actions = new List<Action>();
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                var a = plugin.CommandComplete(Commands.InitCommand);
                if (a != null) actions.Add(a);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            ConsoleHelper.printError($"{FAILED}Error run init command complete plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        foreach (var a in actions) a();
        ConsoleHelper.printSuccess($"{SUCCESS}Initialize has been run.");
        return true;
    }
}