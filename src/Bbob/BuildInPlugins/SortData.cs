using System.Dynamic;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class SortData : IPlugin
{
    public void CommandComplete(Commands command)
    {
        if (command != Commands.GenerateCommand) return;
        if (PluginHelper.getRegisteredObject<dynamic>("blog", out var blog))
        {
            if (blog == null) return;

            PluginHelper.getPluginJsonConfig<Config>(out var tar);
            Config config = tar ?? new Config();
            if (Extensions.IsPropertyExists<List<dynamic>>(blog, "links", out List<dynamic>? links))
            {
                links?.Sort((art1, art2) =>
                {
                    var a1 = art1 as IDictionary<string, object?>;
                    var a2 = art2 as IDictionary<string, object?>;
                    const string order = "order";
                    if (a1 == null || a2 == null) return 0;
                    if (a1.ContainsKey(order) && !a2.ContainsKey(order))
                    {
                        return -1; //improve
                    }
                    else if (!a1.ContainsKey(order) && a2.ContainsKey(order))
                    {
                        return 1; //decline
                    }
                    else if (!a1.ContainsKey(order) && !a2.ContainsKey(order))
                    {
                        return sortArticlesDefault(art1, art2);
                    }
                    else if (!(a1[order] is int) || !(a2[order] is int))
                    {
                        string title = (a1[order] is int) == false ? art1.title : art2.title;
                        PluginHelper.printConsole($"{order} article with title {title} is not numbers");
                    }
                    else if (art1.order > art2.order) return 1;
                    else if (art1.order < art2.order) return -1;

                    return 0;
                });
                PluginHelper.printConsole("Sort the links.");
            }
            if (Extensions.IsPropertyExists<IDictionary<string, List<dynamic>>>(blog, "categories", out IDictionary<string, List<dynamic>>? allCategory))
            {
                var categories = allCategory?.ToList() ?? new List<KeyValuePair<string, List<dynamic>>>();
                Dictionary<string, int> cs = new Dictionary<string, int>();
                for (int i = 0; i < config.categories.Length; i++)
                {
                    cs.Add(config.categories[i], i);
                }
                categories.Sort((category1, category2) =>
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
                blog.categories = categories;
                PluginHelper.printConsole("Sort the categories.");
            }
            if (Extensions.IsPropertyExists<IDictionary<string, List<dynamic>>>(blog, "tags", out IDictionary<string, List<dynamic>>? allTag))
            {
                var tags = allTag?.ToList() ?? new List<KeyValuePair<string, List<dynamic>>>();
                Dictionary<string, int> cs = new Dictionary<string, int>();
                for (int i = 0; i < config.tags.Length; i++)
                {
                    cs.Add(config.tags[i], i);
                }
                tags.Sort((tag1, tag2) =>
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
                        return sortCategoriesDefault(tag1, tag2);
                    }
                    else if (cs[tag1.Key] > cs[tag2.Key]) return 1;
                    else if (cs[tag1.Key] < cs[tag2.Key]) return -1;

                    return 0;
                });
                blog.tags = tags;
                PluginHelper.printConsole("Sort the tags.");
            }
        }

    }

    private int sortArticlesDefault(dynamic linkInfo1, dynamic linkInfo2)
    {
        var date1 = linkInfo1.date != null ? DateTime.Parse(linkInfo1.date) : DateTime.Now;
        var date2 = linkInfo2.date != null ? DateTime.Parse(linkInfo2.date) : DateTime.Now;
        if (date1 < date2) return 1;
        if (date1 > date2) return -1;
        return 0;
    }

    private int sortCategoriesDefault(KeyValuePair<string, List<dynamic>> category1, KeyValuePair<string, List<dynamic>> category2)
    {
        if (category1.Value.Count > category2.Value.Count) return 1;
        if (category1.Value.Count < category2.Value.Count) return -1;
        return 0;
    }

    private int sortTagsDefault(KeyValuePair<string, List<dynamic>> tag1, KeyValuePair<string, List<dynamic>> tag2)
    {
        if (tag1.Value.Count > tag2.Value.Count) return 1;
        if (tag1.Value.Count < tag2.Value.Count) return -1;
        return 0;
    }

    private class Config
    {
        public string[] categories { get; set; } = Array.Empty<string>();
        public string[] tags { get; set; } = Array.Empty<string>();
    }
}