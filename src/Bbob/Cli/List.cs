using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class List : Command
{
    public enum DataType
    {
        Plugins
    }
    public new static string Name => "list";
    public new static string Help => "List the data.\n" +
    "<option>:\n" +
    "--plugins | -p : list the loaded plugins. It is processed plugins list.\n\n" +
    "Use:\n" +
    "// list <option>\n" +
    "// l <option>";

    public DataType dataType { get; set; }
    public List(DataType type)
    {
        dataType = type;
    }
    public override bool Process()
    {
        switch (dataType)
        {
            case DataType.Plugins:
                Shared.SharedLib.ConsoleHelper.printDividingLine();
                PluginSystem.printAllPlugin();
                Shared.SharedLib.ConsoleHelper.printDividingLine();
                break;

            default: break;
        }
        return true;
    }
}