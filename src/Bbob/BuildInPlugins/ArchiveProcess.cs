using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class ArchiveProcess : IPlugin
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
            if (!PluginHelper.isTargetPluginEnable("LinkProcess")) 
            {
                PluginHelper.printConsole("LinkProcess disable, please enable to generate archives.");
            }
            if (!PluginHelper.isTargetPluginDone("LinkProcess"))
            {
                 PluginHelper.ExecutingCommandResult = new CommandResult("Wait the links", CommandOperation.RunMeAgain);
                 return;
            }
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            Dictionary<string, List<dynamic>> archives = new Dictionary<string, List<dynamic>>();
            if (blog != null)
            {
                if (blog == null) return;
                var links = (List<dynamic>)blog.links;
                foreach (dynamic link in links)
                {
                    if (Extensions.IsPropertyExists<string>(link, "date", out string dateString))
                    {
                        string text = DateTime.Parse(dateString).Year.ToString();
                        if (!archives.ContainsKey(text))
                        {
                            archives.Add(text, new List<dynamic> { link });
                        }
                        else archives[text].Add(link);
                    }
                }
                var a = archives.ToList();
                a.Sort((v1, v2)=>{
                    if (int.Parse(v1.Key) > int.Parse(v2.Key)) return -1;
                    if (int.Parse(v1.Key) < int.Parse(v2.Key)) return 1;
                    return 0;
                });
                blog.archives = FilterSourceHandler.BuildFilterFile(a, distribution, "archives");
                PluginHelper.printConsole($"Resolve {BuildInShared.SharedFunctions.GetLengthFromAny(blog.archives)} archives.");
            }
        }
    }
}