using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Main.Configuration;
using static Bbob.Main.JSApi.JSApiType;
using Bbob.Plugin;
using System.Security.Cryptography;

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
        string mainjsOriginal = Path.Combine(Environment.CurrentDirectory, "JSApi", "bbobMain.js");
        string mainjsDist = Path.Combine(dist, "bbob.js");
        string bbobAssetsPath = Path.Combine(dist, bbobAssets);
        string mjs = File.ReadAllText(mainjsOriginal);
        (dynamic[], string[]) allLinkInfos = getLinkInfos(buildData.LinkInfos, bbobAssetsPath);
        List<ArchiveYear> archives = generateArchives(buildData.LinkInfos, bbobAssetsPath);
        ConfigManager.ConfigJson config = ConfigManager.GetConfigManager().MainConfig;
        BuildListToFolder(buildData.Categories, bbobAssetsPath, "categories");
        BuildListToFolder(buildData.Tags, bbobAssetsPath, "tags");
        JSApiType.Blog blog = new JSApiType.Blog()
        {
            categories = listItemToArray(buildData.Categories),
            tags = listItemToArray(buildData.Tags),
            links = allLinkInfos.Item1,
            archives = archives.ToArray(),
            nextFileLinks = allLinkInfos.Item2
        };
        JSApiType.BbobMeta meta = new JSApiType.BbobMeta(config);
        meta.copyright = meta.copyright.Replace("year", DateTime.Now.Year.ToString()).Replace("author", config.author).Replace("themeName", themeInfo.name);
        LoadThirdMetas(meta);
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

    private static void BuildListToFolder(List<KeyValuePair<string, List<dynamic>>> dict, string dist, string v)
    {
        string localPath = Path.Combine(dist, v);
        if (dict.Count == 0) return;
        Directory.CreateDirectory(localPath);

        foreach (var item in dict)
        {
            string vFile = Path.Combine(localPath, $"{item.Key}.json");
            using (FileStream fs = File.OpenWrite(vFile))
            {
                JsonSerializer.Serialize(fs, item.Value);
            }
        }
    }

    private static void LoadThirdMetas(JSApiType.BbobMeta bbobMeta)
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
                object? third = JsonSerializer.Deserialize(File.ReadAllText(meta), typeof(Object));
                if (third == null) continue;
                metasJson.Add(name, third);
            }

        }
        string extraPlain = "{";
        foreach (var item in metasJson)
        {
            extraPlain += $"\"{item.Key}\":{JsonSerializer.Serialize(item.Value)},";
        }
        if (metasJson.Count > 0)
        {
            bbobMeta.extra = JsonSerializer.Deserialize(extraPlain.Remove(extraPlain.Length - 1) + '}', typeof(object));
        }
    }

    private static List<ArchiveYear> generateArchives(List<dynamic> LinkInfos, string dist)
    {
        var config = ConfigManager.GetConfigManager().MainConfig;
        List<ArchiveYear> a = new List<ArchiveYear>();
        Dictionary<int, Dictionary<int, List<dynamic>>> archives = new();
        foreach (dynamic link in LinkInfos)
        {
            DateTime dateTime = DateTime.Parse(link.date);
            if (!archives.ContainsKey(dateTime.Year)) archives.Add(dateTime.Year, new Dictionary<int, List<dynamic>>());
            if (!archives[dateTime.Year].ContainsKey(dateTime.Month)) archives[dateTime.Year].Add(dateTime.Month, new List<dynamic>());
            archives[dateTime.Year][dateTime.Month].Add(link);
        }
        string folder = Path.Combine(dist, "archives");
        Directory.CreateDirectory(folder);
        string tempFile = Path.Combine(folder, "archive.temp.json");
        SHA256 sha256 = SHA256.Create();
        foreach (var item in archives)
        {
            string hash = "";
            List<ArchiveMonth> months = new ();
            foreach (var m in item.Value)
            {
                months.Add(new ArchiveMonth(m.Key, m.Value));
            }
            using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
            {
                JsonSerializer.Serialize(fs, months);
                fs.Flush(); //If not flush now, hash can't compute
                fs.Position = 0; //set to 0 to read.
                hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
            }
            string newName = $"archive.{item.Key}.{hash.Substring(0,9)}.json";
            string newPath = Path.Combine(folder, newName);
            File.Move(tempFile, newPath);
            string webUrl = Path.Combine($"{config.baseUrl}archives/{newName}");
            a.Add(new ArchiveYear(item.Key, webUrl));
        }
        return a;
    }

    private static (dynamic[], string[]) getLinkInfos(List<dynamic> LinkInfos, string dist)
    {
        ConfigManager.ConfigJson config = ConfigManager.GetConfigManager().MainConfig;
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
            nextFileLinkInfos.Add($"{config.baseUrl}{nextLinkInfoFilesFolder}/{newName}");
        }

        return (current.ToArray(), nextFileLinkInfos.ToArray());
    }

    private static string[] listItemToArray(List<KeyValuePair<string, List<dynamic>>> categories)
    {
        string[] array = new string[categories.Count];
        for (int i = 0; i < categories.Count; i++)
        {
            array[i] = categories.ElementAt(i).Key;
        }
        return array;
    }
}