using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Deploy : Command
{
    public new static string Name => "deploy";
    public new static string Help => "Deploy the blog.\n"+
    "Use:\n"+
    "// deploy\n"+
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
            System.Console.WriteLine($"{FAILED}Distribution not exists!");
            return false;
        }
        if (Directory.GetFiles(distribution, "*", SearchOption.AllDirectories).Length == 0)
        {
            System.Console.WriteLine($"${FAILED}Distribution is not exists any files!");
            return false;
        }
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.DeployCommand(distribution);
            });
            PluginSystem.cyclePlugins((plugin)=>{
                plugin.CommandComplete(Commands.DeployCommand);
            });
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine($"{FAILED}Error executing plugin deploy command:\n" + msg);
            return false;
        }
        System.Console.WriteLine($"{SUCCESS}Deployment has been run");
        return true;
    }
}