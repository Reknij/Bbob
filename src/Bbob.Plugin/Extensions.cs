namespace Bbob.Plugin;

public static class Extensions
{
    public static bool IsPropertyExists(object obj, string property)
    {
        IsPropertyExistsMain(obj, property, out bool exists, out object? pObj);
        return exists;
    }
    public static bool IsPropertyExists<T>(object obj, string property)
    {
        IsPropertyExistsMain(obj, property, out bool exists, out object? pObj);
        if (exists && pObj?.GetType() is T)
        {
            return true;
        }
        return false;
    }
    public static bool IsPropertyExists<T>(object obj, string property, out T? propertyWithType)
    {
        IsPropertyExistsMain(obj, property, out bool exists, out object? pObj);
        if (exists && pObj?.GetType() == typeof(T))
        {
            propertyWithType = (T)pObj;
            return true;
        }
        propertyWithType = default(T);
        return false;
    }
    public static void IsPropertyExistsMain(object obj, string property, out bool exists, out object? pObj)
    {
        if (obj is System.Dynamic.ExpandoObject)
        {
            var tar = (IDictionary<string, object>)obj;
            if (tar.ContainsKey(property))
            {
                exists = true;
                pObj = tar[property];
                return;
            }
        }
        var properties = ((object)obj).GetType().GetProperties();
        foreach (var p in properties)
        {
            if (p.Name == property)
            {
                exists = true;
                pObj = p.GetValue(obj);
                return;
            }
        }
        exists = false;
        pObj = null;
    }
}