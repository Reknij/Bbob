using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("MarkdownParser", PluginOrder = PluginOrder.BeforeMe, ConditionType = ConditionType.OrderCheck)]
public class MarkdownStyle : IPlugin
{
    readonly string InitDoneFile = Path.Combine(PluginHelper.CurrentDirectory, "BbobInitDone");
    public bool IsInitDone => File.Exists(InitDoneFile);
    public void InitCommand()
    {
        if (!PluginHelper.isPluginJsonConfigExists())
        {
            PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig());
            System.Console.WriteLine("Initialize config file.");
        }
        else
        {
            PluginHelper.printConsole("Already exists config.");
        }
    }

    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage != GenerationStage.FinalProcess) return;

        PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
        if (article == null) return;
        string script = "<script>hljs.highlightAll()</script>";
        article.contentParsed = $"<article class=\"markdown-body\">{article.contentParsed}</article>{script}";
    }

    public void CommandComplete(Commands cmd)
    {
        if (cmd != Commands.GenerateCommand) return;
        string indexFile = Path.Combine(PluginHelper.DistributionDirectory, "index.html");
        string indexPlain = File.ReadAllText(indexFile);
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
        MyConfig config = tar ?? new MyConfig();
        string githubMode = config.mode == "light" ? "" : "-dark";
        string css = $"<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.1.0/github-markdown-{config.mode}.min.css\">" +
                    $"<link rel=\"stylesheet\" href=\"//unpkg.com/@highlightjs/cdn-assets@11.4.0/styles/github{githubMode}.min.css\">";
        string highlight = "<script src=\"//unpkg.com/@highlightjs/cdn-assets@11.4.0/highlight.min.js\"></script>";
        string patternHead = @"<head>(.*)</head>";
        string replacement1 = $"<head>{css}{highlight}$1</head>";
        indexPlain = Regex.Replace(indexPlain, patternHead, replacement1, RegexOptions.Singleline);
        File.WriteAllText(indexFile, indexPlain);
    }

    public class MyConfig
    {
        public string mode {get;set;} = "light";
        public void Recheck()
        {
            if (mode != "light" && mode != "dark")
            {
                PluginHelper.printConsole("Config.mode is not 'light' or 'dark'!");
                PluginHelper.printConsole("Auto reset to 'light'");
                mode = "light";
            }
        }
    }
}