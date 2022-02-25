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

    List<dynamic> allLink = new List<dynamic>();
    Dictionary<string, List<dynamic>> allCategory = new Dictionary<string, List<dynamic>>();
    Dictionary<string, List<dynamic>> allTag = new Dictionary<string, List<dynamic>>();
    public Generator(string _distribution, string _articlesFolderPath)
    {
        distribution = _distribution;
        articlesFolderPath = _articlesFolderPath;
        PluginSystem.LoadAllPlugins();
        ThemeProcessor.LoadAllTheme();
    }

    private bool isSkip(bool displayMessage = true)
    {
        if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Skip)
        {
            if (displayMessage)
            {
                System.Console.WriteLine($"<{PluginHelper.ExecutingPlugin.name}> Skip this file.");
                if (!string.IsNullOrWhiteSpace(PluginHelper.ExecutingCommandResult.Message))
                    System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
            }
            return true;
        }
        return false;
    }
    public bool Process()
    {
        if (Directory.Exists(distribution)) Shared.SharedLib.DirectoryHelper.DeleteDirectory(distribution);
        Directory.CreateDirectory(distribution);
        Directory.CreateDirectory(articlesFolderPath);
        var config = ConfigManager.GetConfigManager().MainConfig;
        ConfigManager.GetConfigManager().MainConfig.registerToPluginSystem();

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
                    if (isSkip(false)) break;
                    if (PluginHelper.ExecutingCommandResult.Operation == CommandOperation.Stop)
                    {
                        System.Console.WriteLine($"<{PluginHelper.ExecutingPlugin.name}> Stop command execution");
                        if (!string.IsNullOrWhiteSpace(PluginHelper.ExecutingCommandResult.Message))
                            System.Console.WriteLine($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
#if DEBUG
                    msg = ex.ToString();
#endif
                    System.Console.WriteLine($"Executing generate functions of plugin error in stage {stage}:\n" + msg);
                    return false;
                }
            }
            if (isSkip()) continue;

            PluginHelper.getRegisteredObject<dynamic>("link", out dynamic? link);
            if (link == null) continue;
            allLink.Add(link);

            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) continue;

            if (((IDictionary<string, object?>)article).TryGetValue("categories", out object? c) && c is List<object> categories)
            {
                foreach (var category in categories)
                {
                    if (category is string text)
                    {
                        if (!allCategory.ContainsKey(text))
                        {
                            allCategory.Add(text, new List<dynamic> { link });
                        }
                        else allCategory[text].Add(link);
                    }
                }
            }

            if (((IDictionary<string, object?>)article).TryGetValue("tags", out object? t) && t is List<object> tags)
            {
                foreach (var tag in tags)
                {
                    if (tag is string text)
                    {
                        if (!allTag.ContainsKey(text))
                        {
                            allTag.Add(text, new List<dynamic> { link });
                        }
                        else allTag[text].Add(link);
                    }
                }
            }
        }
        BuildData jsBuildData;
        try
        {
            jsBuildData = SortAll();
        }
        catch (System.Exception ex)
        {
            string msg = ex.Message;
#if DEBUG
            msg = ex.ToString();
#endif
            System.Console.WriteLine("Executing sort functions error:\n" + msg);
            return false;
        }
        if (theme != null)
        {
            printResult();
            string mainName = "bbob.js";
            System.Console.WriteLine($"Building {mainName}");
            string newName = JSAPiHelper.BuildBbobJS(distribution, jsBuildData, theme.Info);
            System.Console.WriteLine($"Hook in {mainName} to index file...");
            JSAPiHelper.Hook(distribution, theme.Info.index, newName);
            PluginSystem.cyclePlugins((plugin)=>{
                plugin.CommandComplete(Commands.GenerateCommand);
            });
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
                category.Value.Sort((item1, item2) => PluginHelper.sortArticles(item1, item2));
            }
            foreach (var tag in listTag)
            {
                tag.Value.Sort((item1, item2) => PluginHelper.sortArticles(item1, item2));
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

    private void printResult()
    {
        System.Console.WriteLine($"Resolved {allLink.Count} files.");
        System.Console.WriteLine($"Resolved {allCategory.Count} categories.");
        System.Console.WriteLine($"Resolved {allTag.Count} tags.");
    }
}