namespace Bbob.Plugin;

/// <summary>
/// Sort order of plugins.
/// </summary>
public enum PluginOrder
{
    /// <summary>Ignore sort order</summary>
    Any,
    /// <summary>Must before your plugin</summary>
    BeforeMe,
    /// <summary>Must after your plugin</summary>
    AfterMe
}

/// <summary>
/// Condition types
/// </summary>
public enum ConditionType
{
    /// <summary>Your plugin is require target plugin</summary>
    Require = 2,
    /// <summary>Your plugin must sort order with target plugin</summary>
    OrderCheck = 4,
    /// <summary>Your plugin is require and must sort order with target plugin</summary>
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

    /// <summary>
    /// Initialize PluginCondition with target plugin name.
    /// </summary>
    /// <param name="pluginName"></param>
    public PluginCondition(string pluginName)
    {
        this.PluginName = pluginName;
    }
}