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
        if (commands == Commands.GenerateCommand && distribution != null)
        {
            string baseUrl = PluginHelper.getRegisteredObjectNoNull<string>("config.baseUrl");
            string indexFile = Path.Combine(distribution, "index.html");
            string script = "<script>!function(n){var a;'/'===n.search[1]&&(a=n.search.slice(1).split('&').map(function(n){return n.replace(/~and~/g,'&')}).join('?'),window.history.replaceState(null,null,n.pathname.slice(0,-1)+a+n.hash))}(window.location);</script>";
            string indexPlain = File.ReadAllText(indexFile);
            string pattern = @"<head>(.*)</head>";
            string replacement = $"<head>{script}$1</head>";
            indexPlain = Regex.Replace(indexPlain, pattern, replacement, RegexOptions.Singleline);
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
            string sitemapPath = $"Sitemap: {config.domain}sitemap.txt";
            File.WriteAllText(robot, sitemapPath);
        }
    }

    private void generateSitemap(string distribution)
    {
        string sitemap = Path.Combine(distribution, "sitemap.txt");
        List<string> aurl = new List<string>();
        if (File.Exists(sitemap))
        {
            aurl.AddRange(File.ReadAllLines(sitemap));
        }
        aurl.AddRange(articlesUrl);
        File.WriteAllLines(sitemap, aurl);
    }

    public record class Config
    {
        public string? articleBaseUrl { get; set; }
        public string? domain { get; set; }
    }
}