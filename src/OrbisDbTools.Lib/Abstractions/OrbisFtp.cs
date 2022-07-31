using System.Net;
using FluentFTP;
using OrbisDbTools.Utils.Connections;

namespace OrbisDbTools.Lib.Abstractions;

public class OrbisFtp : IAsyncDisposable
{
    private IFtpClient? _ftpClient;
    private FtpProfile? _ftpProfile;

    public async ValueTask DisposeAsync()
    {
        if (_ftpClient is not null)
        {
            await _ftpClient.DisconnectAsync();
        }

        GC.SuppressFinalize(this);
    }

    public async Task<bool> OpenConnection(IPEndPoint endpoint, CancellationToken cts = default)
    {
        _ftpClient = FtpConnectionFactory.CreateClient(endpoint);
        _ftpClient.ConnectTimeout = 5000;

        _ftpProfile = await _ftpClient.AutoConnectAsync(cts);

        var success = _ftpClient.IsConnected;
        if (!success && cts.IsCancellationRequested)
        {
            throw new Exception("Connection cancelled by user");
        }
        else if (!success)
        {
            throw new Exception($"Could not connect to '{_ftpClient.Host}:{_ftpClient.Port}'.");
        }

        Console.WriteLine($"FTP connection seems successful. Protocols: {_ftpProfile.Protocols}");
        return success;
    }

    public bool IsConnected()
    {
        return _ftpClient?.IsConnected == true;
    }

    public async Task<bool> DownloadFile(Uri localPath, string remotePath)
    {
        if (!IsConnected())
        {
            throw new Exception("Cannot download file because FTP is not connected.");
        }

        var resultStatus = await _ftpClient!.DownloadFileAsync(localPath.LocalPath, remotePath);
        return resultStatus == FtpStatus.Success;
    }

    public async Task<Stream?> OpenFileStream(string remotePath)
    {
        if (!IsConnected())
        {
            throw new Exception("Cannot download file because FTP is not connected.");
        }

        return await _ftpClient!.OpenReadAsync(remotePath, FtpDataType.Binary, 0, false);
    }

    public async Task<IDictionary<string, long>?> ListFilesAndSizes(string remotePath, bool recursive)
    {
        if (!IsConnected())
        {
            throw new Exception("Cannot download file because FTP is not connected.");
        }

        var exists = await _ftpClient!.DirectoryExistsAsync(remotePath);
        if (!exists)
        {
            return null;
        }

        var listingOption = recursive ? FtpListOption.Recursive : FtpListOption.Auto;
        var listing = await _ftpClient!.GetListingAsync(remotePath, listingOption);

        return listing.ToDictionary(x => x.FullName, y => y.Size);
    }

    public async Task<IList<string>> ListTitleDlcFiles(string remotePath)
    {
        if (!IsConnected())
        {
            throw new Exception("Cannot download file because FTP is not connected.");
        }

        var exists = await _ftpClient!.DirectoryExistsAsync(remotePath);
        if (!exists)
        {
			return Array.Empty<string>();
        }

        var listing = await _ftpClient!.GetListingAsync(remotePath, FtpListOption.Recursive);

        return listing.Where(x => x.FullName.EndsWith("ac.pkg")).Select(x => x.FullName).ToList();
    }

    public async Task<long?> FileSizeInBytes(string remoteFilePath)
    {
        if (!IsConnected())
        {
            throw new Exception("Cannot download file because FTP is not connected.");
        }

        var fileInfo = await _ftpClient!.GetObjectInfoAsync(remoteFilePath) ?? null;
        return fileInfo?.Size;
    }
}