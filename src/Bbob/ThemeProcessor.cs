using System.Dynamic;
using System.Text.Json;
using Bbob.Plugin;
using Bbob.Shared;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main;

public static class ThemeProcessor
{
    static readonly string themesFolder = Path.Combine(AppContext.BaseDirectory, "themes"); //themes in base of Bbob directory.
    static readonly string thirdThemesFolder = Path.Combine(Environment.CurrentDirectory, "themes"); //themes in base of Bbob directory.
    public static bool ShowLoadedMessage {get;set;} = true;
    static Dictionary<string, Theme> themes = new Dictionary<string, Theme>();
    public static void LoadAllTheme()
    {
        if (ShowLoadedMessage) ConsoleHelper.printSuccess("Loading Themes...");
        themes.Clear();
        Directory.CreateDirectory(themesFolder);
        Directory.CreateDirectory(thirdThemesFolder);
        List<string> directories = new List<string>(Directory.GetDirectories(themesFolder));
        directories.AddRange(Directory.GetDirectories(thirdThemesFolder));
        foreach (var dir in directories)
        {
            if (IsTheme(dir, out ThemeInfo themeInfo))
            {
                if (themes.ContainsKey(themeInfo.name)) themes.Remove(themeInfo.name);
                themes.Add(themeInfo.name, new Theme(themeInfo, dir));
            }
        }
    }

    public static Theme? BuildThemeToDist(string themeName, string dist)
    {
        if (themes.TryGetValue(themeName, out Theme? theme))
        {
            if (theme != null)
            {
                string themeJson = Path.Combine(theme.Path, "theme.json");
                SharedLib.DirectoryHelper.CopyDirectory(theme.Path, dist, themeJson, true, true);
                return theme;
            }
        }
        return null;
    }
    public static bool CompressHtml(string dist) => CompressHtml(Configuration.ConfigManager.MainConfig.theme, dist);
    public static bool CompressHtml(string themeName, string dist)
    {
        getTheme(themeName, out Theme? t);
        if (t == null) return false;

        string index = Path.Combine(dist, t.Info.index);
        string content = File.ReadAllText(index);
        try
        {
            content = NUglify.Uglify.Html(content).Code;
            File.WriteAllText(index, content);
        }
        catch (System.Exception ex)
        {
            ConsoleHelper.printError($"Compress {t.Info.index} error:\n{ex.Message}");
            return false;
        }
        return true;
    }

    public static bool getTheme(out Theme? info) => getTheme(Configuration.ConfigManager.MainConfig.theme, out info);
    public static bool getTheme(string themeName, out Theme? info)
    {
        info = null;
        if (themes.TryGetValue(themeName, out Theme? theme))
        {
            if (theme != null)
            {
                info = theme;
                return true;
            }
        }
        return false;
    }

    private static bool IsTheme(string dir, out ThemeInfo themeInfo)
    {
        string themeJson = Path.Combine(dir, "theme.json");
        if (File.Exists(themeJson))
        {
            using (FileStream fs = new FileStream(themeJson, FileMode.Open, FileAccess.Read))
            {
                ThemeInfo? info = JsonSerializer.Deserialize<ThemeInfo>(fs);
                if (info != null)
                {
                    themeInfo = info;
                    return true;
                }
            }
        }
        themeInfo = new ThemeInfo();
        return false;
    }

    public class Theme
    {
        public ThemeInfo Info { get; set; }
        public string Path { get; set; }

        public Theme(ThemeInfo info, string path)
        {
            Info = info;
            Path = path;
        }
    }

    public class ThemeInfo
    {
        private string? _name;
        public string name
        {
            get => _name ?? throw new NullReferenceException("Theme name is null");
            set => _name = value;
        }
        public string index { get; set; }
        public string description { get; set; }
        public string author { get; set; }

        public ThemeInfo()
        {
            index = "index.html";
            description = "Nothing.";
            author = "Unknown author";
        }
    }
}