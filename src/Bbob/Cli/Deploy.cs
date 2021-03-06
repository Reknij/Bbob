using Bbob.Main.PluginManager;
using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class Deploy : Command
{
    public new static string Name => "deploy";
    public new static string Help => "Deploy the blog.\n" +
    "Use:\n" +
    "// deploy\n" +
    "// d";
    string distribution;

    public Deploy(string dist)
    {
        distribution = dist;
    }
    public override bool Process()
    {
        const string SUCCESS = "Success deploy: ";
        const string FAILED = "Failed deploy: ";
        if (!Directory.Exists(distribution))
        {
            ConsoleHelper.printError($"{FAILED}Distribution not exists!");
            return false;
        }
        if (Directory.GetFiles(distribution, "*", SearchOption.AllDirectories).Length == 0)
        {
            ConsoleHelper.printError($"${FAILED}Distribution is not exists any files!");
            return false;
        }
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.DeployCommand();
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            ConsoleHelper.printError($"{FAILED}Error run deploy command in plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        List<Action> actions = new List<Action>();
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.CommandComplete(Commands.DeployCommand);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            ConsoleHelper.printError($"{FAILED}Error run deploy command complete in plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        foreach (var a in actions) a();
        ConsoleHelper.printSuccess($"{SUCCESS}Deployment has been run");
        return true;
    }
}