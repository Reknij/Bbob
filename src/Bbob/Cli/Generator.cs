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
using ConsoleHelper = Bbob.Shared.SharedLib.ConsoleHelper;

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
                ConsoleHelper.printWarning($"<{PluginHelper.ExecutingPlugin.name}> Skip this file.");
                if (!string.IsNullOrWhiteSpace(PluginHelper.ExecutingCommandResult.Message))
                    ConsoleHelper.printWarning($"Message: {PluginHelper.ExecutingCommandResult.Message}");
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
            ConsoleHelper.printWarning($"{FAILED}Not found theme.");
            return false;
        }
        ConsoleHelper.print($"Using theme <{theme.Info.name}>", color:ConsoleColor.Blue);
        ConsoleHelper.print($"Author: {theme.Info.author}", color:ConsoleColor.DarkCyan);
        ConsoleHelper.print($"Description: {theme.Info.description}\n", color:ConsoleColor.DarkCyan);

        InitializeConventionObjects();
        if (files.Length > 0) ConsoleHelper.print($"Run generate all stage for article files in '{articlesFolderPath.Replace(Environment.CurrentDirectory, ".")}'.", color:ConsoleColor.DarkCyan);
        else ConsoleHelper.printWarning("Nothing files to generate.");
        foreach (string file in files)
        {
            string shortFilePath = file.Replace(articlesFolderPath, "").Remove(0, 1);
            ConsoleHelper.print($"- {shortFilePath}", color:ConsoleColor.DarkGray);
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
                        ConsoleHelper.printWarning($"<{PluginHelper.ExecutingPlugin.name}> Stop command execution");
                        if (!string.IsNullOrWhiteSpace(PluginHelper.ExecutingCommandResult.Message))
                            ConsoleHelper.printWarning($"Message: {PluginHelper.ExecutingCommandResult.Message}");
                        ConsoleHelper.printError($"{FAILED}Plugin stop execution.");
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
#if DEBUG
                    msg = ex.ToString();
#endif
                    ConsoleHelper.printError($"{FAILED}Executing generate command of plugin <{PluginHelper.ExecutingPlugin.name}> error in stage {stage}:\n" + msg);
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
            ConsoleHelper.printError($"{FAILED}Error run generate command complete in plugin <{PluginHelper.ExecutingPlugin.name}>:\n" + msg);
            return false;
        }
        string mainName = "bbob.js";
        ConsoleHelper.print($"Building {mainName}");
        string newName = JSAPiHelper.BuildBbobJS(distribution, GetBuildData(), theme.Info);
        JSAPiHelper.Hook(distribution, theme.Info.index, newName);
        if (config.compress)
        {
            if (ThemeProcessor.CompressHtml(distribution)) ConsoleHelper.printSuccess("Compress html success.");
            else ConsoleHelper.printError("Compress html failed.");
        }

        foreach (var a in actions) a();

        DateTime afterGenerateDateTime = DateTime.Now;
        double diff = (afterGenerateDateTime - beforeGenerateDateTime).TotalSeconds;
        ConsoleHelper.printSuccess($"{SUCCESS} Generation has been run. ({string.Format("{0:0.00}", diff)} seconds)");
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
            ConsoleHelper.printError("'blog' is missing!");
        }
        return new BuildData(new ExpandoObject(), PluginHelperCore.metas);
    }
}