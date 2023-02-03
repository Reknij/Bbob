using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "BuildWebArticleJson", version = "1.0.0", author = "Jinker")]
[PluginCondition("LinkProcess", PluginOrder.BeforeMe)]
public class BuildWebArticleJson : IPlugin
{
    public BuildWebArticleJson()
    {
    }
    public void InitCommand()
    {
    }

    Dictionary<string, int> numbers = new Dictionary<string, int>();
    Dictionary<string, ArticleData> articlesToCreate = new Dictionary<string, ArticleData>();
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage != GenerationStage.Confirm) return;

        PluginHelper.getRegisteredObject<dynamic>("article", out var article);
        if (article == null) return;
        string targetFile = $"{Path.GetFileNameWithoutExtension(filePath)}.json";
        string folder = "articles";
        string FileLocalFolder = Path.Combine(PluginHelper.DistributionDirectory, JSApi.JSAPiHelper.bbobAssets, folder);
        string FileLocal = Path.Combine(FileLocalFolder, targetFile);

        if (File.Exists(FileLocal))
        {
            string name = Path.GetFileNameWithoutExtension(FileLocal);
            if (!numbers.ContainsKey(name)) numbers.Add(name, 2);
            int number = numbers[name];
            PluginHelper.printConsole($"Already exists article json {name}, will change to other name", ConsoleColor.Yellow);
            while (File.Exists(FileLocal))
            {
                string otherName = $"{name}-{number++}";
                FileLocal = Path.Combine(FileLocalFolder, $"{otherName}.json");
            }
            numbers[name] = number;
        }

        Directory.CreateDirectory(FileLocalFolder);
        string hash = "";
        hash = Shared.SharedLib.HashHelper.GetContentHash(JsonSerializer.SerializeToUtf8Bytes(article));

        if (PluginHelper.ConfigBbob.useHashName)
        {
            FileLocal = Path.Combine(FileLocalFolder, $"{Path.GetFileNameWithoutExtension(FileLocal)}-{hash.Substring(0, 9)}.json");
        }

        string baseUrl = PluginHelper.ConfigBbob.baseUrl;
        article.id = Path.GetFileNameWithoutExtension(FileLocal);
        article.contentHash = hash;
        articlesToCreate.Add(article.id, new ArticleData(article, FileLocal)); //add to list to create file.
    }

    public Action? CommandComplete(Commands cmd)
    {
        if (cmd != Commands.GenerateCommand) return null;

        processRelative(); //set next and previous article data of any article.
        createArticles(); //finally create article json files.

        return null;
    }

    private void processRelative()
    {
        dynamic links = PluginHelper.getRegisteredObjectNotNull<dynamic>("pureLinks");
        for (int i = 0; i < links.Count; i++)
        {
            var current = articlesToCreate[links[i].id];
            if (i > 0) current.data.nextArticle = links[i - 1];
            if (i < articlesToCreate.Count - 1) current.data.previousArticle = links[i + 1];
        }
    }

    private void createArticles()
    {
        foreach (var ad in articlesToCreate)
        {
            using (FileStream fs = new FileStream(ad.Value.path, FileMode.Create, FileAccess.Write))
            {
                JsonSerializer.Serialize(fs, ad.Value.data, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
        }
    }

    private struct ArticleData
    {
        public dynamic data { get; set; }
        public string path { get; set; }

        public ArticleData(dynamic data, string path)
        {
            this.data = data;
            this.path = path;
        }
    }
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