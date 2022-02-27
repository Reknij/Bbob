using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bbob.Main;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using System;
using System.IO;
using static Bbob.Main.ThemeProcessor;

namespace Bbob.Tests;

[TestClass]
public class Initialize
{
    [AssemblyInitialize]
    public static void MyTestInitialize(TestContext testContext)
    {
        ConfigManager.GetConfigManager().MainConfig = ConfigManager.GetConfigManager().DefaultConfig;
        ConfigManager.GetConfigManager().registerConfigToPluginSystem();
        ThemeProcessor.registerThemeInfoToPluginSystem(new ThemeInfo()
        {
            name = "default",
            description = "default theme for bbob",
            author = "Jinker"
        });
        PluginSystem.LoadAllPlugins();
    }
}