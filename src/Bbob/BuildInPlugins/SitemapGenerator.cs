using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class SitemapGenerator : IPlugin
{
    string? distribution { get; set; }
    List<string> articlesUrl = new List<string>();
    private Config? config = null;
    public void GenerateCommand(string filePath, string distribution, GenerationStage stage)
    {
        switch (stage)
        {
            case GenerationStage.Initialize:
                this.distribution = distribution;
                if (PluginHelper.getPluginJsonConfig<Config>(out this.config) && this.config != null)
                {
                    if (this.config.domain != null && this.config.domain.Last() != '/') this.config.domain += '/';
                }
                else
                {
                    PluginHelper.printConsole("No generate sitemap because no config.");
                }
                break;
            case GenerationStage.FinalProcess:
                if (PluginHelper.getRegisteredObject<dynamic>("link", out dynamic? value))
                {
                    if (value == null) return;
                    if (config != null && config.articleBaseUrl != null)
                    {
                        string address = value.address;
                        string baseUrl = PluginHelper.getRegisteredObjectNoNull<string>("config.baseUrl");
                        string remake = config.articleBaseUrl.Replace("&", "~and~").Replace('?', '&');
                        string redirectUrl = $"{config.domain}?{remake}{address}";
                        articlesUrl.Add(redirectUrl);
                    }
                }
                else PluginHelper.ExecutingCommandResult = new CommandResult("Can't get link object.", CommandOperation.RunMeAgain);
                break;

            default:
                break;
        }
    }
    public void CommandComplete(Commands commands)
    {
        if (commands == Commands.GenerateCommand && distribution != null && articlesUrl.Count > 0)
        {
            string baseUrl = PluginHelper.getRegisteredObjectNoNull<string>("config.baseUrl");
            string indexFile = Path.Combine(distribution, "index.html");
            string script = "<script>!function(n){var a;'/'===n.search[1]&&(a=n.search.slice(1).split('&').map(function(n){return n.replace(/~and~/g,'&')}).join('?'),window.history.replaceState(null,null,n.pathname.slice(0,-1)+a+n.hash))}(window.location);</script>";
            string add = "<a href=\"/itissm.html\" hidden>Sitemap html</a>";
            string indexPlain = File.ReadAllText(indexFile);
            string patternHead = @"<head>(.*)</head>";
            string patternBody = @"<body>(.*)</body>";
            string replacement1 = $"<head>{script}$1</head>";
            string replacement2 = $"<body>{add}$1</body>";
            indexPlain = Regex.Replace(indexPlain, patternHead, replacement1, RegexOptions.Singleline);
            indexPlain = Regex.Replace(indexPlain, patternBody, replacement2, RegexOptions.Singleline);
            File.WriteAllText(indexFile, indexPlain);
            generateSitemap(distribution);
            generateRobotTxt(distribution);
        }
    }
    private void generateRobotTxt(string distribution)
    {
        if (config != null)
        {
            string robot = Path.Combine(distribution, "robots.txt");
            string sitemapPath = $"Sitemap: {config.domain}sitemap.xml";
            File.WriteAllText(robot, sitemapPath);
        }
    }

    private void generateSitemap(string distribution)
    {
        string sitemapFile = Path.Combine(distribution, "sitemap.xml");
        List<string> aurl = new List<string>();
        if (File.Exists(sitemapFile))
        {
            aurl.AddRange(File.ReadAllLines(sitemapFile));
        }
        aurl.AddRange(articlesUrl);
        SitemapXml sitemapXml = new SitemapXml() { Format = true };
        sitemapXml.AddRange(aurl);
        sitemapXml.WriteToFile(sitemapFile);
        generateSitemapHtml(distribution, aurl);
    }

    private void generateSitemapHtml(string distribution, List<string> addressList)
    {
        string sitemap = Path.Combine(distribution, "itissm.html");
        string metas = "<meta charset=\"UTF-8\"/>" +
                        "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />" +
                        "<meta name=\"robots\" content=\"noindex\">";
        string style = "<style>li{margin: 15px auto}</style>";
        UnsortedList unsortedList = new UnsortedList();
        foreach (string address in addressList)
        {
            unsortedList.Add($"<a href=\"{address}\">address</a>");
        }
        string html = $"<!DOCTYPE html><html lang=\"en\"><head>{metas}<title>Sitemap Html</title>{style}</head><body>{unsortedList}</body></html>";
        File.WriteAllText(sitemap, html);
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

    public record class Config
    {
        public string? articleBaseUrl { get; set; }
        public string? domain { get; set; }
    }
}