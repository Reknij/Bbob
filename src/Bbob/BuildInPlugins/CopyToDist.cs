using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class CopyToDist: IPlugin
{
    static readonly string myFiles = Path.Combine(PluginHelper.CurrentDirectory, "MyFiles");
    string? distribution {get;set;}

    public void GenerateCommand(string filePath, string distribution, GenerationStage stage) => this.distribution = distribution;
    public void CommandComplete(Commands commands)
    {
        if (commands == Commands.GenerateCommand && Directory.Exists(myFiles) && Directory.Exists(distribution))
        {
            PluginHelper.printConsole("Exists 'MyFiles' folder, will copy all files and directories of 'MyFiles' to distribution.");
            Shared.SharedLib.DirectoryHelper.CopyDirectory(myFiles, distribution);
        }
    }
}