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
        PluginSystem.cyclePlugins((plugin) =>
        {
            plugin.NewCommand(filePath, ref content, NewType);
        });
        File.WriteAllText(filePath, content);
        return true;
    }

    private void CheckExists()
    {
        if (!Directory.Exists(articlesFolderPath)) Directory.CreateDirectory(articlesFolderPath);
    }
}