using System.Diagnostics;
using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "GitDeploy", version = "1.0.0", author = "Jinker")]
public class GitDeploy : IPlugin
{
    static readonly string ghDirectoryName = ".deploy_git";
    static readonly string ghDirectory = Path.Combine(PluginHelper.CurrentDirectory, ghDirectoryName);

    public GitDeploy()
    {
        PluginHelper.registerCustomCommand("config", (args) =>
        {
            PluginHelper.getPluginJsonConfig<GitConfig>(out var tar);
            GitConfig config = tar ?? new GitConfig();
            if (args.Length == 2)
            {
                string value = args[1];
                switch (args[0])
                {
                    case "repos":
                        config.repos = value;
                        break;
                    case "branch":
                        config.branch = value;
                        break;
                    case "message":
                        config.message = value;
                        break;
                    case "type":
                        if (value != "github")
                        {
                            PluginHelper.printConsole("type only allow 'github'!", ConsoleColor.Yellow);
                            return;
                        }
                        config.type = value;
                        break;
                    default: break;
                }
                PluginHelper.printConsole("Config save success!", ConsoleColor.Green);
                PluginHelper.savePluginJsonConfig<GitConfig>(config);
            }
            else
            {
                PluginHelper.printConsole("Please enter config name and value!", ConsoleColor.Red);
            }
        });
    }

    public void InitCommand()
    {
        if (!PluginHelper.isPluginJsonConfigExists())
        {
            PluginHelper.savePluginJsonConfig<GitConfig>(new GitConfig());
            PluginHelper.printConsole("Initialize config file.", ConsoleColor.Green);
        }
        else
        {
            PluginHelper.printConsole("Already exists config.", ConsoleColor.Yellow);
        }
    }
    public void DeployCommand()
    {
        string distribution = PluginHelper.DistributionDirectory;
        if (PluginHelper.getPluginJsonConfig<GitConfig>("GitDeploy", out var config) && config != null)
        {
            if (config.repos == null)
            {
                PluginHelper.printConsole("Please enter your repository url in the config.", ConsoleColor.Red);
                return;
            }
            if (config.branch == null) config.branch = "main";
            if (config.message == null) config.message = $"{Shared.SharedLib.DateTimeHelper.GetDateTimeNowString()} Update";
            PluginHelper.printConsole($"Initialize deploy action...", ConsoleColor.Yellow);
            if (!Directory.Exists(ghDirectory))
            {
                cloneReposAndCheckout(config);
            }
            else Shared.SharedLib.DirectoryHelper.DeleteDirectory(ghDirectory);

            Shared.SharedLib.DirectoryHelper.CopyDirectory(distribution, ghDirectory, overwrite: true);
            if (config.type == "github")
            {
                string index = Path.Combine(ghDirectory, "index.html");
                string file404 = Path.Combine(ghDirectory, "404.html");
                if (File.Exists(index))
                {
                    if (!File.Exists(file404)) File.Copy(index, file404);
                    else PluginHelper.printConsole("Distribution already exists '404.html'", ConsoleColor.Yellow);
                }
                else PluginHelper.printConsole("No exists 'index.html'", ConsoleColor.Red);
            }
            CommandRunner git = new CommandRunner("git", ghDirectory);
            PluginHelper.printConsole($"Adding all \"./dist\" files...", ConsoleColor.Yellow);
            git.Run($"add .");
            PluginHelper.printConsole($"Trying deploy to {config.repos}, branch {config.branch}", ConsoleColor.Yellow);
            git.Run($"commit -m \"{config.message}\"");
            PluginHelper.printConsole(git.Run($"push -f origin {config.branch}"));
            if (config.ping)
            {
                updateSitemap(distribution);
            }
            PluginHelper.printConsole("Done..", ConsoleColor.Green);
        }
        else
        {
            PluginHelper.printConsole("Config is null, please save you config into ../configs/GitDeploy.config.json", ConsoleColor.Red);
        }
    }
    private void updateSitemap(string distribution)
    {
        string robots = Path.Combine(distribution, "robots.txt");
        if (File.Exists(robots))
        {
            var result = Regex.Match(File.ReadAllText(robots), "Sitemap: (.*)(\n|$)", RegexOptions.Singleline);
            if (result.Success)
            {
                PluginHelper.printConsole("Found sitemap in robots.txt file, will update sitemap if modified.", ConsoleColor.Yellow);
                string urlOfSitemap = result.Result("$1");
                string sitemap = urlOfSitemap.Substring(urlOfSitemap.IndexOf("sitemap."));
                PluginHelper.printConsole($"Sitemap name: {sitemap}", ConsoleColor.Green);
                string sitemapDist = Path.Combine(distribution, sitemap);
                string sitemapRepos = Path.Combine(ghDirectory, sitemap);
                if (File.Exists(sitemapDist)) pingSitemap(urlOfSitemap);
                else PluginHelper.printConsole($"robots.txt contain sitemap but physical path '{sitemapDist}' not exists.", ConsoleColor.Red);
            }
        }
    }

    private void pingSitemap(string sitemapUrl)
    {
        HttpClient client = new HttpClient();
        PluginHelper.printConsole($"ping sitemap to google and bing now.");

        string[] enginesName =
        {
            "Google",
            "Bing",
        };
        var searchEngines = new Task<HttpResponseMessage>[]
        {
            client.GetAsync($"https://www.google.com/ping?sitemap={sitemapUrl}"), //google
            client.GetAsync($"https://bing.com/webmaster/ping.aspx?sitemap={sitemapUrl}"), //bing
        };

        Task.WaitAll(searchEngines);
        for (int i = 0; i < enginesName.Length; i++)
        {
            if (searchEngines[i].Result.IsSuccessStatusCode) PluginHelper.printConsole($"Success ping {enginesName[i]} to update sitemap with url '{sitemapUrl}'", ConsoleColor.Green);
            else PluginHelper.printConsole($"Failed ping {enginesName[i]} to update sitemap with url '{sitemapUrl}'", ConsoleColor.Red);
        }
    }

    private void cloneReposAndCheckout(GitConfig config)
    {
        CommandRunner git = new CommandRunner("git", ghDirectory);
        PluginHelper.printConsole(git.Run($"clone {config.repos} {ghDirectoryName}"));
        if (git.Run($"checkout -b {config.branch}").Contains("already exists")) git.Run($"checkout {config.branch}");
    }

    class CommandRunner
    {
        readonly string Command;
        readonly string WorkingDirectory;
        public CommandRunner(string command, string workingDirectory)
        {
            Command = command;
            WorkingDirectory = workingDirectory;
        }

        public string Run(string argument = "")
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c {Command}";
            if (OperatingSystem.IsLinux())
            {
                p.StartInfo.FileName = "/bin/bash";
                p.StartInfo.Arguments = $"-c {Command}";
            }
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.WorkingDirectory = WorkingDirectory;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            using (StreamReader s = p.StandardError)
            {
                output += s.ReadToEnd();
            }
            return output;
        }
    }

    public class GitConfig
    {
        public string? repos { get; set; }
        public string? branch { get; set; }
        public string? message { get; set; }
        public string? type { get; set; }
        public bool ping { get; set; } = false;
    }
}