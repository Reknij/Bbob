using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin.BuildInShared;

public static class FilterSourceHandler
{
    public record class FilterSource(string text, string address);
    public static FilterSource[] BuildFilterFile(IDictionary<string, List<dynamic>> dict, string distribution, string name) =>
    BuildFilterFile(dict.ToList(), distribution, name);
    public static FilterSource[] BuildFilterFile(List<KeyValuePair<string, List<dynamic>>> list, string dist, string name)
    {
        List<FilterSource> filterSources = new List<FilterSource>();
        var config = PluginHelper.ConfigBbob;
        string localPath = Path.Combine(dist, Variables.bbobAssets, name);
        Directory.CreateDirectory(localPath);
        foreach (var item in list)
        {
            string vFile = Path.Combine(localPath, $"{item.Key}.json");
            string hash = "";
            using (FileStream fs = new FileStream(vFile, FileMode.Create, FileAccess.ReadWrite))
            {
                JsonSerializer.Serialize(fs, item.Value);
                fs.Flush(); //If not flush now, hash can't compute
                fs.Position = 0; //set to 0 to read.
                hash = Shared.SharedLib.HashHelper.GetContentHash(fs);
            }
            string newName = config.useHashName ? $"{item.Key}-{hash.Substring(0, 9)}.json" : $"{item.Key}.json";
            string newPath = Path.Combine(localPath, newName);
            string webPath = Path.Combine($"{config.baseUrl}{BuildInShared.Variables.bbobAssets}/{name}/{newName}");
            if (vFile != newPath) File.Move(vFile, newPath);
            filterSources.Add(new FilterSource(item.Key, webPath));
        }
        PluginHelper.printConsole($"Resolve {filterSources.Count} {name}.");
        return filterSources.ToArray();
    }
}