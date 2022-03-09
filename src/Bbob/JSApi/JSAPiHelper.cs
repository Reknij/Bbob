using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Main.Configuration;
using static Bbob.Main.JSApi.JSApiType;
using Bbob.Plugin;
using System.Security.Cryptography;
using System.Dynamic;
using NUglify;

namespace Bbob.Main.JSApi;

public static class JSAPiHelper
{
    public static readonly string metasFolder = Path.Combine(Environment.CurrentDirectory, "metas");
    public static readonly string bbobAssets = "bbob-assets";

    public static void Hook(string dist, string indexName, string hookFile)
    => Hook(dist, indexName, new string[] { hookFile });
    public static void Hook(string dist, string indexName, string[] hookFiles)
    {
        if (hookFiles.Length > 0)
        {
            var config = ConfigManager.MainConfig;
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
            indexHtml = changeIndexTitle(indexHtml, ConfigManager.MainConfig.blogName);
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
        ConfigJson config = ConfigManager.MainConfig;
        JSApiType.BbobMeta meta = new JSApiType.BbobMeta(config);
        meta.copyright = meta.copyright.Replace("year", DateTime.Now.Year.ToString()).Replace("author", config.author).Replace("themeName", themeInfo.name);
        LoadThirdMetas(meta, buildData.Metas);
        JsonSerializerOptions a = new JsonSerializerOptions();
        string blogPlain = $"\nconst blog = {JsonSerializer.Serialize(buildData.blog)}";
        string metaPlain = $"\nconst meta =  {JsonSerializer.Serialize(meta)}";
        string globalVariable = "\nvar Bbob = { blog, meta, api }";
        string content = mjs + blogPlain + metaPlain + globalVariable;
        try
        {
            content = Uglify.Js(content).Code;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine("Compress bbob.js error:\n" + ex.Message);
        }
        string hash = "";
        using (FileStream fs = new FileStream(mainjsDist, FileMode.Create, FileAccess.ReadWrite))
        {
            fs.Write(Encoding.UTF8.GetBytes(content));
            fs.Flush(); //If not flush now, hash can't compute
            fs.Position = 0; //set to 0 to read.
            hash = Shared.SharedLib.HashHelper.GetContentHash(fs);
        }
        string newName = config.useHashName?$"bbob-{hash.Substring(0, 9)}.js": "bbob.js";
        string newPath = Path.Combine(dist, newName);
        if (mainjsDist != newPath) File.Move(mainjsDist, newPath);
        ProcessPublicPath(dist, themeInfo.index);
        return newName;
    }

    private static void ProcessPublicPath(string dist, string indexHtml)
    {
        var config = ConfigManager.MainConfig;
        string distIndex = Path.Combine(dist, indexHtml);
        string index = File.ReadAllText(distIndex);
        string pattern = "=\"/([^/].*)\"";
        string replacement = $"=\"{config.baseUrl}$1\"";
        index = Regex.Replace(index, pattern, replacement);
        File.WriteAllText(distIndex, index);
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
        Dictionary<string, object> objDict = new();
        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            System.Console.WriteLine(property.Name);
            if (value != null) objDict.Add(property.Name, value);
        }
        return objDict;
    }


}