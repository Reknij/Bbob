using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class LinkProcess : IPlugin
{
    List<dynamic> links = new List<dynamic>();
    readonly string[] ignoreAttributes =
    {
        "contentParsed",
        "toc"
    };
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage == GenerationStage.Confirm)
        {
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? art);
            if (art != null)
            {
                links.Add(art);
            }
        }
    }

    private bool isIgnore(string attribute)
    {
        foreach (var item in ignoreAttributes)
        {
            if (item == attribute) return true;
        }
        return false;
    }

    public void CommandComplete(Commands command)
    {
        if (command == Commands.GenerateCommand)
        {
            foreach (var link in links)
            {
                var tar = (IDictionary<string, object>)link;
                foreach (var ignore in ignoreAttributes)
                {
                    tar.Remove(ignore);
                }
            }
            sort(links);
            PluginHelper.registerObject("links", links);
            var pack = getLinkInfos(links, PluginHelper.DistributionDirectory);
            dynamic blog = PluginHelper.getRegisteredObjectNoNull<dynamic>("blog");
            blog.links = pack.Item1;
            blog.nextFileLinks = pack.Item2;
            PluginHelper.printConsole($"Resolve {links.Count} files.");
        }
    }

    private void sort(List<dynamic> links)
    {
        links.Sort((art1, art2) =>
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
                return sortLinksDefault(art1, art2);
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

    private int sortLinksDefault(dynamic linkInfo1, dynamic linkInfo2)
    {
        var date1 = linkInfo1.date != null ? DateTime.Parse(linkInfo1.date) : DateTime.Now;
        var date2 = linkInfo2.date != null ? DateTime.Parse(linkInfo2.date) : DateTime.Now;
        if (date1 < date2) return 1;
        if (date1 > date2) return -1;
        return 0;
    }

    private static (List<dynamic>, List<string>) getLinkInfos(List<dynamic> LinkInfos, string dist)
    {
        dist = Path.Combine(dist, BuildInShared.Variables.bbobAssets);

        ConfigJson config = PluginHelper.ConfigBbob;
        List<dynamic> current = new List<dynamic>();
        string nextLinkInfoFilesFolder = "nextLinkInfoFiles";
        string nextLinkInfoFilesFolderLocal = Path.Combine(dist, nextLinkInfoFilesFolder);
        Directory.CreateDirectory(nextLinkInfoFilesFolderLocal);
        int bcotCurrent = config.blogCountOneTime;
        int bcotNext = bcotCurrent;
        switch (config.allLink)
        {
            case "current":
                bcotCurrent = int.MaxValue;
                bcotNext = 0;
                break;
            case "next":
                bcotCurrent = 0;
                bcotNext = int.MaxValue;
                break;
            default:
                break;
        }
        int i = 0;
        for (; i < bcotCurrent && i < LinkInfos.Count; i++)
        {
            current.Add(LinkInfos[i]);
        }
        List<string> nextFileLinkInfos = new List<string>(); //the file LinkInfo for save nextLinkInfos
        while (i < LinkInfos.Count)
        {
            int bcot = i + bcotNext;
            List<dynamic> nextLinkInfos = new List<dynamic>(); //next of LinkInfos
            string nextLinkInfosFile = Path.Combine(nextLinkInfoFilesFolderLocal, "next.temp.json");
            for (; i < LinkInfos.Count && i < bcot; i++)
            {
                nextLinkInfos.Add(LinkInfos[i]);
            }
            string hash = "";
            using (FileStream fs = new FileStream(nextLinkInfosFile, FileMode.Create, FileAccess.ReadWrite))
            {
                JsonSerializer.Serialize(fs, nextLinkInfos);
                fs.Flush(); //If not flush now, hash can't compute
                fs.Position = 0; //set to 0 to read.
                hash = Shared.SharedLib.HashHelper.GetContentHash(fs);
            }
            string newName = $"next-{hash.Substring(0, 9)}.js";
            string newPath = Path.Combine(nextLinkInfoFilesFolderLocal, newName);
            File.Move(nextLinkInfosFile, newPath, true);
            nextFileLinkInfos.Add($"{config.baseUrl}{BuildInShared.Variables.bbobAssets}/{nextLinkInfoFilesFolder}/{newName}");
        }

        return (current, nextFileLinkInfos);
    }
}