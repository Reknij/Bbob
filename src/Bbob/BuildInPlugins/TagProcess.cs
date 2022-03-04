using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("LinkProcess", PluginOrder = PluginOrder.BeforeMe)]
[PluginCondition("SortData", ConditionType = ConditionType.StatusCheck, PluginOrder = PluginOrder.AfterMe)]
public class TagProcess : IPlugin
{
    string distribution = "";

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        if (stage == GenerationStage.Confirm)
        {
            this.distribution = distribution;
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
                    blog.tags = FilterSourceHandler.BuildFilterFile(blog.tags, distribution, "tags");
                    return;
                }
                Dictionary<string, List<dynamic>> all = new Dictionary<string, List<dynamic>>();
                if (Extensions.IsPropertyExists<List<dynamic>>(blog, "links", out List<dynamic> links))
                {
                    foreach (var link in links)
                    {
                        if (Extensions.IsPropertyExists<List<object>>(link, "tags", out List<object> tags))
                        {
                            foreach (var t in tags)
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

                blog.tags = all;
                if (PluginHelper.isTargetPluginEnable("SortData"))
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("Wait to sort", CommandOperation.RunMeAgain);
                    return;
                }
                else
                {
                    blog.tags = FilterSourceHandler.BuildFilterFile(blog.tags, distribution, "tags");
                    return;
                }
            }
        }
    }
}