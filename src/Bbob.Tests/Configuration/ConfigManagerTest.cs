using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bbob.Main.Configuration;

namespace Bbob.Tests.Configuration;

[TestClass]
public class ConfigManagerTest
{
    [TestMethod]
    public void TestGetConfigDefault()
    {
        ConfigManager.ConfigPath = "unknown";
        ConfigManager main = ConfigManager.GetConfigManager();
        ConfigManager.ConfigJson mainConfig = main.MainConfig;

        var propertyInfos = mainConfig.GetType().GetProperties();
        foreach (var item in propertyInfos)
        {
            Assert.IsNotNull(item.GetValue(mainConfig));
        }
    }
}