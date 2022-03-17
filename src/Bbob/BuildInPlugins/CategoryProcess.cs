using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("LinkProcess", PluginOrder = PluginOrder.BeforeMe)]
public class CategoryProcess : IPlugin
{
    public void InitCommand()
    {
        if (!PluginHelper.isPluginJsonConfigExists())
        {
            PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig());
            PluginHelper.printConsole("Initialize config file.");
        }
        else
        {
            PluginHelper.printConsole("Already exists config.");
        }
    }

    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage == GenerationStage.Confirm)
        {
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) return;

            PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
            MyConfig config = tar ?? new MyConfig();
            switch (config.mode)
            {
                case "default":
                    break;
                case "folder":
                    if (!Extensions.IsPropertyExists<List<object>>(article, "categories")) article.categories = new List<object>();
                    var all = (List<object>)article.categories;
                    string folder = Directory.GetParent(filePath)?.Name ?? "get-folder-fail";
                    all.Clear();
                    all.Add(folder);
                    break;

                default:
                    PluginHelper.printConsole($"Unknown mode '{config.mode}'!");
                    ((IDictionary<string, object>)article).Remove("categories");
                    break;
            }
        }
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
                var list = all.ToList();
                sort(list);
                dynamic blog = PluginHelper.getRegisteredObjectNoNull<dynamic>("blog");
                blog.categories = FilterSourceHandler.BuildFilterFile(list, PluginHelper.DistributionDirectory, "categories");
            }
        }
        return null;
    }
    private void sort(List<KeyValuePair<string, List<dynamic>>> all)
    {
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
        MyConfig config = tar ?? new MyConfig();
        Dictionary<string, int> cs = new Dictionary<string, int>();
        for (int i = 0; i < config.sort.Length; i++)
        {
            cs.Add(config.sort[i], i);
        }
        all.Sort((category1, category2) =>
        {
            if (cs.ContainsKey(category1.Key) && !cs.ContainsKey(category2.Key))
            {
                return -1; //improve
            }
            else if (!cs.ContainsKey(category1.Key) && cs.ContainsKey(category2.Key))
            {
                return 1; //decline
            }
            else if (!cs.ContainsKey(category1.Key) && !cs.ContainsKey(category2.Key))
            {
                return sortCategoriesDefault(category1, category2);
            }
            else if (cs[category1.Key] > cs[category2.Key]) return 1;
            else if (cs[category1.Key] < cs[category2.Key]) return -1;

            return 0;
        });
        PluginHelper.printConsole("Sort the categories.");
    }
    private int sortCategoriesDefault(KeyValuePair<string, List<dynamic>> category1, KeyValuePair<string, List<dynamic>> category2)
    {
        if (category1.Value.Count > category2.Value.Count) return 1;
        if (category1.Value.Count < category2.Value.Count) return -1;
        return 0;
    }
    public class MyConfig
    {
        public string mode { get; set; } = "default";
        public string[] sort { get; set; } = Array.Empty<string>();
    }
}