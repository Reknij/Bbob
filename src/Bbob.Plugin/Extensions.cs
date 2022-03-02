namespace Bbob.Plugin;

public static class Extensions
{
    public static bool IsPropertyExists(object obj, string property)
    {
        bool exists = IsPropertyExistsMain(obj, property, out object? pObj);
        return exists;
    }
    public static bool IsPropertyExists<T>(object obj, string property)
    {
        bool exists = IsPropertyExistsMain(obj, property, out object? pObj);
        if (exists && pObj is T)
        {
            return true;
        }
        return false;
    }
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