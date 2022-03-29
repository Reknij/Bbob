
/// <summary>
/// Option of register object
/// </summary>
public class RegisterObjectOption
{
    /// <summary>
    /// Group of name. Will add to front name. Etc. name = $"{group}-{name}"
    /// </summary>
    /// <value></value>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Process fields
    /// </summary>
    /// <param name="name">Name of register object.</param>
    internal void Process(ref string name)
    {
        if (!string.IsNullOrWhiteSpace(Group)) name = $"{Group}-{name}";
    }
}