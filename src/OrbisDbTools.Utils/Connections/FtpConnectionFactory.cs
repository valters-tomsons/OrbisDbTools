using System.Text;
using System.Net;
using System.Security.Authentication;
using FluentFTP;

namespace OrbisDbTools.Utils.Connections;

public static class FtpConnectionFactory
{
    public static IFtpClient CreateClient(IPEndPoint endpoint)
    {
        var profile = new FtpProfile()
        {
            Host = endpoint.Address.ToString(),
            Credentials = new NetworkCredential("", ""),
            Encryption = FtpEncryptionMode.None,
            Protocols = SslProtocols.Tls11 | SslProtocols.Tls12,
            DataConnection = FtpDataConnectionType.PORT,
            Encoding = Encoding.UTF8,
        };

        var client = new FtpClient();
        client.LoadProfile(profile);

        client.Port = endpoint.Port == 0 ? 2121 : endpoint.Port;
        client.ConnectTimeout = 5000;
        client.ValidateAnyCertificate = true;

        return client;
    }
}