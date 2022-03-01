using Bbob.Main.PluginManager;
using Bbob.Plugin;

namespace Bbob.Main;

public static class InitializeBbob
{
    [System.Flags]
    public enum InitializeOptions
    {
        Theme = 2,
        Config = 4,
        Plugin = 8,
        All = Theme | Config | Plugin
    }
    public static void Initialize(InitializeOptions options)
    {
        if ((options & InitializeOptions.Theme) != 0)
        {
            ThemeProcessor.LoadAllTheme();
            if (ThemeProcessor.getTheme(out var info) && info != null)
            {
                PluginHelper.ThemePath = info.Path;
            }
            else
            {
                System.Console.WriteLine($"Not found theme '{Configuration.ConfigManager.GetConfigManager().MainConfig.theme}'");
                Environment.Exit(-1);
            }
        }
        if ((options & InitializeOptions.Config) != 0)
        {
            Configuration.ConfigManager.GetConfigManager().registerConfigToPluginSystem();
        }
        if ((options & InitializeOptions.Plugin) != 0)
        {
            try
            {
                PluginSystem.LoadAllPlugins();
            }
            catch (System.Exception ex)
            {
                string msg = ex.Message;
                #if DEBUG
                    msg = ex.ToString();
                #endif
                System.Console.WriteLine($"Load plugin and create instance of plugin object error:\n{msg}");
                Environment.Exit(-1);
            }
        }

    }

    public static void registerDataAgain()
    {
        Configuration.ConfigManager.GetConfigManager().registerConfigToPluginSystem();
    }
}