using System.Dynamic;
using System.Text.Json;
using Bbob.Plugin;
using Bbob.Shared;

namespace Bbob.Main;

public static class ThemeProcessor
{
    static readonly string themesFolder = Path.Combine(AppContext.BaseDirectory, "themes"); //themes in base of Bbob directory.

    static Dictionary<string, Theme> themes = new Dictionary<string, Theme>();
    public static void LoadAllTheme()
    {
        System.Console.WriteLine("Loading Themes...");
        themes.Clear();
        Directory.CreateDirectory(themesFolder);
        string[] directories = Directory.GetDirectories(themesFolder);
        foreach (var dir in directories)
        {
            if (IsTheme(dir, out ThemeInfo themeInfo))
            {
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
                SharedLib.DirectoryHelper.CopyDirectory(theme.Path, dist, themeJson);
                return theme;
            }
        }
        return null;
    }

    public static bool getTheme(out Theme? info) => getTheme(Configuration.ConfigManager.GetConfigManager().MainConfig.theme, out info);
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