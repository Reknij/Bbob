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
        public LinkInfo[]? links { get; set; }
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
        public List<LinkInfo> LinkInfos { get; set; }
        public List<KeyValuePair<string, List<LinkInfo>>> Categories { get; set; }
        public List<KeyValuePair<string, List<LinkInfo>>> Tags { get; set; }

        public BuildData(List<LinkInfo> LinkInfos,
                        List<KeyValuePair<string, List<LinkInfo>>> categories,
                        List<KeyValuePair<string, List<LinkInfo>>> tags)
        {
            this.LinkInfos = LinkInfos;
            Categories = categories;
            Tags = tags;
        }
    }
}