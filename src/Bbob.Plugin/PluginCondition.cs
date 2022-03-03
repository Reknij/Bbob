namespace Bbob.Plugin;

public enum PluginStatus
{
    Waiting,
    Done
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginCondition : Attribute
{
    public string? RequirePlugin {get;set;} = null;
    public PluginStatus? RequirePluginStatus {get;set;} = null;
    public bool ShowWarning {get;set;} = true;
}