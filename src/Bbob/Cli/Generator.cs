using System.IO;
using Bbob.Plugin;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using Bbob.Main.JSApi;
using static Bbob.Main.JSApi.JSApiType;
using System.Text.RegularExpressions;

namespace Bbob.Main.Cli;

public class Generator : ICommand
{
    string articlesFolderPath;
    public string ArticlesFolderPath { get => articlesFolderPath; }
    string distribution;

    List<LinkInfo> allLink = new List<LinkInfo>();
    Dictionary<string, List<LinkInfo>> allCategory = new Dictionary<string, List<LinkInfo>>();
    Dictionary<string, List<LinkInfo>> allTag = new Dictionary<string, List<LinkInfo>>();
    public Generator(string _distribution, string _articlesFolderPath)
    {
        distribution = _distribution;
        articlesFolderPath = _articlesFolderPath;
        PluginSystem.LoadAllPlugins();
        ThemeProcessor.LoadAllTheme();
    }
    public bool Process()
    {
        if (Directory.Exists(distribution)) Directory.Delete(distribution, true);
        Directory.CreateDirectory(distribution);
        Directory.CreateDirectory(articlesFolderPath);
        var config = ConfigManager.GetConfigManager().MainConfig;
        registerConfig();

        string[] files = Directory.GetFiles(articlesFolderPath, "*.*", config.recursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        ThemeProcessor.Theme? theme = ThemeProcessor.BuildThemeToDist(config.theme, distribution);
        foreach (string file in files)
        {
            System.Console.WriteLine($"Run generate all stage for <{file}>");
            foreach (GenerationStage stage in Enum.GetValues<GenerationStage>())
            {
               try
               {
                    PluginSystem.cyclePlugins((plugin) =>
                {
                    plugin.GenerateCommand(file, distribution, stage);
                });
               }
               catch (System.Exception ex)
               {
                   System.Console.WriteLine($"Stage {stage} error!");
                   System.Console.WriteLine(ex);
               }
            }

            PluginHelper.getRegisteredObject<LinkInfo>("linkInfo", out var linkInfo);
            if (linkInfo == null || linkInfo.address == null) continue;
            allLink.Add(linkInfo);
            if (linkInfo.categories != null) foreach (var category in linkInfo.categories)
                {
                    if (!allCategory.ContainsKey(category))
                    {
                        allCategory.Add(category, new List<LinkInfo> { linkInfo });
                    }
                    else allCategory[category].Add(linkInfo);
                }

            if (linkInfo.tags != null) foreach (var tag in linkInfo.tags)
                {
                    if (!allTag.ContainsKey(tag))
                    {
                        allTag.Add(tag, new List<LinkInfo> { linkInfo });
                    }
                    else allTag[tag].Add(linkInfo);
                }
        }
        BuildData jsBuildData = SortAll();

        if (theme != null)
        {
            printResult();
            string mainName = "bbob.js";
            System.Console.WriteLine($"Building {mainName}");
            string newName = JSAPiHelper.BuildBbobJS(distribution, jsBuildData, theme.Info);
            System.Console.WriteLine($"Hook in {mainName} to index file...");
            JSAPiHelper.Hook(distribution, theme.Info.index, newName);

        }
        else
        {
            System.Console.WriteLine("Not found theme.");
            return false;
        }

        return true;
    }

    private BuildData SortAll()
    {
        var listCategory = allCategory.ToList();
        var listTag = allTag.ToList();

        if (PluginHelper.sortArticles != null)
        {
            allLink.Sort((item1, item2) => PluginHelper.sortArticles(item1, item2));
            foreach (var category in listCategory)
            {
                category.Value.Sort((item1, item2)=> PluginHelper.sortArticles(item1, item2));
            }
            foreach (var tag in listTag)
            {
                tag.Value.Sort((item1, item2)=> PluginHelper.sortArticles(item1, item2));
            }
        }

        if (PluginHelper.sortCategories != null)
        {
            listCategory.Sort((item1, item2) => PluginHelper.sortCategories(item1, item2));
        }

        if (PluginHelper.sortTags != null)
        {
            listTag.Sort((item1, item2) => PluginHelper.sortTags(item1, item2));
        }

        return new BuildData(allLink, listCategory, listTag);
    }

    private void registerConfig()
    {
        System.Console.WriteLine("Register configs...");
        var properties = typeof(ConfigManager.ConfigJson).GetProperties();
        foreach (var property in properties)
        {
            PluginHelper.registerObject($"config.{property.Name}", property.GetValue(ConfigManager.GetConfigManager().MainConfig));
        }
    }

    private void printResult()
    {
        System.Console.WriteLine($"Resolved {allLink.Count} files.");
        System.Console.WriteLine($"Resolved {allCategory.Count} categories.");
        System.Console.WriteLine($"Resolved {allTag.Count} tags.");
    }
}