using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

public class SkipIfDraft : IPlugin
{
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage == GenerationStage.Process)
        {
            if (PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article))
            {
                if (article == null) return;
                if (Extensions.IsPropertyExists<bool>(article, "draft", out bool value) && value)
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("It is draft.", CommandOperation.Skip);
                }
            }
        }
    }
}