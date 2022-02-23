using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main.Cli;

public class Creator : ICommand
{
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
        PluginSystem.LoadAllPlugins();
    }
    public bool Process()
    {
        CheckExists();
        string filePath = Path.Combine(articlesFolderPath, filename + ".md");
        if (File.Exists(filePath))
        {
            System.Console.WriteLine("Already exists target article.");
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
                return false;
            }
            File.WriteAllText(filePath, content);
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine("Error executing plugin new command:\n" + msg);
            return false;
        }
        return true;
    }

    private void CheckExists()
    {
        if (!Directory.Exists(articlesFolderPath)) Directory.CreateDirectory(articlesFolderPath);
    }
}