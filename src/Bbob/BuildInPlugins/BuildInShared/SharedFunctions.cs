namespace Bbob.Main.BuildInPlugin.BuildInShared;

public static class SharedFunctions
{
    public static int GetLengthFromAny(object obj)
    {
        if (obj is Array) return ((Array)obj).Length;
        if (obj is IDictionary<object, object>) return ((IDictionary<object, object>)obj).Count;
        if (obj is IList<object>) return ((IList<object>)obj).Count;
        throw new Exception("Unknown type. Can't get length.");
    }
}