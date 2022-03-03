using System.IO;
using Bbob.Plugin;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using Bbob.Main.JSApi;
using static Bbob.Main.JSApi.JSApiType;
using System.Text.RegularExpressions;
using System.Dynamic;

namespace Bbob.Main.Cli;

public class Generator : Command
{
    public new static string Name => "generate";
    public new static string Help => "Generate the blog to distribution (folder name 'dist').\n" +
    "[option]:\n" +
    "--deploy | -d : Deploy the blog after generation.\n" +
    "--preview | -p : Preview the blog after generation.\n\n" +
    "Use:\n" +
    "// generate [option]\n" +
    "// g [option]";

    string articlesFolderPath;
    public string ArticlesFolderPath { get => articlesFolderPath; }
    string distribution;
    public Generator(string _distribution, string _articlesFolderPath)
    {
        distribution = _distribution;
        articlesFolderPath = _articlesFolderPath;
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
    public override bool Process()
    {
        const string SUCCESS = "Success generate: ";
        const string FAILED = "Failed generate: ";
        if (Directory.Exists(distribution)) Shared.SharedLib.DirectoryHelper.DeleteDirectory(distribution);
        Directory.CreateDirectory(distribution);
        Directory.CreateDirectory(articlesFolderPath);
        var config = ConfigManager.GetConfigManager().MainConfig;

        string[] files = Directory.GetFiles(articlesFolderPath, "*.*", config.recursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        ThemeProcessor.Theme? theme = ThemeProcessor.BuildThemeToDist(config.theme, distribution);
        InitializeConventionObjects();
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
                        System.Console.WriteLine($"{FAILED}Plugin stop execution.");
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
#if DEBUG
                    msg = ex.ToString();
#endif
                    System.Console.WriteLine($"{FAILED}Executing generate command of plugin <{PluginHelper.ExecutingPlugin.name}> error in stage {stage}:\n" + msg);
                    return false;
                }
            }
            if (isSkip()) continue;
        }

        if (theme != null)
        {
            try
            {
                PluginSystem.cyclePlugins((plugin) =>
           {
               plugin.CommandComplete(Commands.GenerateCommand);
           });
            }
            catch (System.Exception ex)
            {
                string msg = ex.Message;
#if DEBUG
                msg = ex.ToString();
#endif
                System.Console.WriteLine($"{FAILED}Error run generate command complete in plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
                return false;
            }
            string mainName = "bbob.js";
            System.Console.WriteLine($"Building {mainName}");
            string newName = JSAPiHelper.BuildBbobJS(distribution, GetBuildData(), theme.Info);
            System.Console.WriteLine($"Hook in {mainName} to index file...");
            JSAPiHelper.Hook(distribution, theme.Info.index, newName);

        }
        else
        {
            System.Console.WriteLine($"{FAILED}Not found theme.");
            return false;
        }
        System.Console.WriteLine($"{SUCCESS} Generation has been run.");
        return true;
    }

    private void InitializeConventionObjects()
    {
        PluginHelper.registerObject("blog", new ExpandoObject());
    }

    private BuildData GetBuildData()
    {
        if (PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog) && blog != null)
        {
            return new BuildData(blog, PluginHelper._getAllMetas());
        }
        else
        {
            System.Console.WriteLine("'blog' is missing!");
        }
        return new BuildData(new ExpandoObject(), PluginHelper._getAllMetas());
    }
}