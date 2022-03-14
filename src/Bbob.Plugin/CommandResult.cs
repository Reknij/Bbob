namespace Bbob.Plugin;

/// <summary>
/// Operation of plugin executed.
/// </summary>
public enum CommandOperation
{
    ///<summary>Continue executing</summary>
    None,
    ///<summary>Skip executing for this article file.</summary>
    Skip,
    ///<summary>Stop executing for all file.</summary>
    Stop,
    ///<summary>Run this plugin again.</summary>
    RunMeAgain
}

/// <summary>
/// Result of command executed.
/// </summary>
public struct CommandResult
{
    /// <summary>
    /// Message of your operation.
    /// </summary>
    /// <value></value>
    public string Message {get;set;}

    /// <summary>
    /// Operation for the plugin to do.
    /// </summary>
    /// <value>Default is CommandOperation.None</value>
    public CommandOperation Operation{get;set;}

    /// <summary>
    /// Initialize the CommandResult.
    /// </summary>
    /// <param name="messsage">Message of your operation.</param>
    /// <param name="operation">Operation for the plugin to do.</param>
    public CommandResult(string messsage = "", CommandOperation operation = CommandOperation.None)
    {
        Message = messsage;
        Operation = operation;
    }
}