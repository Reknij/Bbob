using System.IO;
using Bbob.Plugin;
using Bbob.Main.PluginManager;
using Bbob.Main.Configuration;
using Bbob.Main.JSApi;
using static Bbob.Main.JSApi.JSApiType;
using System.Text.RegularExpressions;
using System.Dynamic;
using System.Text.Json;
using System.Security.Cryptography;
using Bbob.Plugin.Cores;

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
        DateTime beforeGenerateDateTime = DateTime.Now;
        if (Directory.Exists(distribution)) Shared.SharedLib.DirectoryHelper.DeleteDirectory(distribution);
        Directory.CreateDirectory(distribution);
        Directory.CreateDirectory(articlesFolderPath);
        var config = ConfigManager.MainConfig;

        string[] files = Directory.GetFiles(articlesFolderPath, "*.*", config.recursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        ThemeProcessor.Theme? theme = ThemeProcessor.BuildThemeToDist(config.theme, distribution);
        if (theme == null)
        {
            System.Console.WriteLine($"{FAILED}Not found theme.");
            return false;
        }
        InitializeConventionObjects();
        if (files.Length > 0) System.Console.WriteLine($"Run generate all stage for article files in '{articlesFolderPath.Replace(Environment.CurrentDirectory, ".")}'.");
        else System.Console.WriteLine("Nothing files to generate.");
        foreach (string file in files)
        {
            string shortFilePath = file.Replace(articlesFolderPath, "").Remove(0, 1);
            System.Console.WriteLine($"- {shortFilePath}");
            foreach (GenerationStage stage in Enum.GetValues<GenerationStage>())
            {
                try
                {
                    PluginSystem.cyclePlugins((plugin) =>
                    {
                        plugin.GenerateCommand(file, stage);
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
        List<Action> actions = new();
        try
        {
            PluginSystem.cyclePlugins((plugin) =>
            {
                var a = plugin.CommandComplete(Commands.GenerateCommand);
                if (a != null) actions.Add(a);
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
        if (config.compress)
        {
            if (ThemeProcessor.CompressHtml(distribution)) System.Console.WriteLine("Compress html success.");
            else System.Console.WriteLine("Compress html failed.");
        }

        foreach (var a in actions) a();

        DateTime afterGenerateDateTime = DateTime.Now;
        double diff = (afterGenerateDateTime - beforeGenerateDateTime).TotalSeconds;
        System.Console.WriteLine($"{SUCCESS} Generation has been run. ({string.Format("{0:0.00}", diff)} seconds)");
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
            return new BuildData(blog, PluginHelperCore.metas);
        }
        else
        {
            System.Console.WriteLine("'blog' is missing!");
        }
        return new BuildData(new ExpandoObject(), PluginHelperCore.metas);
    }
}