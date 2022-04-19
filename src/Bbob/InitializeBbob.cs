using Bbob.Main.PluginManager;
using Bbob.Plugin;
using Bbob.Plugin.Cores;
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

namespace Bbob.Main;

public static class InitializeBbob
{
    [System.Flags]
    public enum InitializeOptions
    {
        Theme = 2,
        Config = 4,
        Plugin = 8,
        ThemePlugin = 10,
        All = Theme | Config | Plugin | ThemePlugin
    }
    public static void Initialize(InitializeOptions options)
    {
        if ((options & InitializeOptions.Config) != 0)
        {
            Configuration.ConfigManager.registerConfigToPluginSystem();
        }
        if ((options & InitializeOptions.Theme) != 0)
        {
            ThemeProcessor.LoadAllTheme();
            if (ThemeProcessor.getTheme(out var info) && info != null)
            {
                PluginHelperCore.themePath = info.Path;
            }
            else
            {
                ConsoleHelper.printWarning($"Not found theme '{Configuration.ConfigManager.MainConfig.theme}'");
                Environment.Exit(-1);
            }
        }
        if ((options & InitializeOptions.Plugin) != 0)
        {
            string themePlugins = "";
            if ((options & InitializeOptions.ThemePlugin) != 0) themePlugins = Path.Combine(PluginHelper.ThemePath, "plugins");
            try
            {
                PluginSystem.LoadAllPlugins(themePlugins);
            }
            catch (System.Exception ex)
            {
                string msg = ex.Message;
#if DEBUG
                msg = ex.ToString();
#endif
                ConsoleHelper.printWarning($"Load plugin and create instance of plugin object error:\n{msg}");
                Environment.Exit(-1);
            }
        }
    }

    public static void registerDataAgain()
    {
        Configuration.ConfigManager.registerConfigToPluginSystem();
    }
}