using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Main.Configuration;
using static Bbob.Main.JSApi.JSApiType;
using Bbob.Plugin;
using System.Security.Cryptography;
using System.Dynamic;

namespace Bbob.Main.JSApi;

public static class JSAPiHelper
{
    public static readonly string metasFolder = Path.Combine(Environment.CurrentDirectory, "metas");
    public static readonly string bbobAssets = "bbob.assets";

    public static void Hook(string dist, string indexName, string hookFile)
    => Hook(dist, indexName, new string[] { hookFile });
    public static void Hook(string dist, string indexName, string[] hookFiles)
    {
        if (hookFiles.Length > 0)
        {
            var config = ConfigManager.GetConfigManager().MainConfig;
            string indexPath = Path.Combine(dist, indexName);
            string indexHtml = File.ReadAllText(indexPath);
            string src = "";
            foreach (string jsFile in hookFiles)
            {
                System.Console.WriteLine($"Hook {jsFile} to {indexName}");
                src += $"\n<script src=\"{config.baseUrl}{jsFile}\"></script>";
            }
            string pattern = @"<head>(.*)</head>";
            string replacement = $"<head>{src}$1</head>";
            indexHtml = Regex.Replace(indexHtml, pattern, replacement, RegexOptions.Singleline);
            indexHtml = changeIndexTitle(indexHtml, ConfigManager.GetConfigManager().MainConfig.blogName);
            File.WriteAllText(indexPath, indexHtml);
        }
    }

    private static string changeIndexTitle(string indexHtml, string title)
    {
        string pattern = @"<title>.*</title>";
        string replacement = $"<title>{title}</title>";
        return Regex.Replace(indexHtml, pattern, replacement, RegexOptions.Singleline);
    }

    public static string BuildBbobJS(string dist, BuildData buildData, ThemeProcessor.ThemeInfo themeInfo)
    {
        string mainjsOriginal = Path.Combine(AppContext.BaseDirectory, "JSApi", "bbobMain.js");
        string mainjsDist = Path.Combine(dist, "bbob.js");
        string bbobAssetsPath = Path.Combine(dist, bbobAssets);
        string mjs = File.ReadAllText(mainjsOriginal);
        (dynamic[], string[]) allLinkInfos = getLinkInfos(buildData.LinkInfos, bbobAssetsPath);
        ConfigJson config = ConfigManager.GetConfigManager().MainConfig;
        JSApiType.Blog blog = new JSApiType.Blog()
        {
            categories = BuildListToFolder(buildData.Categories, bbobAssetsPath, "categories"),
            tags = BuildListToFolder(buildData.Tags, bbobAssetsPath, "tags"),
            links = allLinkInfos.Item1,
            archives = generateArchives(buildData.LinkInfos, bbobAssetsPath, "archives"),
            nextFileLinks = allLinkInfos.Item2
        };
        JSApiType.BbobMeta meta = new JSApiType.BbobMeta(config);
        meta.copyright = meta.copyright.Replace("year", DateTime.Now.Year.ToString()).Replace("author", config.author).Replace("themeName", themeInfo.name);
        LoadThirdMetas(meta, buildData.Metas);
        JsonSerializerOptions a = new JsonSerializerOptions();
        string blogPlain = $"\nconst blog = {JsonSerializer.Serialize(blog, blog.GetType())}";
        string metaPlain = $"\nconst meta =  {JsonSerializer.Serialize(meta, meta.GetType())}";
        string globalVariable = "\nvar Bbob = { blog, meta, api }";
        string content = mjs + blogPlain + metaPlain + globalVariable;
        SHA256 sha256 = SHA256.Create();
        string hash = "";
        using (FileStream fs = new FileStream(mainjsDist, FileMode.Create, FileAccess.ReadWrite))
        {
            fs.Write(Encoding.UTF8.GetBytes(content));
            fs.Flush(); //If not flush now, hash can't compute
            fs.Position = 0; //set to 0 to read.
            hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
        }
        string newName = $"bbob.{hash.Substring(0, 9)}.js";
        string newPath = Path.Combine(dist, newName);
        File.Move(mainjsDist, newPath);
        ProcessPublicPath(dist, themeInfo.index);
        return newName;
    }

    private static void ProcessPublicPath(string dist, string indexHtml)
    {
        var config = ConfigManager.GetConfigManager().MainConfig;
        string distIndex = Path.Combine(dist, indexHtml);
        string index = File.ReadAllText(distIndex);
        string pattern = "=\"/([^/].*)\"";
        string replacement = $"=\"{config.baseUrl}$1\"";
        index = Regex.Replace(index, pattern, replacement);
        File.WriteAllText(distIndex, index);
    }

