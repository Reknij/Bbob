using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Command
{
    public static string Name => "Base command";
    public static string Help => "Here is base command.";
    public Command()
    {
        PluginHelper.clearAllObject();
        InitializeBbob.registerDataAgain();
    }
    public virtual bool Process()
    {
        return true;
    }
}