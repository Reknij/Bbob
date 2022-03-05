namespace Bbob.Main.Cli;

public class Remove: Command
{
    public new static string Name => "remove";
    public new static string Help => "Remove the theme or plugin. Auto detect.\n"+
    "Use:\n"+
    "// remove <name>";

    public string PluginOrTheme {get;set;}
    private bool isGlobal = false;
    public bool Global
    {
        get=>isGlobal;
        set
        {
            isGlobal = value;
            if (isGlobal)
            {
                DownloadPath.Plugins = Path.Combine(AppContext.BaseDirectory, "plugins");
                DownloadPath.Themes = Path.Combine(AppContext.BaseDirectory, "themes");
            }
            else
            {
                DownloadPath.Plugins = Path.Combine(Environment.CurrentDirectory, "plugins");
                DownloadPath.Themes = Path.Combine(Environment.CurrentDirectory, "themes");
            }
        }
    }
    public static class DownloadPath
    {
        public static string Plugins = Path.Combine(Environment.CurrentDirectory, "plugins");
        public static string Themes = Path.Combine(Environment.CurrentDirectory, "themes");
        public readonly static string Temp = Path.Combine(Environment.CurrentDirectory, "temp");
    }
    public Remove(string PluginOrTheme, bool Global) : base(false)
    {
        this.PluginOrTheme = PluginOrTheme;
        this.Global = Global;
    }

    public override bool Process()
    {
        const string SUCCESS = "Success: ";
        const string FAILED = "Failed: ";

        bool isPlugin = PluginOrTheme.StartsWith("bbob-plugin-");
        bool isTheme = PluginOrTheme.StartsWith("bbob-theme-");
        string directory = Path.Combine(isPlugin?DownloadPath.Plugins: DownloadPath.Themes, PluginOrTheme);
        if (!isPlugin && !isTheme)
        {
            System.Console.WriteLine($"{FAILED}Can't remove because it not plugin or theme.");
            return false;
        }
        if (!Directory.Exists(directory))
        {
            System.Console.WriteLine($"{FAILED}Can't remove because not exists!");
            return false;
        }
        Shared.SharedLib.DirectoryHelper.DeleteDirectory(directory);
        System.Console.WriteLine($"{SUCCESS}Remove done...");
        return true;
    }
}