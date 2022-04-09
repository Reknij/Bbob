using Bbob.Plugin;
using Bbob.Plugin.Cores;
using System.Text.RegularExpressions;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main.Cli;

public class Run : Command
{
    public new static string Name => "Run";
    public new static string Help => "Run custom command from target plugin, if [command [argument]] is null will enter to mode custom command, otherwise direct run command. Or run the global command.\n" +
    "<option>:\n" +
    "name : Name of plugin to run custom command or global option.\n\n" +
    "Use:\n" +
    "// run [-g|--global] <name> [command [argument]]";

    string name;
    bool global;
    string command;
    string[] arguments;
    public Run(string name, string command, string[] arguments, bool global)
    {
        this.name = name.ToUpper();
        this.command = command;
        this.arguments = arguments;
        this.global = global;
    }

    public override bool Process()
    {
        const string SUCCESS = "SUCCESS: ";
        const string FAILED = "FAILED: ";
        if (global)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ConsoleHelper.printError($"{FAILED}Please enter global custom command!");
                return false;
            }
            if (string.IsNullOrWhiteSpace(PluginHelperCore.customGlobalCommandKey))
            {
                ConsoleHelper.printError($"{FAILED}No register any global commands!");
                return false;
            }
            string globalCommand = command + PluginHelperCore.customGlobalCommandKey;
            if (!PluginHelperCore.customCommands.ContainsKey(globalCommand))
            {
                ConsoleHelper.printError($"{FAILED}Target global command is not register!");
                return false;
            }
            foreach (var item in PluginHelperCore.customCommands[globalCommand])
            {
                item.Value(arguments);
            }
            ConsoleHelper.printSuccess($"{SUCCESS} Run global command done.");
            return true;
        }

        if (!PluginHelper.isTargetPluginLoaded(name))
        {
            ConsoleHelper.printError($"{FAILED}Target plugin is unload! Please make sure plugin is exists.");
            return false;
        }
        if (!PluginHelper.isTargetPluginEnable(name))
        {
            ConsoleHelper.printError($"{FAILED}Target plugin is disable! Please enable to try again.");
            return false;
        }
        if (!PluginHelperCore.customCommands.ContainsKey(name))
        {
            ConsoleHelper.printError($"{FAILED}Target plugin is no register any custom command!");
            return false;
        }
        Func<string, bool> existsCommand = (cmd) =>
        {
            if (!PluginHelperCore.customCommands[name].ContainsKey(cmd))
            {
                ConsoleHelper.printError($"{FAILED}Target plugin is no register custom command '{cmd}'.");
                return false;
            }
            return true;
        };

        if (!string.IsNullOrWhiteSpace(command))
        {
            if (!existsCommand(command)) return false;
            PluginHelperCore.customCommands[name][command](arguments);
        }
        else
        {

            string? c = null;
            Action readC = () =>
            {
                ConsoleHelper.print($"{name}: ", false, ConsoleColor.DarkCyan);
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
                PluginHelperCore.customCommands[name][command](arguments);
                readC();
            }
            ConsoleHelper.printWarning("Custom command mode exited.");
        }
        ConsoleHelper.printSuccess($"{SUCCESS}Run custom command from {name} done.");
        return true;
    }
}