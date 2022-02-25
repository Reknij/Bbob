using Bbob.Plugin;

namespace Bbob.Plugin;
public interface IPlugin
{
    void InitCommand(){}
    void NewCommand(string filePath, ref string content, NewTypes types = NewTypes.blog){}
    void GenerateCommand(string filePath, string distribution, GenerationStage stage){}
    void DeployCommand(string distribution){}
    void CommandComplete(Commands commands){}
}
