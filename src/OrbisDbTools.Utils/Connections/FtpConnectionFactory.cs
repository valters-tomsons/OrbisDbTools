using System.Net;
using FluentFTP;

namespace OrbisDbTools.Utils.Connections;

public static class FtpConnectionFactory
{
    public static IFtpClient CreateClient(IPEndPoint endpoint)
    {
        var host = endpoint.Address.ToString();
        var port = endpoint.Port;

        if (port == 0)
        {
            port = 2121;
        }

        return new FtpClient(host, port, new());
    }
}