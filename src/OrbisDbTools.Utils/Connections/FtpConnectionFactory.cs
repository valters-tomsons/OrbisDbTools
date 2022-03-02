using FluentFTP;

namespace OrbisDbTools.Utils.Connections;

public static class FtpConnectionFactory
{
    public static async Task<IFtpClient?> OpenConnection(string ipAddress)
    {
        var parts = ipAddress.Split(':');

        var ip = parts.FirstOrDefault();
        var portString = parts.Length > 1 ? parts[1] : string.Empty;

        var isPort = int.TryParse(portString, out var port);

        if (!isPort)
        {
            port = 2121;
        }

        var ftpClient = new FtpClient(ip, port, new());
        await ftpClient.ConnectAsync();

        AssertClientValid(ftpClient);
        return ftpClient;
    }

    private static void AssertClientValid(IFtpClient client)
    {
        if (!client.IsConnected)
        {
            throw new Exception("Failed to connect, make sure FTP is turned on. Double check IP address (and port!)");
        }
    }
}