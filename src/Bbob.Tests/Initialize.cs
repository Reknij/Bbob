using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bbob.Main;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using System;
using System.IO;

namespace Bbob.Tests;

[TestClass]
public class Initialize
{
    [AssemblyInitialize]
    public static void MyTestInitialize(TestContext testContext)
    {
        ConfigManager.GetConfigManager().MainConfig = ConfigManager.GetConfigManager().DefaultConfig;
        PluginSystem.LoadAllPlugins();
        ThemeProcessor.LoadAllTheme();
    }
}