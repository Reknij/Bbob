namespace Bbob.Plugin;

/// <summary>
/// Sort order of plugins.
/// </summary>
public enum PluginOrder
{
    Any,
    BeforeMe,
    AfterMe
}

/// <summary>
/// Condition types
/// </summary>
public enum ConditionType
{
    Require = 2,
    OrderCheck = 4,
    All = Require | OrderCheck
}

/// <summary>
/// Condition of plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginCondition : Attribute
{
    /// <summary>
    /// Set the condition type.
    /// </summary>
    /// <value></value>
    public ConditionType ConditionType {get;set;} = ConditionType.All;

    /// <summary>
    /// Set the sort order of plugin.
    /// </summary>
    /// <value></value>
    public PluginOrder PluginOrder {get;set;} = PluginOrder.Any;

    /// <summary>
    /// Show warning if require target plugin is not enable or contain.
    /// </summary>
    /// <value>Default true</value>
    public bool ShowWarning {get;set;} = true;

    /// <summary>
    /// Name of target plugin about condition
    /// </summary>
    /// <value></value>
    public string PluginName {get;set;}
    public PluginCondition(string pluginName)
    {
        this.PluginName = pluginName;
    }
}