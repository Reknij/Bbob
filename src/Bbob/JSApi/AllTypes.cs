using System.Text.Json.Serialization;
using static Bbob.Main.Cli.Generator;
using Bbob.Plugin;
using System.Dynamic;

namespace Bbob.Main.JSApi;

public class JSApiType
{
    public class BbobMeta
    {
        public string blogName { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string about { get; set; }
        public string copyright { get; set; }
        public int blogCountOneTime { get; set; }
        public string allLink { get; set; }
        public string baseUrl{get;set;}
        public string lastBuild {get;set;}
        public dynamic extra { get; set; }
        public BbobMeta(ConfigJson config)
        {
            this.blogName = config.blogName;
            this.author = config.author;
            this.description = config.description;
            this.about = config.about;
            this.copyright = $"© year author Powered by Bbob & themeName";
            this.blogCountOneTime = config.blogCountOneTime;
            this.allLink = config.allLink;
            this.baseUrl = config.baseUrl;
            this.lastBuild = Shared.SharedLib.DateTimeHelper.GetDateTimeNowString();
            extra = new ExpandoObject();
        }
    }

    public class BuildData
    {
        public dynamic blog {get;set;}
        public Dictionary<string, dynamic> Metas {get;set;}

        public BuildData(dynamic blog, Dictionary<string, dynamic> metas)
        {
            this.blog = blog;
            this.Metas = metas;
        }
    }
}