namespace Bbob.Main.Cli;

public static class CliShared
{
    public enum TextType
    {
        Theme, Plugin, None
    }
    public static TextType isPluginOrThemeName(string name)
    {
        if (name.StartsWith("bbob-theme-")) return TextType.Theme;
        if (name.StartsWith("bbob-plugin-")) return TextType.Plugin;

        return TextType.None;
    }
}