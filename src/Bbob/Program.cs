using System.Reflection;
using Bbob.Main.PluginManager;

namespace Bbob.Main;

public class Program
{
    public static void Main(string[] args)
    {
        string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "'ErrorGetVersion'";
        Console.WriteLine($"【Bbob v{version}】");
        ConsoleParser parser = new ConsoleParser(args);
        parser.Parse();
        Console.WriteLine("-----End-----");
    }
}