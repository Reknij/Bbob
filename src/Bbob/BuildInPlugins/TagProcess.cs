using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("LinkProcess", PluginOrder = PluginOrder.BeforeMe)]
public class TagProcess : IPlugin
{
    public void InitCommand()
    {
        PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig());
    }

    public Action? CommandComplete(Commands command)
    {
        if (command == Commands.GenerateCommand)
        {
            PluginHelper.getRegisteredObject<List<dynamic>>("links", out List<dynamic>? links);
            if (links != null)
            {
                Dictionary<string, List<dynamic>> all = new Dictionary<string, List<dynamic>>();
                foreach (var link in links)
                {
                    if (Extensions.IsPropertyExists<List<object>>(link, "tags", out List<object> tags))
                    {
                        HashSet<string> addedText = new HashSet<string>();
                        foreach (var t in tags)
                        {
                            if (t is string text && !addedText.Contains(text))
                            {
                                if (!all.ContainsKey(text))
                                {
                                    all.Add(text, new List<dynamic> { link });
                                }
                                else all[text].Add(link);
                                addedText.Add(text);
                            }
                        }
                    }
                }

                var list = all.ToList();
                sort(list);
                dynamic blog = PluginHelper.getRegisteredObjectNoNull<dynamic>("blog");
                blog.tags = FilterSourceHandler.BuildFilterFile(list, PluginHelper.DistributionDirectory, "tags");
            }
        }
        return null;
    }

    private void sort(List<KeyValuePair<string, List<dynamic>>> all)
    {
        PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
        MyConfig config = tar ?? new MyConfig();
        Dictionary<string, int> cs = new Dictionary<string, int>();
        for (int i = 0; i < config.sort.Length; i++)
        {
            cs.Add(config.sort[i], i);
        }
        all.Sort((tag1, tag2) =>
        {
            if (cs.ContainsKey(tag1.Key) && !cs.ContainsKey(tag2.Key))
            {
                return -1; //improve
            }
            else if (!cs.ContainsKey(tag1.Key) && cs.ContainsKey(tag2.Key))
            {
                return 1; //decline
            }
            else if (!cs.ContainsKey(tag1.Key) && !cs.ContainsKey(tag2.Key))
            {
                return sortTagsDefault(tag1, tag2);
            }
            else if (cs[tag1.Key] > cs[tag2.Key]) return 1;
            else if (cs[tag1.Key] < cs[tag2.Key]) return -1;

            return 0;
        });
        PluginHelper.printConsole("Sort the tags.");
    }
    private int sortTagsDefault(KeyValuePair<string, List<dynamic>> tag1, KeyValuePair<string, List<dynamic>> tag2)
    {
        if (tag1.Value.Count > tag2.Value.Count) return 1;
        if (tag1.Value.Count < tag2.Value.Count) return -1;
        return 0;
    }

    public record class MyConfig
    {
        public string[] sort { get; set; } = Array.Empty<string>();
    }
}