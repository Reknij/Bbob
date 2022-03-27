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
        if (startingPort < 1024 || startingPort > 49151) throw new Exception("startingPort must in 1024 - 49151");
        var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

        var usedPorts =
            ipProperties.GetActiveTcpConnections()
                .Where(connection => connection.State == TcpState.Listen)
                .Select(connection => connection.LocalEndPoint)
                .Concat(ipProperties.GetActiveTcpListeners())
                .Concat(ipProperties.GetActiveUdpListeners())
                .Select(endpoint => endpoint.Port)
                .ToArray();

        while (startingPort <= 49151)
        {
            if (!usedPorts.Contains(startingPort)) return startingPort;
            else startingPort++;
        }
        throw new Exception($"All local TCP ports between {startingPort} and {49151} are currently in use.");
    }
}