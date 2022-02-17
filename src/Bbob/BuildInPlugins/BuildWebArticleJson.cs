using System.Security.Cryptography;
using System.Text.Json;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;
public class BuildWebArticleJson: IPlugin
{
    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        if (stage != GenerationStage.FinalProcess) return;

        PluginHelper.getRegisteredObject<LinkInfo>("linkInfo", out var linkInfo);
        PluginHelper.getRegisteredObject<string>("contentParsed", out var contentParsed);
        PluginHelper.getRegisteredObject<string>("toc", out var toc);
        if (linkInfo == null || contentParsed == null) return;
        
        DateTime dateTime = linkInfo.date != null? DateTime.Parse(linkInfo.date) : DateTime.Now;
        string year = dateTime.Year.ToString();
        string month = dateTime.Month.ToString();
        string day = dateTime.Day.ToString();
        string targetFile = $"{Path.GetFileNameWithoutExtension(filePath)}.json";
        string folder = "articles";
        string webPath = Path.Combine(folder, year,month,day, targetFile);
        string FileLocalFolder = Path.Combine(distribution, folder, year, month, day);
        string FileLocal = Path.Combine(FileLocalFolder, targetFile);
        Directory.CreateDirectory(FileLocalFolder);
        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        SHA256 sha256 = SHA256.Create();
        string hash = "";
        using (FileStream fs = new FileStream(FileLocal, FileMode.Create, FileAccess.ReadWrite))
        {
            JsonSerializer.Serialize(fs, new {
                title = linkInfo.title,
                categories = linkInfo.categories,
                tags = linkInfo.tags,
                date = linkInfo.date,
                contentParsed = contentParsed,
                toc = toc
            }, options);
            fs.Flush(); //If not flush now, hash can't compute
            fs.Position = 0; //set to 0 to read.
            hash = Shared.SharedLib.BytesToString(sha256.ComputeHash(fs));
            System.Console.WriteLine($"My hash: {hash}");
        }
        string newName = $"{Path.GetFileNameWithoutExtension(filePath)}.{hash.Substring(0,9)}.json";
        string newLocal = Path.Combine(FileLocalFolder, newName);
        File.Move(FileLocal, newLocal);
        PluginHelper.getRegisteredObject<string>("config.publicPath", out var publicPath);
        if (publicPath == null)
        {
            throw new NullReferenceException("publicPath is null");
        }
        linkInfo.address = $"{publicPath}{folder}/{year}/{month}/{day}/{newName}";
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