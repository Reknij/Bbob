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
            PluginHelper.printConsole($"Trying deploy to {config.repos}, branch {config.branch}", ConsoleColor.Yellow);
            if (!Directory.Exists(ghDirectory))
            {
                cloneReposAndCheckout(config);
            }
            else
            {
                if (!Regex.IsMatch(runCommand("git remote -v", ghDirectory), @$"origin\s+{config.repos}"))
                {
                    PluginHelper.printConsole("Exists other repository, replace it.", ConsoleColor.Yellow);
                    Shared.SharedLib.DirectoryHelper.DeleteDirectory(ghDirectory);
                    cloneReposAndCheckout(config);
                }
            }
            DeleteAll();
            Shared.SharedLib.DirectoryHelper.CopyDirectory(distribution, ghDirectory);
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
            runCommand($"git add .", ghDirectory);
            runCommand($"git commit -m \"{config.message}\"", ghDirectory);
            PluginHelper.printConsole(runCommand($"git push -f origin {config.branch}", ghDirectory));
            updateSitemap(distribution);
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
                if (File.Exists(sitemapDist))
                {
                    HttpClient client = new HttpClient();
                    PluginHelper.printConsole("ping sitemap to google now.");
                    var task = client.GetAsync($"https://www.google.com/ping?sitemap={urlOfSitemap}");
                    task.Wait();
                    if (task.Result.IsSuccessStatusCode) PluginHelper.printConsole($"Success ping google update sitemap with url '{urlOfSitemap}'", ConsoleColor.Green);
                    else PluginHelper.printConsole($"Failed ping google update sitemap with url '{urlOfSitemap}'", ConsoleColor.Red);
                }
                else
                {
                    PluginHelper.printConsole($"robots.txt contain sitemap but physical path '{sitemapDist}' not exists.", ConsoleColor.Red);
                }
            }
        }
    }
    private void cloneReposAndCheckout(GitConfig config)
    {
        PluginHelper.printConsole(runCommand($"git clone {config.repos} {ghDirectoryName}", PluginHelper.CurrentDirectory));
        if (runCommand($"git checkout -b {config.branch}", ghDirectory).Contains("already exists")) runCommand($"git checkout {config.branch}", ghDirectory);
    }

    private void DeleteAll()
    {
        string[] directories = Directory.GetDirectories(ghDirectory);
        string[] files = Directory.GetFiles(ghDirectory);
        foreach (string file in files)
        {
            File.Delete(file);
        }
        foreach (string directory in directories)
        {
            if (directory == Path.Combine(ghDirectory, ".git")) continue;
            Directory.Delete(directory, true);
        }
    }
    static string? WorkingDirectoryGlobal = null;
    private string runCommand(string command, string? workingDirectory = null)
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.Arguments = $"/c {command}";
        if (OperatingSystem.IsLinux())
        {
            p.StartInfo.FileName = "/bin/bash";
            p.StartInfo.Arguments = $"-c {command}";
        }
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.EnableRaisingEvents = true;
        p.StartInfo.WorkingDirectory = workingDirectory == null ? WorkingDirectoryGlobal : workingDirectory;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        using (StreamReader s = p.StandardError)
        {
            output += s.ReadToEnd();
        }
        return output;
    }

    public class GitConfig
    {
        public string? repos { get; set; }
        public string? branch { get; set; }
        public string? message { get; set; }
        public string? type { get; set; }
    }
}