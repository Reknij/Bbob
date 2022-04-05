using System.Text.Json;
using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "BuildWebArticleJson", version = "1.0.0", author = "Jinker")]
public class BuildWebArticleJson : IPlugin
{
    public BuildWebArticleJson()
    {
        PluginHelper.registerCustomCommand("config", (args) =>
        {
            if (args.Length == 2)
            {
                PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
                MyConfig myConfig = tar ?? new MyConfig();
                switch (args[0])
                {
                    case "shortAddress":
                        var value = args[1];
                        if (!bool.TryParse(value, out bool result))
                        {
                            PluginHelper.printConsole("shortAddress must is boolean value!");
                            return;
                        }
                        myConfig.shortAddress = result;
                        break;

                    default:
                        PluginHelper.printConsole($"Unknown config name 'args[0]'!");
                        return;
                }
                PluginHelper.printConsole("Config save success!");
                PluginHelper.savePluginJsonConfig<MyConfig>(myConfig);
            }
            else
            {
                PluginHelper.printConsole("Please enter config name and value!");
            }
        });
    }
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
    Dictionary<string, int> numbers = new Dictionary<string, int>();
    public void GenerateCommand(string filePath, GenerationStage stage)
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
        string FileLocalFolder = Path.Combine(PluginHelper.DistributionDirectory, JSApi.JSAPiHelper.bbobAssets, folder);
        string FileLocal = Path.Combine(FileLocalFolder, targetFile);

        if (File.Exists(FileLocal))
        {
            string name = Path.GetFileNameWithoutExtension(FileLocal);
            int number = numbers.ContainsKey(name) ? numbers[name] : 2;
            PluginHelper.printConsole($"Already exists article json {name}, will change to other name");
            while (File.Exists(FileLocal))
            {
                string otherName = $"{name}-{number++}";
                FileLocal = Path.Combine(FileLocalFolder, $"{otherName}.json");
            }
            numbers[name] = number;
        }

        Directory.CreateDirectory(FileLocalFolder);
        string hash = "";
        using (FileStream fs = new FileStream(FileLocal, FileMode.Create, FileAccess.ReadWrite))
        {
            JsonSerializer.Serialize(fs, article, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            fs.Flush(); //If not flush now, hash can't compute
            fs.Position = 0; //set to 0 to read.
            hash = Shared.SharedLib.HashHelper.GetContentHash(fs);
        }
        string newName = PluginHelper.ConfigBbob.useHashName ? $"{Path.GetFileNameWithoutExtension(filePath)}-{hash.Substring(0, 9)}.json" : $"{Path.GetFileNameWithoutExtension(filePath)}.json";
        string newLocal = Path.Combine(FileLocalFolder, newName);
        if (FileLocal != newLocal) File.Move(FileLocal, newLocal, true);
        string baseUrl = PluginHelper.ConfigBbob.baseUrl;
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? configPlugin);
        bool isShortAddress = configPlugin != null ? configPlugin.shortAddress : false;
        bool shortAddressEndWithSlash = configPlugin != null ? configPlugin.shortAddressEndWithSlash : false;
        article.address = isShortAddress ? Path.GetFileNameWithoutExtension(newName) : $"{baseUrl}{JSApi.JSAPiHelper.bbobAssets}/{folder}/{newName}";
        if (shortAddressEndWithSlash) article.address += '/';
        article.contentHash = hash;
        if (isShortAddress)
        {
            Meta meta = new Meta($"{baseUrl}{JSApi.JSAPiHelper.bbobAssets}/{folder}/", ".json");
            PluginHelper.registerMeta("shortAddress", meta);
        }
    }

    public Action? CommandComplete(Commands cmd)
    {
        if (cmd != Commands.GenerateCommand) return null;

        return () =>
        {
            PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
            MyConfig config = tar ?? new MyConfig();
            string[] files = Directory.GetFiles(PluginHelper.DistributionDirectory, "bbob*.js");
            string bbobJs = string.Empty;
            foreach (var item in files)
            {
                if (bbobJs == string.Empty && Regex.IsMatch(item, "bbob(.{10})?.js")) bbobJs = item;
                else
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("Can't insert function, found bbob js file but is not single.", CommandOperation.Stop);
                    return;
                }
            }
            if (bbobJs == string.Empty)
            {
                PluginHelper.ExecutingCommandResult = new CommandResult("Can't insert function, bbob js file is not exists!", CommandOperation.Stop);
                return;
            }
            string bbobJsString = File.ReadAllText(bbobJs);
            string code = "$1=meta.extra.shortAddress.startOfAddress+$1+meta.extra.shortAddress.endOfAddress;";
            if (config.shortAddressEndWithSlash) code = "let l=$1.length;if(l>1&&$1[l-1]=='/'){$1=$1.slice(0, -1);}" + code;
            bbobJsString = Regex.Replace(bbobJsString, @"getArticleFromAddress\(([A-Za-z0-9_]+),\s*([A-Za-z0-9_]+)\)\s*{", "getArticleFromAddress($1,$2){" + code, RegexOptions.Singleline);
            bbobJsString = Regex.Replace(bbobJsString, @"getArticleFromAddressAsync\(([A-Za-z0-9_]+)\)\s*{", "getArticleFromAddressAsync($1){" + code, RegexOptions.Singleline);
            File.WriteAllText(bbobJs, bbobJsString);
        };
    }

    public class MyConfig
    {
        public bool shortAddress { get; set; } = false;
        public bool shortAddressEndWithSlash { get; set; } = false;
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