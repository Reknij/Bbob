namespace Bbob.Plugin;

/// <summary>
/// PluginHelper provide extension function to help develop.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Check the target object is exists target property with name or not.
    /// </summary>
    /// <param name="obj">Object for check</param>
    /// <param name="property">Name of property</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool IsPropertyExists(object obj, string property)
    {
        bool exists = IsPropertyExistsMain(obj, property, out object? pObj);
        return exists;
    }

    /// <summary>
    /// Check the target object is exists target property with name or not.
    /// </summary>
    /// <param name="obj">Object for check</param>
    /// <param name="property">Name of property</param>
    /// <typeparam name="T">Type of property object</typeparam>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool IsPropertyExists<T>(object obj, string property)
    {
        bool exists = IsPropertyExistsMain(obj, property, out object? pObj);
        if (exists && pObj is T)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check the target object is exists target property with name or not.
    /// </summary>
    /// <param name="obj">Object for check</param>
    /// <param name="property">Name of property</param>
    /// <typeparam name="T">Type of property object</typeparam>
    /// <param name="propertyWithType">Get the property from object with target type.</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool IsPropertyExists<T>(object obj, string property, out T? propertyWithType)
    {
        bool exists = IsPropertyExistsMain(obj, property, out object? pObj);
        if (exists && pObj is T)
        {
            propertyWithType = (T)pObj;
            return true;
        }
        propertyWithType = default(T);
        return false;
    }
    
    private static bool IsPropertyExistsMain(object obj, string property, out object? pObj)
    {
        if (obj is IDictionary<string, object?> tar)
        {
            if (tar.ContainsKey(property))
            {
                pObj = tar[property];
                return true;
            }
        }
        var properties = ((object)obj).GetType().GetProperties();
        foreach (var p in properties)
        {
            if (p.Name == property)
            {
                pObj = p.GetValue(obj);
                return true;
            }
        }
        pObj = null;
        return false;
    }
}