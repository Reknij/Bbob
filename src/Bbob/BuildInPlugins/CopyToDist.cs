using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "CopyToDist", version = "1.0.0", author = "Jinker")]
public class CopyToDist : IPlugin
{
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

    static readonly string publicPath = Path.Combine(PluginHelper.CurrentDirectory, "public");
    public Action? CommandComplete(Commands commands)
    {
        if (commands == Commands.GenerateCommand && Directory.Exists(publicPath) && Directory.Exists(PluginHelper.DistributionDirectory))
        {
            PluginHelper.getPluginJsonConfig(out MyConfig? config);
            config ??= new MyConfig();
            PluginHelper.printConsole("Exists 'public' folder, will copy all files and directories of 'public' to distribution.");
            Shared.SharedLib.DirectoryHelper.CopyDirectory(publicPath, PluginHelper.DistributionDirectory, string.Empty, true, config.overwrite);
            HashSet<string> already = new HashSet<string>() { publicPath };
            foreach (var item in config.files)
            {
                if (!already.Contains(item))
                {
                    PluginHelper.printConsole($"Will copy files of '{item}' too.");
                    Shared.SharedLib.DirectoryHelper.CopyDirectory(item, PluginHelper.DistributionDirectory, string.Empty, true, config.overwrite);
                }
            }
        }
        return null;
    }

    public class MyConfig
    {
        public bool overwrite { get; set; } = true;
        public string[] files { get; set; } = Array.Empty<string>();
    }
}