using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bbob.Main;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using System;
using System.IO;
using static Bbob.Main.ThemeProcessor;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Tests;

[TestClass]
public class Initialize
{
    [AssemblyInitialize]
    public static void MyTestInitialize(TestContext testContext)
    {
        ConfigManager.MainConfig = ConfigManager.DefaultConfig;
        ConfigManager.registerConfigToPluginSystem();
        string themeTest = "./testTheme/theme.json";
        string themeFolder = "./testTheme";
        Directory.CreateDirectory(themeFolder);
        if (File.Exists(themeTest)) File.Delete(themeTest);
        using (FileStream fs = File.OpenWrite(themeTest))
        {
            JsonSerializer.Serialize(fs, new ThemeInfo()
            {
                name = "default",
                description = "default theme for bbob",
                author = "Jinker"
            });
        }
        PluginHelper.ThemePath = themeFolder;
        PluginSystem.LoadAllPlugins();
    }
}