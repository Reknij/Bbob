using System.Net.NetworkInformation;

namespace Bbob.Main.Cli;

public static class CliShared
{
    public enum TextType
    {
        Theme, Plugin, None
    }
    public static TextType isPluginOrThemeName(string name, out string fixedName)
    {
        fixedName = name;
        if (name.StartsWith("bbob-theme-")) return TextType.Theme;
        if (name.StartsWith("bbob-plugin-")) return TextType.Plugin;
        if (name.StartsWith("theme-"))
        {
            fixedName = $"bbob-{name}";
            return TextType.Theme;
        }
        if (name.StartsWith("plugin-"))
        {
            fixedName = $"bbob-{name}";
            return TextType.Plugin;
        }

        return TextType.None;
    }

    public static int GetAvailablePort(int startingPort)
    {
        if (startingPort > ushort.MaxValue) throw new ArgumentException($"Can't be greater than {ushort.MaxValue}", nameof(startingPort));
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
        var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
        var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
        var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                                             .Concat(udpListenersEndpoints)
                                             .Select(e => e.Port);

        return Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).FirstOrDefault();
    }
}