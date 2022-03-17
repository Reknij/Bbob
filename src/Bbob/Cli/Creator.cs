using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Creator : Command
{
    public new static string Name => "new";
    public new static string Help => "Create the blog. Default create blog.\n" +
    "[option]:\n" +
    "--blog | -b : create article.\n\n" +
    "Use:\n" +
    "// new [option] [blogName]";
    NewTypes newType;
    public NewTypes NewType { get => newType; }
    string filename;
    public string FileName { get => filename; }
    string articlesFolderPath;
    public string ArticlesFolderPath { get => articlesFolderPath; }
    public Creator(string? _filename, string _articlesFolderPath, NewTypes NewType = NewTypes.blog)
    {
        newType = NewType;
        filename = _filename ?? Shared.SharedLib.DateTimeHelper.GetDateTimeNowString().Replace(':', '-');
        articlesFolderPath = _articlesFolderPath;
    }
    public override bool Process()
    {
        const string FAILED = "Failed create: ";
        const string SUCCESS = "Success create: ";
        CheckExists();
        string filePath = Path.Combine(articlesFolderPath, filename + ".md");
        if (File.Exists(filePath))
        {
            System.Console.WriteLine($"{FAILED}Already exists target article.");
            return false;
        }
        System.Console.WriteLine($"Processing <{filename}> file content...");
        string content = "";
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                plugin.NewCommand(filePath, ref content, NewType);
            });
            if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Skip ||
                PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Stop)
            {
                System.Console.WriteLine($"<{PluginHelper.ExecutingPlugin.name}> Stop command execution");
                if (!string.IsNullOrWhiteSpace(PluginHelper.ExecutingCommandResult.Message))
                    System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                System.Console.WriteLine($"{FAILED}Plugin stop execution."); ;
                return false;
            }
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine($"{FAILED}Error run new command of plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        List<Action> actions = new List<Action>();
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                var a = plugin.CommandComplete(Commands.NewCommand);
                if (a != null)actions.Add(a);
            });
            File.WriteAllText(filePath, content);
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine($"{FAILED}Error run new command complete of plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        foreach (var a in actions) a();
        System.Console.WriteLine($"{SUCCESS}Article file in '{filePath}'");
        return true;
    }

    private void CheckExists()
    {
        if (!Directory.Exists(articlesFolderPath)) Directory.CreateDirectory(articlesFolderPath);
    }
}