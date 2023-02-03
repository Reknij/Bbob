namespace Bbob.Main.Cli;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

public class Remove : Command
{
    public new static string Name => "remove";
    public new static string Help => "Remove the theme or plugin. Auto detect.\n" +
    "<option>:\n" +
    "--all-theme : remove all theme.\n" +
    "--all-plugin : remove all plugin.\n" +
    "--global | -g : remove [name] or all plugin from global directory.\n\n" +
    "Use:\n" +
    "// remove [option] [name]";

    private string _pluginOrTheme = string.Empty;
    public string PluginOrTheme
    {
        get => _pluginOrTheme;
        set => _pluginOrTheme = value.ToUpper();
    }

    public bool isAllTheme { get; set; } = false;
    public bool isAllPlugin { get; set; } = false;
    private bool isAll => isAllTheme || isAllPlugin;
    private bool isGlobal = false;
    public bool Global
    {
        get => isGlobal;
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
    public Remove(string PluginOrTheme, bool Global, bool isAllPlugin, bool isAllTheme) : base(false)
    {
        this.PluginOrTheme = PluginOrTheme;
        this.Global = Global;
        this.isAllPlugin = isAllPlugin;
        this.isAllTheme = isAllTheme;
    }

    public override bool Process()
    {
        const string SUCCESS = "Success: ";
        const string FAILED = "Failed: ";

        CliShared.TextType type = isAllPlugin ? CliShared.TextType.Plugin : CliShared.TextType.Theme;
        if (!isAll)
        {
            type = CliShared.isPluginOrThemeName(PluginOrTheme, out string fixedName);
            PluginOrTheme = fixedName;
        }
        string root = type == CliShared.TextType.Theme ? DownloadPath.Themes : DownloadPath.Plugins;
        string p = isGlobal ? "global" : "current";
        string top = type == CliShared.TextType.Theme ? "theme" : "plugin";

        if (isAll)
        {
            if (Directory.Exists(root))
            {
                if (type == CliShared.TextType.Plugin)
                {
                    Shared.SharedLib.DirectoryHelper.DeleteDirectory(root);
                    Directory.CreateDirectory(root);
                }
                else
                {
                    var themes = ThemeProcessor.GetThemes();
                    var filters = from theme in themes where theme.Info.name != "default" select theme;
                    foreach (var filter in filters)
                    {
                        Shared.SharedLib.DirectoryHelper.DeleteDirectory(filter.Path);
                    }
                }
            }

            ConsoleHelper.printSuccess($"{SUCCESS}Remove all {top} from {p} directory.");
            return true;
        }

        string directory = Path.Combine(root, PluginOrTheme);
        if (type == CliShared.TextType.None)
        {
            ConsoleHelper.printError($"{FAILED}Can't remove because it not plugin or theme.");
            return false;
        }
        if (!Directory.Exists(directory))
        {
            ConsoleHelper.printError($"{FAILED}Can't remove because not exists!");
            return false;
        }
        Shared.SharedLib.DirectoryHelper.DeleteDirectory(directory);
        ConsoleHelper.printSuccess($"{SUCCESS}Remove {top} {PluginOrTheme} from {p} directory.");
        return true;
    }
}