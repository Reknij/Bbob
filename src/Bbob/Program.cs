using System.Reflection;
using System.Runtime.InteropServices;
using Bbob.Main.Cli;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main;

public class Program
{
    public static string Version => Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "'ErrorGetVersion'";
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "--version":
                case "-v":
                    System.Console.WriteLine($"Bbob v{Program.Version}, {RuntimeInformation.FrameworkDescription}, {Environment.OSVersion}");
                    System.Console.WriteLine($"Base directory: {AppContext.BaseDirectory}");
                    return;
                case ConsoleParser.Commands.Help.Current:
                case ConsoleParser.Commands.Help.CurrentAka:
                    System.Console.WriteLine($"Bbob help:");
                    PrintCommandsHelp();
                    return;
                default:
                    break;
            }
        }
        ConsoleHelper.print($">>> Bbob v{Version} <<<", color:ConsoleColor.Magenta);
        Console.CancelKeyPress += (sender, e) =>
        {
            ConsoleHelper.print($"Bbob has been exited by user.", color:ConsoleColor.Yellow);
        };
        ConsoleParser parser = new ConsoleParser(args);
        parser.Parse();
    }

    private static void PrintCommandsHelp()
    {
        const string Cli = "Bbob.Main.Cli.";
        Assembly main = Assembly.GetExecutingAssembly();
        Type[] types = main.GetTypes();
        List<Type> bip = new List<Type>();
        foreach (var type in types)
        {
            if (type.FullName == null) continue;
            if (type.FullName.StartsWith(Cli) && type.BaseType?.FullName == typeof(Command).FullName)
            {
                string Name = (string?)type.GetProperty("Name", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ?? "";
                string Help = (string?)type.GetProperty("Help", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ?? "";

                System.Console.WriteLine($"Command '{Name}': {Help}");
                System.Console.WriteLine();
                Shared.SharedLib.ConsoleHelper.printDividingLine();
            }
        }
    }
}