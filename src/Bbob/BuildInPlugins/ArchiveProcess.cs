using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "ArchiveProcess", version = "1.0.0", author = "Jinker")]
[PluginCondition("LinkProcess", PluginOrder = PluginOrder.BeforeMe)]
public class ArchiveProcess : IPlugin
{
    public Action? CommandComplete(Commands command)
    {
        if (command == Commands.GenerateCommand)
        {
            PluginHelper.getRegisteredObject<List<dynamic>>("links", out List<dynamic>? links);
            Dictionary<string, List<dynamic>> archives = new Dictionary<string, List<dynamic>>();
            if (links != null)
            {
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
                dynamic blog = PluginHelper.getRegisteredObjectNoNull<dynamic>("blog");
                blog.archives = FilterSourceHandler.BuildFilterFile(a, PluginHelper.DistributionDirectory, "archives");
            }
        }
        return null;
    }
}