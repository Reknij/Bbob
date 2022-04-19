namespace Bbob.Plugin;

internal static class SafeLocks
{
    internal static readonly object printConsole = new Object();
    internal static readonly object readConsole = new Object();
    internal static readonly object ConfigBbob = new Object();
    internal static readonly object HashPluginsLoaded = new Object();
    internal static readonly object registerAndGetObject = new Object();
    internal static readonly object registerAndGetMeta = new Object();
    internal static readonly object registerCustomCommand = new Object();
}