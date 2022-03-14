namespace Bbob.Plugin;

/// <summary>
/// Operation of plugin executed.
/// </summary>
public enum CommandOperation
{
    None,
    Skip,
    Stop,
    RunMeAgain
}

/// <summary>
/// Result of command executed.
/// </summary>
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