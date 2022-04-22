using Bbob.Main.BuildInPlugin.BuildInShared;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "CategoryProcess", version = "1.0.0", author = "Jinker")]
[PluginCondition("LinkProcess", PluginOrder = PluginOrder.BeforeMe)]
public class CategoryProcess : IPlugin
{
    public CategoryProcess()
    {
        PluginHelper.registerCustomCommand("config", (args) =>
        {
            if (args.Length == 2)
            {
                PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
                MyConfig myConfig = tar ?? new MyConfig();
                switch (args[0])
                {
                    case "mode":
                        var value = args[1];
                        if (value != "default" && value != "folder")
                        {
                            PluginHelper.printConsole("mode must is 'default' or 'folder'!", ConsoleColor.Yellow);
                            return;
                        }
                        myConfig.mode = value;
                        break;

                    default:
                        PluginHelper.printConsole($"Unknown config name 'args[0]'!", ConsoleColor.Red);
                        return;
                }
                PluginHelper.printConsole("Config save success!", ConsoleColor.Green);
                PluginHelper.savePluginJsonConfig<MyConfig>(myConfig);
            }
            else
            {
                PluginHelper.printConsole("Please enter config name and value!", ConsoleColor.Red);
            }
        });
    }

    public void InitCommand()
    {
        if (!PluginHelper.isPluginJsonConfigExists())
        {
            PluginHelper.savePluginJsonConfig<MyConfig>(new MyConfig());
            PluginHelper.printConsole("Initialize config file.", ConsoleColor.Green);
        }
        else
        {
            PluginHelper.printConsole("Already exists config.", ConsoleColor.Yellow);
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
                    PluginHelper.printConsole($"Unknown mode '{config.mode}'!", ConsoleColor.Red);
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
                        HashSet<string> addedText = new HashSet<string>();
                        foreach (var t in categories)
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
                dynamic blog = PluginHelper.getRegisteredObjectNotNull<dynamic>("blog");
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