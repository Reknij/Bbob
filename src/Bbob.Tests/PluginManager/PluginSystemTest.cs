using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bbob.Main.PluginManager;
using Bbob.Plugin;
using System.IO;
using System;
using Bbob.Main.Cli;

namespace Bbob.Tests.PluginManager;

[TestClass]
public class PluginSystemTest
{
    readonly string testMDPath = Path.Combine(Environment.CurrentDirectory, "Test.md");
    readonly string testHtmlPath = Path.Combine(Environment.CurrentDirectory, "Test.html");

    readonly string afp = Path.Combine(Environment.CurrentDirectory, "articles");

    private void ClearRubbish()
    {
        foreach (string file in Directory.GetFiles(afp))
        {
            File.Delete(file);
        }
        if (Directory.Exists(afp)) Directory.Delete(afp);
    }
    [TestMethod]
    public void IsSuccessCreateDefaultMarkdownFile()
    {
        ClearRubbish();
        Creator creator = new Creator(null, afp, Plugin.NewTypes.blog);
        string filePath = Path.Combine(afp, $"{creator.FileName}.md");
        creator.Process();
        Assert.IsTrue(File.Exists(filePath));
    }
    [TestMethod]
    public void IsSuccessCreateTargetMarkdownFile()
    {
        ClearRubbish();
        string filename = "TestFile";
        string filePath = Path.Combine(afp, $"{filename}.md");
        Creator creator = new Creator(filename, afp, Plugin.NewTypes.blog);
        creator.Process();
        Assert.IsTrue(File.Exists(filePath));
    }

    [TestMethod]
    public void IsPluginCountSame()
    {
        int expectBIP = 3;
        int expectTP = 2;
        Assert.AreEqual(expectBIP, PluginSystem.BuildInPluginCount);
        Assert.AreEqual(expectTP, PluginSystem.ThirdPluginCount);
        Assert.AreEqual(expectBIP + expectTP, PluginSystem.AllPluginCount);
    }

    [TestMethod]
    public void RunGenerateStage1()
    {
        PluginSystem.cyclePlugins((IPlugin plugin) =>
        {
            plugin.GenerateCommand(testMDPath, Path.Combine(Environment.CurrentDirectory, "dist"), GenerationStage.Initialize);
        });
        Assert.IsTrue(PluginHelper.getRegisteredObject<string>("markdown", out string? markdown));
        Assert.IsNotNull(markdown);
    }

    [TestMethod]
    public void RunGenerateStage2()
    {
        PluginSystem.cyclePlugins((IPlugin plugin) =>
        {

            plugin.GenerateCommand(testMDPath, Path.Combine(Environment.CurrentDirectory, "dist"), GenerationStage.Process);
        });
        PluginHelper.getRegisteredObject<string>("markdown", out string? markdown);
        using (StreamReader sr = new StreamReader(testMDPath))
        {
            string emarkdown = sr.ReadToEnd().Replace("title", "ttat").Replace("paragraph", "ppap");
            emarkdown = emarkdown.Substring(43);
            Assert.AreEqual(emarkdown, markdown);
        }
        Assert.IsTrue(PluginHelper.getRegisteredObject<string>("injectData", out string? value));
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void RunGenerateStage3()
    {
        PluginSystem.cyclePlugins((IPlugin plugin) =>
        {

            plugin.GenerateCommand(testMDPath, Path.Combine(Environment.CurrentDirectory, "dist"), GenerationStage.Parse);
        });
        Assert.IsTrue(PluginHelper.getRegisteredObject<string>("contentParsed", out var contentParsed));
        Assert.IsNotNull(contentParsed);
    }

    [TestMethod]
    public void RunGenerateStage4()
    {
        PluginSystem.cyclePlugins((IPlugin plugin) =>
        {

            plugin.GenerateCommand(testMDPath, Path.Combine(Environment.CurrentDirectory, "dist"), GenerationStage.FinalProcess);
        });
        PluginHelper.getRegisteredObject<string>("contentParsed", out var contentParsed);
        using (StreamReader sr = new StreamReader(testHtmlPath))
        {
            string n = "Hello-World-plus-2022";
            string ehtml = sr.ReadToEnd().Replace("class", "id").Replace("bbob", n).Replace("title", "ttat").Replace("paragraph", "ppap");
            Assert.AreEqual(ehtml, contentParsed);
        }
    }

    [TestMethod]
    public void IsHaveFunctions()
    {
        Assert.IsTrue(PluginHelper.getRegisteredObject<Func<Type, object>>("getYamlObject", out var getYamlObject));
        Assert.IsNotNull(getYamlObject);
    }
}