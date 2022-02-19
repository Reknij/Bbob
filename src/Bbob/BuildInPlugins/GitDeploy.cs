using System.Diagnostics;
using System.Text.RegularExpressions;
using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class GitDeploy : IPlugin
{
    static readonly string ghDirectoryName = ".deploy_git";
    static readonly string ghDirectory = Path.Combine(PluginHelper.ExecutingDirectory, ghDirectoryName);
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
                File.Copy(index, file404);
            }
            runCommand($"git add .", ghDirectory);
            runCommand($"git commit -m \"{config.message}\"", ghDirectory);
            PluginHelper.printConsole(runCommand($"git push -f origin {config.branch}", ghDirectory));
            PluginHelper.printConsole("Done..");
        }
        else
        {
            PluginHelper.printConsole("Config is null, please save you config into ../configs/GitDeploy.config.json");
        }
    }

    private void cloneReposAndCheckout(GitConfig config)
    {
        PluginHelper.printConsole(runCommand($"git clone {config.repos} {ghDirectoryName}", PluginHelper.ExecutingDirectory));
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
        public string? type {get;set;}
    }
}