    private static FilterSource[] BuildListToFolder(List<KeyValuePair<string, List<dynamic>>> dict, string dist, string name)
    {
        List<FilterSource> filterSources = new List<FilterSource>();
        var config = ConfigManager.GetConfigManager().MainConfig;
        string localPath = Path.Combine(dist, name);
        Directory.CreateDirectory(localPath);
        SHA256 sha256 = SHA256.Create();
        foreach (var item in dict)
        {
            string vFile = Path.Combine(localPath, $"{item.Key}.json");
            string hash = "";
            using (FileStream fs = new FileStream(vFile, FileMode.Create, FileAccess.ReadWrite))
            {
                JsonSerializer.Serialize(fs, item.Value);
                fs.Flush(); //If not flush now, hash can't compute
                fs.Position = 0; //set to 0 to read.
                hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
            }
            string newName = $"{item.Key}.{hash.Substring(0,9)}.json";
            string newPath = Path.Combine(localPath, newName);
            string webPath = Path.Combine($"{config.baseUrl}{bbobAssets}/{name}/{newName}");
            File.Move(vFile, newPath);
            filterSources.Add(new FilterSource(item.Key, webPath));
        }
        return filterSources.ToArray();
    }

    private static void LoadThirdMetas(JSApiType.BbobMeta bbobMeta, Dictionary<string, object> pluginsMeta)
    {
        Directory.CreateDirectory(metasFolder);
        string[] metas = Directory.GetFiles(metasFolder);
        Dictionary<string, object> metasJson = new Dictionary<string, object>();
        foreach (var meta in metas)
        {
            string ext = ".meta.json";
            if (Shared.SharedLib.PathHelper.FileNameEndWith(meta, ext))
            {
                string name = Path.GetFileName(meta).Replace(ext, string.Empty);
                object? third = JsonSerializer.Deserialize<IDictionary<string, object>>(File.ReadAllText(meta));
                if (third == null) continue;
                metasJson.Add(name, third);
            }
        }
        foreach (var item in pluginsMeta)
        {
            if (!metasJson.ContainsKey(item.Key)) metasJson.Add(item.Key, item.Value);
            else
            {
                IDictionary<string, object> metaDict = getPropertiesToDictionary(metasJson[item.Key]);
                IDictionary<string, object> pluginMetaDict = getPropertiesToDictionary(item.Value);
                foreach (var pm in pluginMetaDict)
                {
                    if (!metaDict.ContainsKey(pm.Key)) metaDict.Add(pm.Key, pm.Value);
                }
                metasJson[item.Key] = metaDict;
            }
        }
        var extra = (IDictionary<string, object>)bbobMeta.extra;
        foreach (var item in metasJson)
        {
            if (!extra.ContainsKey(item.Key))
            {
                extra.Add(item.Key, item.Value);
            }
            else
            {
                System.Console.WriteLine($"extra meta already contain target meta '{item.Key}'");
            }
        }
    }

    private static IDictionary<string, object> getPropertiesToDictionary(object obj)
    {
        if (obj is IDictionary<string, object>) return (IDictionary<string, object>)obj;
        var properties = obj.GetType().GetProperties();
        Dictionary<string, object> objDict = new ();
        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            System.Console.WriteLine(property.Name);
            if (value != null) objDict.Add(property.Name, value);
        }
        return objDict;
    }

    private static FilterSource[] generateArchives(List<dynamic> LinkInfos, string dist, string name)
    {
        Dictionary<string, List<dynamic>> a = new();
        foreach (dynamic link in LinkInfos)
        {
            DateTime dateTime = DateTime.Parse(link.date);
            string year = dateTime.Year.ToString();
            if (!a.ContainsKey(year)) a.Add(year, new List<dynamic>());
            a[year].Add(link);
        }
        return BuildListToFolder(a.ToList(), dist, name);
    }

    private static (dynamic[], string[]) getLinkInfos(List<dynamic> LinkInfos, string dist)
    {
        ConfigJson config = ConfigManager.GetConfigManager().MainConfig;
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
            string newName = $"next.{hash.Substring(0, 9)}.js";
            string newPath = Path.Combine(nextLinkInfoFilesFolderLocal, newName);
            File.Move(nextLinkInfosFile, newPath);
            nextFileLinkInfos.Add($"{config.baseUrl}{bbobAssets}/{nextLinkInfoFilesFolder}/{newName}");
        }

        return (current.ToArray(), nextFileLinkInfos.ToArray());
    }
}