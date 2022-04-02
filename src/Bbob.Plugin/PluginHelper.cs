using static Bbob.Plugin.Cores.PluginHelperCore;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System;

[assembly: InternalsVisibleTo("Bbob.Main")]
namespace Bbob.Plugin;

/// <summary>
/// To help your plugin to develop.
/// </summary>
public static class PluginHelper
{
    /// <summary>
    /// Path of target theme folder.
    /// </summary>
    /// <value></value>
    public static string ThemePath => themePath;

    /// <summary>
    /// Config of Bbob cli.
    /// </summary>
    /// <returns></returns>
    public static ConfigJson ConfigBbob => configBbob ?? throw new NullReferenceException("Config of bbob is null!");

    /// <summary>
    /// Information of executing plugin.
    /// </summary>
    /// <value></value>
    public static PluginJson ExecutingPlugin => executingPlugin ?? throw new NullReferenceException("Executing plugin is null.");

    /// <summary>
    /// Current path of executing Bbob cli.
    /// </summary>
    /// <value></value>
    public static string CurrentDirectory => currentDirectory;

    /// <summary>
    /// Base path of Bbob cli.
    /// </summary>
    /// <value></value>
    public static string BaseDirectory => baseDirectory;

    /// <summary>
    /// Bbob command distribution path.
    /// </summary>
    /// <returns></returns>
    public static string DistributionDirectory => Path.Combine(CurrentDirectory, ConfigBbob.distributionPath);

    /// <summary>
    /// Loaded plugin order.
    /// </summary>
    public static string[] PluginsLoadedOrder => pluginsLoadedOrder;

    /// <summary>
    /// Delegates of PluginHelper.
    /// </summary>
    public static class HelperDelegates
    {
        /// <summary>
        /// Delegate of modify object function.
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        public delegate void ModifyObjectDelegate<T>(ref T? obj);
    }

    /// <summary>
    /// Register object with target name to PluginHelper.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <param name="obj">Instance of object</param>
    /// <param name="option">Option of register object</param>
    public static void registerObject(string name, object? obj, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        if (pluginsObject.ContainsKey(name)) pluginsObject[name] = obj;
        else pluginsObject.Add(name, obj);
    }

