namespace Bbob.Plugin;

public enum PluginOrder
{
    Any,
    BeforeMe,
    AfterMe
}

public enum ConditionType
{
    Require = 2,
    StatusCheck = 4,
    All = Require | StatusCheck
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginCondition : Attribute
{
    public ConditionType ConditionType {get;set;} = ConditionType.All;
    public PluginOrder PluginOrder {get;set;} = PluginOrder.Any;
    public bool ShowWarning {get;set;} = true;
    public string PluginName {get;set;}
    public PluginCondition(string pluginName)
    {
        this.PluginName = pluginName;
    }
}