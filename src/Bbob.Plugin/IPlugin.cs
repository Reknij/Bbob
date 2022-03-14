using Bbob.Plugin;

namespace Bbob.Plugin;
public interface IPlugin
{
    /// <summary>
    /// Initialize command. Example 'i', 'init'.
    /// </summary>
    void InitCommand(){}

    /// <summary>
    /// New command. Example 'n', 'new'.
    /// </summary>
    /// <param name="filePath">File path of new command to save.</param>
    /// <param name="content">Content of file to save.</param>
    /// <param name="types">New command option.</param>
    void NewCommand(string filePath, ref string content, NewTypes types = NewTypes.blog){}

    /// <summary>
    /// Generate command. Example 'g', 'generate'.
    /// </summary>
    /// <param name="filePath">Article file path to process.</param>
    /// <param name="stage">Stage of generation.</param>
    void GenerateCommand(string filePath, GenerationStage stage){}

    /// <summary>
    /// Deploy command. Example 'd', 'deploy'.
    /// </summary>
    void DeployCommand(){}

    /// <summary>
    /// Will be executed when the target command is executed.
    /// </summary>
    /// <param name="commands">Command executed.</param>
    void CommandComplete(Commands commands){}
}
