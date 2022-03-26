using Bbob.Plugin;
using Bbob.Plugin.Cores;
using System.Text.RegularExpressions;

namespace Bbob.Main.Cli;

public class Run : Command
{
    public new static string Name => "Run";
    public new static string Help => "Run custom command from target plugin. if [command [argument]] is null will enter to mode custom command, otherwise direct run command.\n" +
    "<option>:\n" +
    "pluginName : Plugin to run custom command.\n\n" +
    "Use:\n" +
    "// run <pluginName> [command [argument]]";

    string pluginName;
    string command;
    string[] arguments;
    public Run(string pluginName, string command, string[] arguments)
    {
        this.pluginName = pluginName.ToUpper();
        this.command = command;
        this.arguments = arguments;
    }

    public override bool Process()
    {
        const string SUCCESS = "SUCCESS: ";
        const string FAILED = "FAILED: ";

        if (!PluginHelper.isTargetPluginLoaded(pluginName))
        {
            System.Console.WriteLine($"{FAILED}Target plugin is unload! Please make sure plugin is exists.");
            return false;
        }
        if (!PluginHelper.isTargetPluginEnable(pluginName))
        {
            System.Console.WriteLine($"{FAILED}Target plugin is disable! Please enable to try again.");
            return false;
        }
        if (!PluginHelperCore.customCommands.ContainsKey(pluginName))
        {
            System.Console.WriteLine($"{FAILED}Target plugin is no register any custom command!");
            return false;
        }
        Func<string, bool> existsCommand = (cmd) =>
        {
            if (!PluginHelperCore.customCommands[pluginName].ContainsKey(cmd))
            {
                System.Console.WriteLine($"{FAILED}Target plugin is no register custom command '{cmd}'.");
                return false;
            }
            return true;
        };

        if (!string.IsNullOrWhiteSpace(command))
        {
            if (!existsCommand(command)) return false;
            PluginHelperCore.customCommands[pluginName][command](arguments);
        }
        else
        {

            string? c = null;
            Action readC = () =>
            {
                System.Console.Write($"{pluginName}: ");
                c = Console.ReadLine();
            };
            readC();
            while (c != "exit")
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    readC();
                    continue;
                }
                string[] args = Regex.Replace(c, @"\s+", "").Split(' ');
                command = args[0];
                if (!existsCommand(command))
                {
                    readC();
                    continue;
                }
                Array.Copy(args, 1, arguments = new string[args.Length - 1], 0, arguments.Length);
                PluginHelperCore.customCommands[pluginName][command](arguments);
                readC();
            }
            System.Console.WriteLine("Custom command mode exited.");
        }
        System.Console.WriteLine($"{SUCCESS}Run custom command from {pluginName} done.");
        return true;
    }
}