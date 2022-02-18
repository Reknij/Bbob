
using System.Text.Json;

namespace Bbob.Plugin;
public static class PluginHelper
{
    private static PluginJson? _ExecutingPlugin;
    public static PluginJson ExecutingPlugin
    {
        get => _ExecutingPlugin ?? throw new NullReferenceException("Executing plugin is null.");
        set => _ExecutingPlugin = value;
    }
    private static string? _executingDirectory;
    public static string ExecutingDirectory
    {
        get => _executingDirectory ?? throw new NullReferenceException("executing directory is null.");
        set => _executingDirectory = value;
    }
    public static class HelperDelegates
    {
        public delegate void ModifyObjectDelegate<T>(ref T? obj);
    }
    static Dictionary<string, object?> pluginsObject = new Dictionary<string, object?>();

    public static void registerObject(string name, object? obj)
    {
        if (pluginsObject.ContainsKey(name)) pluginsObject[name] = obj;
        else pluginsObject.Add(name, obj);
    }
    public static bool getRegisteredObject<T>(string name, out T? value)
    {
        if (pluginsObject.TryGetValue(name, out object? v) && v is T)
        {
            value = (T)v;
            return true;
        }
        value = default(T);
        return false;
    }

    public static void clearAllObject()
    {
        pluginsObject.Clear();
    }

    public static bool modifyRegisteredObject<T>(string name, HelperDelegates.ModifyObjectDelegate<T> modifyObjectDelegate)
    {
        if (pluginsObject.TryGetValue(name, out object? value) && value is T)
        {
            T? obj = (T?)value;
            modifyObjectDelegate?.Invoke(ref obj);
            pluginsObject[name] = obj;

            return true;
        }
        return false;
    }

    public static bool getPluginJsonConfig<T>(string pluginName, out T? config)
    {
        string pluginConfigsFolder = Path.Combine(Environment.CurrentDirectory, "configs");
        string pluginConfigJson = Path.Combine(pluginConfigsFolder, $"{pluginName}.config.json");
        if (File.Exists(pluginConfigJson))
            using (FileStream fs = new FileStream(pluginConfigJson, FileMode.Open, FileAccess.Read))
            {
                config = JsonSerializer.Deserialize<T>(fs);
                return true;
            }
        config = default(T);
        return false;
    }
    public static bool getPluginJsonConfig<T>(out T? config) =>
    getPluginJsonConfig<T>(ExecutingPlugin.name ?? throw new NullReferenceException("Executing plugin name is null."), out config);

    public static void savePluginJsonConfig<T>(string pluginName, T config)
    {
        string pluginConfigsFolder = Path.Combine(Environment.CurrentDirectory, "configs");
        string pluginConfigJson = Path.Combine(pluginConfigsFolder, $"{pluginName}.config.json");
        if (File.Exists(pluginConfigJson)) File.Delete(pluginConfigJson);
        using (FileStream fs = File.OpenWrite(pluginConfigJson))
        {
            JsonSerializer.Serialize<T>(fs, config);
        }
    }
    public static void savePluginJsonConfig<T>(T config) =>
    savePluginJsonConfig<T>(ExecutingPlugin.name ?? throw new NullReferenceException("Executing plugin name is null."), config);
    public static void printConsole(string msg)
    {
        System.Console.WriteLine($"[{ExecutingPlugin.name}]: {msg}");
    }

    public static sortArticlesDelegate? sortArticles { get; set; }
    public static sortCategoriesDelegate? sortCategories { get; set; }
    public static sortTagsDelegate? sortTags { get; set; }
}