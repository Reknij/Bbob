using System.Dynamic;
using System.Security.Cryptography;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class LinkProcess : IPlugin
{
    readonly string[] ignoreAttributes = 
    {
        "contentParsed",
        "toc"
    };
    string distribution = "";
    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        this.distribution = distribution;
        if (stage == GenerationStage.Initialize)
        {
            PluginHelper.modifyRegisteredObject<dynamic>("blog", (ref dynamic? blog) =>
            {
                if (blog == null) return;
                if (Extensions.IsPropertyExists<List<dynamic>>(blog, "links")) return;
                blog.links = new List<dynamic>();
            });
        }
        if (stage == GenerationStage.Confirm)
        {
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? art);
            if (blog != null)
            {
                if (art != null)
                {
                    dynamic link = new ExpandoObject();
                    foreach (var item in (IDictionary<string, object>)art)
                    {
                        if (isIgnore(item.Key) || item.Value == null) continue;
                        ((IDictionary<string, object>)link).Add(item.Key, item.Value);
                    }
                    blog.links.Add(link);
                }
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
            if (PluginHelper.isTargetPluginEnable("SortData") && !PluginHelper.isTargetPluginDone("SortData"))
            {
                PluginHelper.ExecutingCommandResult = new CommandResult("Wait to sort", CommandOperation.RunMeAgain);
                return;
            }
            PluginHelper.getRegisteredObject<dynamic>("blog", out dynamic? blog);
            if (blog != null)
            {
                var pack = getLinkInfos(blog.links, distribution);
                blog.links = pack.Item1;
                blog.nextFileLinks = pack.Item2;
                PluginHelper.printConsole($"Resolve {blog.links.Count} files.");
            }
        }
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
            SHA256 sha256 = SHA256.Create();
            string hash = "";
            using (FileStream fs = new FileStream(nextLinkInfosFile, FileMode.Create, FileAccess.ReadWrite))
            {
                JsonSerializer.Serialize(fs, nextLinkInfos);
                fs.Flush(); //If not flush now, hash can't compute
                fs.Position = 0; //set to 0 to read.
                hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
            }
            string newName = $"next-{hash.Substring(0, 9)}.js";
            string newPath = Path.Combine(nextLinkInfoFilesFolderLocal, newName);
            File.Move(nextLinkInfosFile, newPath);
            nextFileLinkInfos.Add($"{config.baseUrl}{BuildInShared.Variables.bbobAssets}/{nextLinkInfoFilesFolder}/{newName}");
        }

        return (current, nextFileLinkInfos);
    }
}