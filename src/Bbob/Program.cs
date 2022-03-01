using System.Reflection;
using System.Runtime.InteropServices;
using Bbob.Main.Cli;
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
                case "--help":
                case "-h":
                    System.Console.WriteLine($"Bbob help:");
                    PrintCommandsHelp();
                    return;
                default:
                    break;
            }
        }
        Console.WriteLine($"【Bbob v{Version}】");
        ConsoleParser parser = new ConsoleParser(args);
        parser.Parse();
        Console.WriteLine("-----End-----");
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
                printDividingLine();
            }
        }
    }

    private static void printDividingLine() => printDividingLine('-');
    private static void printDividingLine(char dividing) => printDividingLine(dividing.ToString());
    private static void printDividingLine(string dividing)
    {
        for (int i = 0; i < Console.WindowWidth; i++)
        {
            Console.Write(dividing);
        }
        Console.Write('\n');
    }
}