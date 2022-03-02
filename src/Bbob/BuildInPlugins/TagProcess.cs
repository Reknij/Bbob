using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class TagProcess : IPlugin
{
    string distribution = "";

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
         if (stage == GenerationStage.Initialize)
        {
            PluginHelper.modifyRegisteredObject<dynamic>("blog", (ref dynamic? blog) =>
            {
                if (blog == null) return;
                if (Extensions.IsPropertyExists(blog, "tags")) return;
                blog.tags = new Dictionary<string, List<dynamic>>();
            });
        }
        if (stage == GenerationStage.Confirm)
        {
            this.distribution = distribution;
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) return;
            if (blog == null) return;
            if (Extensions.IsPropertyExists<List<object>>(article, "tags", out List<object> tags))
            {
               foreach (var t in tags)
                {
                    if (t is string text)
                    {
                        if (!blog.tags.ContainsKey(text))
                        {
                            blog.tags.Add(text, new List<dynamic> { article });
                        }
                        else blog.tags[text].Add(article);
                    }
                }
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
                blog.tags = FilterSourceHandler.BuildFilterFile(blog.tags, distribution, "tags");
                PluginHelper.printConsole($"Resolve {BuildInShared.SharedFunctions.GetLengthFromAny(blog.tags)} tags.");
            }
        }
    }
}