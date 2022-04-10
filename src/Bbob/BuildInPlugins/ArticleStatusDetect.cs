using Bbob.Plugin;

namespace Bbob.Main.BuildInPlugin;

[PluginJson(name = "ArticleStatusDetect", version = "1.0.0", author = "Jinker")]
[PluginCondition("*", PluginOrder = PluginOrder.AfterMe)]
public class ArticleStatusDetect : IPlugin
{
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage == GenerationStage.Process)
        {
            if (PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article))
            {
                if (article == null) return;
                if (Extensions.IsPropertyExists<bool>(article, "important", out bool important) && important)
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("It is important article. Please complete it first.", CommandOperation.Stop);
                }
                else if (Extensions.IsPropertyExists<bool>(article, "draft", out bool draft) && draft)
                {
                    PluginHelper.ExecutingCommandResult = new CommandResult("It is draft.", CommandOperation.Skip);
                }
                else if (Extensions.IsPropertyExists<string>(article, "notice", out string? notice) && notice != null)
                {
                    PluginHelper.printConsole(notice, ConsoleColor.Yellow);
                    ConsoleKeyInfo info = PluginHelper.readConsoleKey("Enter 'y' to continue generate this article or 'n' to skip it: ");
                    while (info.Key != ConsoleKey.Y && info.Key != ConsoleKey.N)
                    {
                        info = PluginHelper.readConsoleKey("Please enter 'y' or 'n': ", false);
                    };
                    if (info.Key == ConsoleKey.N) PluginHelper.ExecutingCommandResult = new CommandResult(CommandOperation.Skip);
                }
            }
        }
    }
}