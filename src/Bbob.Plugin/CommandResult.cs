namespace Bbob.Plugin;

public enum CommandOperation
{
    None,
    Skip,
    Stop
}
public struct CommandResult
{
    public string Message {get;set;}
    public CommandOperation Operation{get;set;}
    public CommandResult(string messsage = "", CommandOperation operation = CommandOperation.None)
    {
        Message = messsage;
        Operation = operation;
    }
}