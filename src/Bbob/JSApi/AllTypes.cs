using System.Text.Json.Serialization;
using static Bbob.Main.Cli.Generator;
using Bbob.Plugin;

namespace Bbob.Main.JSApi;

public class JSApiType
{
    public class Blog
    {
        public string[]? categories { get; set; }
        public string[]? tags { get; set; }
        public dynamic[]? links { get; set; }
        public string[]? nextFileLinks { get; set; }
    }

    public class BbobMeta
    {
        public string? blogName { get; set; }
        public string? author { get; set; }
        public string? description { get; set; }
        public string? about { get; set; }
        public string copyright { get; set; }
        public string publicPath{get;set;}
        public object? extra { get; set; }
        public BbobMeta(Configuration.ConfigManager.ConfigJson config)
        {
            this.blogName = config.blogName;
            this.author = config.author;
            this.description = config.description;
            this.about = config.about;
            this.copyright = $"© year author Powered by Bbob & themeName";
            this.publicPath = config.publicPath;
            extra = new Object();
        }
    }

    public class BuildData
    {
        public List<dynamic> LinkInfos { get; set; }
        public List<KeyValuePair<string, List<dynamic>>> Categories { get; set; }
        public List<KeyValuePair<string, List<dynamic>>> Tags { get; set; }

        public BuildData(List<dynamic> LinkInfos,
                        List<KeyValuePair<string, List<dynamic>>> categories,
                        List<KeyValuePair<string, List<dynamic>>> tags)
        {
            this.LinkInfos = LinkInfos;
            Categories = categories;
            Tags = tags;
        }
    }
}