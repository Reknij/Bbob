using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("LinkProcess", PluginStatus = PluginStatus.Done)]
[PluginCondition("SortData", ConditionType = ConditionType.StatusCheck, PluginStatus = PluginStatus.Waiting)]
public class CategoryProcess : IPlugin
{
    string distribution = "";

    public void InitCommand()
    {
        if (!PluginHelper.isPluginJsonConfigExists())
        {
            PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig("default"));
            PluginHelper.printConsole("Initialize config file.");
        }
        else
        {
            PluginHelper.printConsole("Already exists config.");
        }
    }

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        if (stage == GenerationStage.Confirm)
        {
            this.distribution = distribution;

            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) return;

            PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
            MyConfig config = tar ?? new MyConfig("default");
            if (!Extensions.IsPropertyExists<List<object>>(article, "categories")) return;
            var all = (List<object>)article.categories;
            switch (config.mode)
            {
                case "default":
                    break;
                case "folder":
                    string folder = Directory.GetParent(filePath)?.Name ?? "get-folder-fail";
                    all.Clear();
                    all.Add(folder);
                    break;

                default:
                    PluginHelper.printConsole($"Unknown mode '{config.mode}'!");
                    all.Clear();
                    break;
            }
        }
    }

    public void CommandComplete(Commands command)
    {
        if (command == Commands.GenerateCommand)
        {
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            if (blog != null)
            {
                if (PluginHelper.isTargetPluginEnableAndDone("SortData"))
                {
                    blog.categories = FilterSourceHandler.BuildFilterFile(blog.categories, distribution, "categories");
                    return;
                }
                Dictionary<string, List<dynamic>> all = new Dictionary<string, List<dynamic>>();
                if (Extensions.IsPropertyExists<List<dynamic>>(blog, "links", out List<dynamic> links))
                {
                    foreach (var link in links)
                    {
                        if (Extensions.IsPropertyExists<List<object>>(link, "categories", out List<object> categories))
                        {
                            foreach (var t in categories)
                            {
                                if (t is string text)
                                {
                                    if (!all.ContainsKey(text))
                                    {
                                        all.Add(text, new List<dynamic> { link });
                                    }
                                    else all[text].Add(link);
                                }
                            }
                        }
                    }
                }

                blog.categories = all;
                if (PluginHelper.isTargetPluginEnable("SortData"))
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("Wait to sort", CommandOperation.RunMeAgain);
                    return;
                }
                else
                {
                    blog.categories = FilterSourceHandler.BuildFilterFile(blog.categories, distribution, "categories");
                    return;
                }
            }
        }
    }
    public record class MyConfig(string mode);
}