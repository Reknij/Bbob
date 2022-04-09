using Bbob.Plugin;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class Command
{
    public static string Name => "Base command";
    public static string Help => "Here is base command.";
    public Command(bool initialize = true)
    {
        if (initialize)
        {
            PluginHelper.clearAllObject();
            InitializeBbob.registerDataAgain();
        }
    }
    public virtual bool Process()
    {
        return true;
    }
}