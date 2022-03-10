using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginCondition("BuildWebArticleJson", PluginOrder = PluginOrder.BeforeMe)]
[PluginCondition("ExtraLinks", PluginOrder = PluginOrder.AfterMe)]
public class SitemapGenerator : IPlugin
{
    List<KeyValuePair<string, string>> articlesUrl = new();
    string fullUrl = Shared.SharedLib.UrlHelper.UrlCombine(PluginHelper.ConfigBbob.domain, PluginHelper.ConfigBbob.baseUrl);

    MyConfig config;
    string? articleBaseUrl;
    public SitemapGenerator()
    {
        PluginHelper.getPluginJsonConfig<MyConfig>(out MyConfig? tar);
        config = tar ?? new MyConfig();
        var theme = PluginHelper.getThemeInfo<Theme>();
        if (theme != null)
        {
            articleBaseUrl = theme.articleBaseUrl;
        }
        if (articleBaseUrl == null)
        {
            PluginHelper.printConsole("theme.articleBaseUrl is null, target theme is not support.");
        }
    }
    class Theme
    {
        public string? articleBaseUrl { get; set; }
    }
    class MyConfig
    {
        public bool redirectUrl { get; set; } = true;
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
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        switch (stage)
        {
            case GenerationStage.Confirm:
                if (PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? value))
                {
                    if (value == null) return;
                    if (articleBaseUrl != null)
                    {
                        string address = value.address;
                        string remake = articleBaseUrl.Replace("&", "~and~").Replace('?', '&');
                        string redirectUrl = $"{fullUrl}?{remake}{address}";
                        string normalUrl = $"{fullUrl}{articleBaseUrl}{address}";
                        articlesUrl.Add(new KeyValuePair<string, string>(value.title, config.redirectUrl ? redirectUrl : normalUrl));
                    }
                }
                else PluginHelper.ExecutingCommandResult = new CommandResult("Can't get article object.", CommandOperation.RunMeAgain);
                break;

            default:
                break;
        }
    }
    public void CommandComplete(Commands commands)
    {
        if (commands == Commands.GenerateCommand && articlesUrl.Count > 0)
        {
            string distribution = PluginHelper.DistributionDirectory;
            if (config.redirectUrl) InsertFuncToIndex(distribution);
            articlesUrl.Add(new KeyValuePair<string, string>("Home", fullUrl));
            generateSitemap(distribution);
            generateRobotTxt(distribution);
            PluginHelper.printConsole("Success generate sitemap.");
        }
    }

    private void InsertFuncToIndex(string distribution)
    {
        string indexFile = Path.Combine(distribution, "index.html");
        string script = "<script>!function(n){var a;'/'===n.search[1]&&(a=n.search.slice(1).split('&').map(function(n){return n.replace(/~and~/g,'&')}).join('?'),window.history.replaceState(null,null,n.pathname.slice(0,-1)+a+n.hash))}(window.location);</script>";
        string indexPlain = File.ReadAllText(indexFile);
        string patternHead = @"<head>(.*)</head>";
        string replacement1 = $"<head>{script}$1</head>";
        indexPlain = Regex.Replace(indexPlain, patternHead, replacement1, RegexOptions.Singleline);
        File.WriteAllText(indexFile, indexPlain);
    }

    private void generateRobotTxt(string distribution)
    {
        string robot = Path.Combine(distribution, "robots.txt");
        string sitemapPath = $"Sitemap: {fullUrl}sitemap.xml";
        File.WriteAllText(robot, sitemapPath);
    }

    private void generateSitemap(string distribution)
    {
        string sitemapFile = Path.Combine(distribution, "sitemap.xml");
        SitemapXml sitemapXml = new SitemapXml() { Format = true };
        articlesUrl.Add(new KeyValuePair<string, string>("It is sitemap html.", $"{fullUrl}sitemap-html.html"));
        var aurl = articlesUrl.Select(i => i.Value);
        sitemapXml.AddRange(aurl);
        sitemapXml.WriteToFile(sitemapFile);
        generateSitemapHtml(distribution);
    }

    private void generateSitemapHtml(string distribution)
    {
        string sitemap = Path.Combine(distribution, "sitemap-html.html");
        string metas = "<meta charset=\"UTF-8\"/>" +
                        "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />" +
                        "<meta name=\"robots\" content=\"noindex\">";
        string style = "<style>li{margin: 15px auto}</style>";
        UnsortedList unsortedList = new UnsortedList();
        foreach (var article in articlesUrl)
        {
            unsortedList.Add($"<a href=\"{article.Value}\">{article.Key}</a>");
        }
        string html = $"<!DOCTYPE html><html lang=\"en\"><head>{metas}<title>Sitemap Html</title>{style}</head><body>{unsortedList}</body></html>";
        File.WriteAllText(sitemap, html);
        PluginHelper.getRegisteredObjectNoNull<Dictionary<string, string>>("extraLinks").Add("Sitemap", "/sitemap-html.html");
    }

    class SitemapXml
    {
        List<string> address = new List<string>();
        public bool Format { get; set; } = false;
        public SitemapXml()
        {
        }
        public void Add(string url) => address.Add(url);
        public void AddRange(IEnumerable<string> range) => address.AddRange(range);
        public override string ToString()
        {
            string real = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            if (Format) real += "\n";
            real += "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">";

            foreach (string a in address)
            {
                string add = replaceIt(a);
                real += Format ? $"\n  <url>\n    <loc>{add}</loc>\n  </url>" : $"<url><loc>{add}</loc></url>";
            }
            return real + "\n</urlset> ";
        }
        public void WriteToFile(string file) => File.WriteAllText(file, this.ToString());
        public string replaceIt(string t)
        {
            return t.Replace(" ", "&nbsp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
    }

    class UnsortedList
    {
        string html;
        public bool Format { get; set; } = false;
        List<string> items = new List<string>();
        public UnsortedList()
        {
            html = "<ul>";
            if (Format) html += '\n';
        }
        public void Add(string item) => items.Add(item);
        public void AddRange(IEnumerable<string> range) => items.AddRange(range);
        public override string ToString()
        {
            string real = html;
            foreach (string item in items)
            {
                real += $"<li>{item}</li>";
                if (Format) html += '\n';
            }
            return real + "</ul>";
        }
    }
}