    /// <summary>
    /// Check is exists object or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if exists, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool existsObject(string name, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        return pluginsObject.ContainsKey(name);
    }

    /// <summary>
    /// Check is exists object and is not null or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if exists and not null, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool existsObjectNoNull(string name, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        return pluginsObject.ContainsKey(name) && pluginsObject[name] != null;
    }

    /// <summary>
    /// Check is exists object and is not null or not.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>True if exists and not null, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool existsObjectNoNull<T>(string name, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        return pluginsObject.ContainsKey(name) && pluginsObject[name]?.GetType() == typeof(T);
    }

    /// <summary>
    /// Get register object from PluginHelper
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <param name="value">Found object. May be null.</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>True if object found, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool getRegisteredObject<T>(string name, out T? value, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        if (pluginsObject.TryGetValue(name, out object? v) && v is T)
        {
            value = (T)v;
            return true;
        }
        value = default(T);
        return false;
    }

    /// <summary>
    /// Get register object from PluginHelper and it not null. If it is null will throw exception. Please confirm it is not null.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <typeparam name="T">Type of object</typeparam>
    /// <returns>Object instance</returns>
    /// <param name="option">Option of register object</param>
    public static T getRegisteredObjectNoNull<T>(string name, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        bool exists = getRegisteredObject<T>(name, out T? a);
        if (exists && a != null)
        {
            return a;
        }
        throw new KeyNotFoundException($"Object name {name} " + (!exists ? "is not exists!" : "value is null!"));
    }

    /// <summary>
    /// Clear all register object from PluginHelper. You should not use it.
    /// </summary>
    public static void clearAllObject()
    {
        pluginsObject.Clear();
    }

    /// <summary>
    /// Get the target register object from PluginHelper to modify.
    /// </summary>
    /// <param name="name">Name of target object</param>
    /// <param name="modifyObjectDelegate">Function to modify object.</param>
    /// <typeparam name="T">Type of target object</typeparam>
    /// <returns>True if found object, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool modifyRegisteredObject<T>(string name, HelperDelegates.ModifyObjectDelegate<T> modifyObjectDelegate, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        if (pluginsObject.TryGetValue(name, out object? value) && value is T)
        {
            T? obj = (T?)value;
            modifyObjectDelegate?.Invoke(ref obj);
            pluginsObject[name] = obj;

            return true;
        }
        return false;
    }

    /// <summary>
    /// Unregister target object from PluginHelper.
    /// </summary>
    /// <param name="name">Name of object</param>
    /// <returns>True if remove success, otherwise false.</returns>
    /// <param name="option">Option of register object</param>
    public static bool unregisterObject(string name, RegisterObjectOption? option = null)
    {
        if (option != null) option.Process(ref name);
        return pluginsObject.Remove(name);
    }

    /// <summary>
    /// Get target plugin config with target type.
    /// </summary>
    /// <param name="pluginName">Name of target plugin</param>
    /// <param name="config">Result of target plugin config.</param>
    /// <typeparam name="T">Type of instance result.</typeparam>
    /// <returns>True if success get config, otherwise false.</returns>
    public static bool getPluginJsonConfig<T>(string pluginName, out T? config)
    {
        string configsDirectory = Path.Combine(CurrentDirectory, "configs");

        string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
        if (File.Exists(pluginConfigJson))
        {
            try
            {
                using (FileStream fs = new FileStream(pluginConfigJson, FileMode.Open, FileAccess.Read))
                {
                    config = JsonSerializer.Deserialize<T>(fs);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[{ExecutingPlugin.name}]: Get plugin config error:\n{ex.ToString()}");
            }
        }
        config = default(T);
        return false;
    }

    /// <summary>
    /// Get executing plugin config with target type.
    /// </summary>
    /// <param name="config">Result of plugin config.</param>
    /// <typeparam name="T">Target type you want parse config.</typeparam>
    /// <returns>True if success get config, otherwise false.</returns>
    public static bool getPluginJsonConfig<T>(out T? config) =>
    getPluginJsonConfig<T>(ExecutingPlugin.name ?? throw new NullReferenceException("Executing plugin name is null."), out config);


    private static JsonSerializerOptions savePluginJsonConfigOptions = new JsonSerializerOptions()
    {
        WriteIndented = true
    };
    /// <summary>
    /// Save object to target plugin config. It is json file.
    /// </summary>
    /// <param name="pluginName">Target plugin name</param>
    /// <param name="config">Object config</param>
    /// <param name="options">Json save options.</param>
    /// <typeparam name="T">Type of object config</typeparam>
    public static void savePluginJsonConfig<T>(string pluginName, T config, JsonSerializerOptions? options = null)
    {
        try
        {
            string configsDirectory = Path.Combine(CurrentDirectory, "configs");
            string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
            if (File.Exists(pluginConfigJson)) File.Delete(pluginConfigJson);
            using (FileStream fs = File.OpenWrite(pluginConfigJson))
            {
                JsonSerializer.Serialize<T>(fs, config, options ?? savePluginJsonConfigOptions);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[{ExecutingPlugin.name}]: Save plugin config error:\n{ex.ToString()}");
        }
    }

    /// <summary>
    /// Save object to executing plugin config. It is json file.
    /// </summary>
    /// <param name="config">Object config</param>
    /// <param name="options">Json save options.</param>
    /// <typeparam name="T">Type of object config</typeparam>
    public static void savePluginJsonConfig<T>(T config, JsonSerializerOptions? options = null) =>
    savePluginJsonConfig<T>(ExecutingPlugin.name, config, options);

    /// <summary>
    /// Check plugin config is exists or not.
    /// </summary>
    /// <param name="pluginName">Name of plugin</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool isPluginJsonConfigExists(string pluginName)
    {
        string configsDirectory = Path.Combine(CurrentDirectory, "configs");
        string pluginConfigJson = Path.Combine(configsDirectory, $"{pluginName}.config.json");
        return File.Exists(pluginConfigJson);
    }

    /// <summary>
    /// Check executing plugin config is exists or not.
    /// </summary>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool isPluginJsonConfigExists() => isPluginJsonConfigExists(ExecutingPlugin.name);

    /// <summary>
    /// Print message to console of Bbob.
    /// </summary>
    /// <param name="msg">Message object. Will use ToString() function.</param>
    public static void printConsole(object? msg)
    {
        msg ??= "<NULL_OBJECT>";
        System.Console.WriteLine($"[{ExecutingPlugin.name}]: {msg.ToString()}");
    }

    /// <summary>
    /// Read message from console of Bbob.
    /// </summary>
    /// <returns>The next line of characters from the input stream, or null if no more lines are available.</returns>
    public static string? readConsole()
    {
        return Console.ReadLine();
    }

    /// <summary>
    /// Read key from console of Bbob.
    /// </summary>
    /// <param name="intercept">Determines whether to display the pressed key in the console window. true to not display the pressed key; otherwise, false.</param>
    /// <returns></returns>
    public static ConsoleKeyInfo readConsoleKey(bool intercept = true)
    {
        return Console.ReadKey(intercept);
    }

    /// <summary>
    /// Register meta with name of executing plugin.
    /// </summary>
    /// <param name="meta">Meta object</param>
    /// <param name="option">Option of register meta</param>
    public static void registerMeta(object meta, RegisterMetaOption? option = null) => registerMeta(ExecutingPlugin.name, meta, option);

    /// <summary>
    /// Register meta with target name.
    /// </summary>
    /// <param name="metaName">Name of meta</param>
    /// <param name="meta">Meta object</param>
    /// <param name="option">Option of register meta</param>
    public static void registerMeta(string metaName, object meta, RegisterMetaOption? option = null)
    {
        option ??= new RegisterMetaOption();
        if (metas.ContainsKey(metaName))
        {
            if (!option.Merge) metas[metaName] = meta;
            else
            {
                if (meta is JsonDocument newDocument)
                {
                    if (metas[metaName] is JsonDocument originalDoc) metas[metaName] = JsonDocument.Parse(JsonHelper.Merge(null, null, originalDoc, newDocument));
                    else metas[metaName] = JsonDocument.Parse(JsonHelper.Merge(JsonSerializer.Serialize(metas[metaName]), null, null, newDocument));
                }
                else
                {
                    string rmeta = JsonSerializer.Serialize(meta);
                    if (metas[metaName] is JsonDocument originalDoc) metas[metaName] = JsonDocument.Parse(JsonHelper.Merge(null, rmeta, originalDoc));
                    else metas[metaName] = JsonDocument.Parse(JsonHelper.Merge(JsonSerializer.Serialize(metas[metaName]), rmeta));
                }

            }
        }
        else metas.Add(metaName, meta);
    }

    /// <summary>
    /// Unregister target meta name is plugin name.
    /// </summary>
    /// <returns>True if exists and removed, otherwise false.</returns>
    public static bool unregisterMeta() => unregisterMeta(ExecutingPlugin.name);

    /// <summary>
    /// Unregister target meta.
    /// </summary>
    /// <param name="metaName">Name of meta</param>
    /// <returns>True if exists and removed, otherwise false.</returns>
    public static bool unregisterMeta(string metaName)
    {
        if (metas.ContainsKey(metaName))
        {
            metas.Remove(metaName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get theme.json with target type.
    /// </summary>
    /// <typeparam name="T">Target type to parse theme.json.</typeparam>
    /// <returns>Type instance of theme.json.</returns>
    public static T? getThemeInfo<T>()
    {
        try
        {
            using (FileStream fs = File.OpenRead(Path.Combine(ThemePath, "theme.json")))
            {
                return JsonSerializer.Deserialize<T>(fs);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[{ExecutingPlugin.name}]: Get theme info error:\n{ex.ToString()}");
            return default(T);
        }
    }

    /// <summary>
    /// Check plugin is done or not.
    /// </summary>
    /// <param name="names">Name of plugins.</param>
    /// <returns>True if done, otherwise false.</returns>
    public static bool isTargetPluginDone(string[] names)
    {
        foreach (var name in names)
        {
            if (!isTargetPluginDone(name)) return false;
        }
        return true;
    }

    /// <summary>
    /// Check plugin is done or not.
    /// </summary>
    /// <param name="name">Name of plugin.</param>
    /// <returns>True if done, otherwise false.</returns>
    public static bool isTargetPluginDone(string name)
    {
        return pluginsDone.Contains(name);
    }

    /// <summary>
    /// Check plugin is enable and is done or not.
    /// </summary>
    /// <param name="name">Name of plugin.</param>
    /// <returns>True if enable and done, otherwise false.</returns>
    public static bool isTargetPluginEnableAndDone(string name)
    {
        return isTargetPluginEnable(name) && isTargetPluginDone(name);
    }

    /// <summary>
    /// Check plugin is enable or disable.
    /// </summary>
    /// <param name="name">Name of plugin.</param>
    /// <returns>True if enable, otherwise false.</returns>
    public static bool isTargetPluginEnable(string name) => ConfigBbob.isPluginEnable(name);

    /// <summary>
    /// Check plugin is loaded or not.
    /// </summary>
    /// <param name="name">Name of plugin.</param>
    /// <returns></returns>
    public static bool isTargetPluginLoaded(string name)
    {
        return pluginsLoaded.Contains(name.ToUpper());
    }

    /// <summary>
    /// Check plugins is loaded or not.
    /// </summary>
    /// <param name="names">Name of plugins.</param>
    /// <returns></returns>
    public static bool isTargetPluginLoaded(string[] names)
    {
        foreach (var name in names)
        {
            if (!isTargetPluginLoaded(name)) return false;
        }
        return true;
    }

    /// <summary>
    /// Bbob will check the command operation in command result to determine execution next.
    /// </summary>
    /// <value></value>
    public static CommandResult ExecutingCommandResult { get; set; }

    /// <summary>
    /// Register custom command from your plugin.
    /// </summary>
    /// <param name="command">Command to register.</param>
    /// <param name="function">Function to execute which command called.</param>
    /// <param name="option">Option of register command</param>
    /// <returns>True if not register then add success. Otherwise already exists and add failed.</returns>
    public static bool registerCustomCommand(string command, Action<string[]> function, RegisterCommandOption? option = null)
    {
        PluginJson registerPlugin = ExecutingPlugin;
        string pluginName = ExecutingPlugin.name.ToUpper();
        option = option ?? new RegisterCommandOption();
        Action<string[]> registerFunction = (args) =>
        {
            executingPlugin = registerPlugin;
            function(args);
        };
        if (option.Global)
        {
            if (string.IsNullOrWhiteSpace(customGlobalCommandKey)) customGlobalCommandKey = $"globalCommand{Random.Shared.Next(131452099)}_";
            command += customGlobalCommandKey;
            if (!customCommands.ContainsKey(command)) customCommands.Add(command, new Dictionary<string, Action<string[]>>());
            if (customCommands[command].ContainsKey(pluginName)) return false;
            customCommands[command].Add(pluginName, registerFunction);
        }
        else
        {
            if (!customCommands.ContainsKey(pluginName)) customCommands.Add(pluginName, new Dictionary<string, Action<string[]>>());
            if (customCommands[pluginName].ContainsKey(command)) return false;
            customCommands[pluginName].Add(command, registerFunction);
        }
        return true;
    }
}