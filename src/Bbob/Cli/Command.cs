using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Command
{
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