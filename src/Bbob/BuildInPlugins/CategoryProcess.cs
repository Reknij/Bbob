using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class CategoryProcess : IPlugin
{
    string distribution = "";

    public void InitCommand()
    {
        PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig("default"));
        PluginHelper.printConsole("Initialize config file.");
    }

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        if (stage == GenerationStage.Initialize)
        {
            PluginHelper.modifyRegisteredObject<dynamic>("blog", (ref dynamic? blog) =>
            {
                if (blog == null) return;
                if (Extensions.IsPropertyExists(blog, "categories")) return;
                blog.categories = new Dictionary<string, List<dynamic>>();
            });
        }
        if (stage == GenerationStage.Confirm)
        {
            this.distribution = distribution;
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) return;
            if (blog == null) return;
            PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
            MyConfig config = tar ?? new MyConfig("default");
            switch (config.mode)
            {
                case "default":
                    {
                        if (Extensions.IsPropertyExists<List<object>>(article, "categories", out List<object> categories))
                        {
                            foreach (var t in categories)
                            {
                                if (t is string text)
                                {
                                    if (!blog.categories.ContainsKey(text))
                                    {
                                        blog.categories.Add(text, new List<dynamic> { article });
                                    }
                                    else blog.categories[text].Add(article);
                                }
                            }
                        }
                        break;
                    }
                case "folder":
                    {
                        string? text = Directory.GetParent(filePath)?.Name;
                        if (text == null)
                        {
                            PluginHelper.printConsole($"Parent folder of file path '{filePath}' is null, can't get the name of category.");
                        }
                        if (!blog.categories.ContainsKey(text))
                        {
                            blog.categories.Add(text, new List<dynamic> { article });
                        }
                        else blog.categories[text].Add(article);
                        break;
                    }
                default:
                    PluginHelper.printConsole("config.mode unknown!");
                    break;
            }
        }
    }

    public void CommandComplete(Commands command)
    {
        if (command == Commands.GenerateCommand)
        {
            if (PluginHelper.isTargetPluginEnable("SortData") && !PluginHelper.isTargetPluginDone("SortData"))
            {
                PluginHelper.ExecutingCommandResult = new CommandResult("Wait to sort", CommandOperation.RunMeAgain);
                return;
            }
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            if (blog != null)
            {
                blog.categories = FilterSourceHandler.BuildFilterFile(blog.categories, distribution, "categories");
                PluginHelper.printConsole($"Resolve {BuildInShared.SharedFunctions.GetLengthFromAny(blog.categories)} categories.");
            }
        }
    }

    public record class MyConfig(string mode);
}