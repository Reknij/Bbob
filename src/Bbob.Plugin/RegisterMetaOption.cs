namespace Bbob.Plugin;
/// <summary>
/// Option of register meta
/// </summary>
public class RegisterMetaOption
{
    /// <summary>
    /// If already exists target meta, merge or not. False will replace exists meta.
    /// </summary>
    /// <value>Default false</value>
    public bool Merge {get;set;} = false;
}