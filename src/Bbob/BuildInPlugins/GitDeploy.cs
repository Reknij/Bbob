using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class GitDeploy : IPlugin
{
    static readonly string ghDirectoryName = ".deploy_git";
    static readonly string ghDirectory = Path.Combine(PluginHelper.CurrentDirectory, ghDirectoryName);
    public void DeployCommand(string distribution)
    {
        if (PluginHelper.getPluginJsonConfig<GitConfig>("GitDeploy", out var config) && config != null)
        {
            if (config.branch == null) config.branch = "main";
            if (config.message == null) config.message = $"{Shared.SharedLib.DateTimeHelper.GetDateTimeNowString()} Update";
            PluginHelper.printConsole($"Trying deploy to {config.repos}, branch {config.branch}");
            if (!Directory.Exists(ghDirectory))
            {
                cloneReposAndCheckout(config);
            }
            else
            {
                if (!Regex.IsMatch(runCommand("git remote -v", ghDirectory), @$"origin\s+{config.repos}"))
                {
                    PluginHelper.printConsole("Exists other repository, replace it.");
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
                    else PluginHelper.printConsole("Distribution already exists '404.html'");
                }
                else PluginHelper.printConsole("No exists 'index.html'");
            }
            runCommand($"git add .", ghDirectory);
            runCommand($"git commit -m \"{config.message}\"", ghDirectory);
            PluginHelper.printConsole(runCommand($"git push -f origin {config.branch}", ghDirectory));
            updateSitemap(distribution);
            PluginHelper.printConsole("Done..");
        }
        else
        {
            PluginHelper.printConsole("Config is null, please save you config into ../configs/GitDeploy.config.json");
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
                PluginHelper.printConsole("Found sitemap in robots.txt file, will update sitemap if modified.");
                string urlOfSitemap = result.Result("$1");
                string sitemap = urlOfSitemap.Substring(urlOfSitemap.IndexOf("sitemap."));
                PluginHelper.printConsole($"Sitemap name: {sitemap}");
                string sitemapDist = Path.Combine(distribution, sitemap);
                string sitemapRepos = Path.Combine(ghDirectory, sitemap);
                if (File.Exists(sitemapDist))
                {
                    SHA256 sHA256 = SHA256.Create();
                    string hashDist = Shared.SharedLib.BytesToString(sHA256.ComputeHash(File.OpenRead(sitemapDist)));
                    string hashRepos = "";
                    if (File.Exists(sitemapRepos))
                    {
                        hashRepos = Shared.SharedLib.BytesToString(sHA256.ComputeHash(File.OpenRead(sitemapRepos)));
                    }
                    if (hashDist != hashRepos)
                    {
                        HttpClient client = new HttpClient();
                        PluginHelper.printConsole("ping sitemap to google now.");
                        var task = client.GetAsync($"https://www.google.com/ping?sitemap={urlOfSitemap}");
                        task.Wait();
                        PluginHelper.printConsole((task.Result.IsSuccessStatusCode ? "Success" : "Failed") + $" ping google update sitemap with url '{urlOfSitemap}'");
                    }
                    else
                    {
                        PluginHelper.printConsole("Sitemap file content is same with git repository.");
                        PluginHelper.printConsole($"Hash of distribution: {hashDist}");
                        PluginHelper.printConsole($"Hash of git repository: {hashRepos}");
                    }
                }
                else
                {
                    PluginHelper.printConsole($"robots.txt contain sitemap but physical path '{sitemapDist}' not exists.");
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