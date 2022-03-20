using FluentFTP;

namespace OrbisDbTools.Utils.Connections;

public static class FtpConnectionFactory
{
    public static IFtpClient CreateClient(string ipAddress)
    {
        var parts = ipAddress.Split(':');

        var ip = parts.FirstOrDefault();
        var portString = parts.Length > 1 ? parts[1] : string.Empty;

        var isPort = int.TryParse(portString, out var port);

        if (!isPort)
        {
            port = 2121;
        }

        return new FtpClient(ip, port, new());
    }
}