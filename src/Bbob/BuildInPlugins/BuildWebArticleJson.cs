using System.Dynamic;
using System.Security.Cryptography;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;
public class BuildWebArticleJson : IPlugin
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
    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        if (stage != GenerationStage.Confirm) return;

        PluginHelper.getRegisteredObject<dynamic>("article", out var article);
        if (article == null) return;
        // DateTime dateTime = article.date != null ? DateTime.Parse(article.date) : DateTime.Now;
        // string year = dateTime.Year.ToString();
        // string month = dateTime.Month.ToString();
        // string day = dateTime.Day.ToString();
        string targetFile = $"{Path.GetFileNameWithoutExtension(filePath)}.json";
        string folder = "articles";
        //string FileLocalFolder = Path.Combine(distribution, JSApi.JSAPiHelper.bbobAssets, folder, year, month, day);
        string FileLocalFolder = Path.Combine(distribution, JSApi.JSAPiHelper.bbobAssets, folder);
        string FileLocal = Path.Combine(FileLocalFolder, targetFile);
        Directory.CreateDirectory(FileLocalFolder);
        SHA256 sha256 = SHA256.Create();
        string hash = "";
        using (FileStream fs = new FileStream(FileLocal, FileMode.Create, FileAccess.ReadWrite))
        {
            JsonSerializer.Serialize(fs, article, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            fs.Flush(); //If not flush now, hash can't compute
            fs.Position = 0; //set to 0 to read.
            hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
        }
        string newName = $"{Path.GetFileNameWithoutExtension(filePath)}-{hash.Substring(0, 9)}.json";
        string newLocal = Path.Combine(FileLocalFolder, newName);
        File.Move(FileLocal, newLocal, true);
        string baseUrl = PluginHelper.ConfigBbob.baseUrl;
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? configPlugin);
        bool isShortAddress = configPlugin != null ? configPlugin.shortAddress : false;
        article.address = isShortAddress ? Path.GetFileNameWithoutExtension(newName) : $"{baseUrl}{JSApi.JSAPiHelper.bbobAssets}/{folder}/{newName}";
        if (isShortAddress)
        {
            Meta meta = new Meta($"{baseUrl}{JSApi.JSAPiHelper.bbobAssets}/{folder}/", ".json");
            PluginHelper.registerMeta("shortAddress", meta);
        }
    }

    public class MyConfig
    {
        public bool shortAddress {get;set;} = false;
    }
    public record class Meta(string startOfAddress, string endOfAddress);

    private class MarkdownFrontMatter
    {
        public string title { get; set; }
        public string date { get; set; }
        public string[]? categories { get; set; }
        public string[]? tags { get; set; }
        public MarkdownFrontMatter()
        {
            title = date = string.Empty;
        }
        public MarkdownFrontMatter(string title, string date, string[] categories, string[] tags)
        {
            this.title = title;
            this.date = date;
            this.categories = categories;
            this.tags = tags;
        }

        public MarkdownFrontMatter(MarkdownFrontMatter mfm)
        {
            this.title = mfm.title;
            this.date = mfm.date;
            this.categories = mfm.categories;
            this.tags = mfm.tags;
        }
        public bool isValid()
        {
            List<string> all = new List<string>();
            if (categories != null) all.AddRange(categories);
            if (tags != null) all.AddRange(tags);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                foreach (var name in all)
                {
                    if (name.Contains(c)) return false;
                }
            }
            return true;
        }
    }
}