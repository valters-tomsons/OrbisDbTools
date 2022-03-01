using FluentFTP;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils;
using OrbisDbTools.Utils.Connections;
using OrbisDbTools.PS4.Enums;

namespace OrbisDbTools.PS4.Discovery;

public class DiscoveryService : IAsyncDisposable
{
    private IFtpClient? _ftpClient;

    public async Task<Uri?> DownloadAppDb(string consoleIp)
    {
        _ftpClient = await FtpConnectionFactory.OpenConnection(consoleIp);
        var status = await _ftpClient.DownloadFileAsync($"{ClientConfig.TempDirectory.LocalPath}/{Constants.AppDbFileName}", Constants.MmsFolderPath + Constants.AppDbFileName);

        if (status == FtpStatus.Success)
        {
            return new Uri($"{ClientConfig.TempDirectory.LocalPath}/{Constants.AppDbFileName}");
        }

        return null;
    }

    public async Task<FtpStatus> UploadAppDb(Uri appDbPath)
    {
        if (string.IsNullOrWhiteSpace(appDbPath.LocalPath))
        {
            Console.WriteLine("Did NOT find local app.db");
            return FtpStatus.Skipped;
        }

        using var stream = new FileStream(appDbPath.LocalPath, FileMode.Open);
        return await _ftpClient?.UploadFileAsync(appDbPath.LocalPath, Constants.MmsFolderPath + Constants.AppDbFileName, FtpRemoteExists.Overwrite);
    }

    public async Task<IEnumerable<ContentSizeDto>> CalculateTitleSize(IEnumerable<AppTitle> titles)
    {
        var result = new List<ContentSizeDto>();
        foreach (var title in titles)
        {
            var titleId = title.TitleId;

            var appSize = await CalculateContentSize(ContentType.App, title);
            var patchSize = await CalculateContentSize(ContentType.Patch, title);
            var dlcSize = await CalculateContentSize(ContentType.AddCont, title);

            if (appSize is not null)
            {
                var totalSize = appSize.TotalSizeInBytes + (patchSize?.TotalSizeInBytes ?? 0) + (dlcSize?.TotalSizeInBytes ?? 0);
                result.Add(new ContentSizeDto(titleId, totalSize));
            }
        }

        return result;
    }

    private async Task<ContentSizeDto?> CalculateContentSize(ContentType contentType, AppTitle title)
    {
        if (contentType == ContentType.AddCont)
        {
            return await CalculateDlcContentSize(title);
        }

        var contentTypeStr = contentType.ToString().ToLower();
        var contentDataPath = $"/user/{contentTypeStr}/{title.TitleId}/{contentTypeStr}.pkg";

        if (title.ExternalStorage)
        {
            contentDataPath = Constants.ExternalDriveMountPoint0 + contentDataPath;
        }

        var pkgInfo = await _ftpClient?.GetObjectInfoAsync(contentDataPath);
        return (pkgInfo is null || pkgInfo.Size == 0) ? null : new ContentSizeDto(title.TitleId, pkgInfo.Size);
    }

    private async Task<ContentSizeDto?> CalculateDlcContentSize(AppTitle title)
    {
        var dlcDataPath = $"/user/addcont/{title.TitleId}/";
        if (title.ExternalStorage)
        {
            dlcDataPath = Constants.ExternalDriveMountPoint0 + dlcDataPath;
        }

        var exists = await _ftpClient?.DirectoryExistsAsync(dlcDataPath);
        if (!exists)
        {
            return null;
        }

        var listing = await _ftpClient?.GetListingAsync(dlcDataPath, FtpListOption.Recursive);
        var contentPkgs = listing.Where(x => x.Name.Contains("ac.pkg"));
        var dlcsTotalSize = contentPkgs.Sum(x => x.Size);

        return new ContentSizeDto(title.TitleId, dlcsTotalSize);
    }

    public async ValueTask DisposeAsync()
    {
        if (_ftpClient is not null)
        {
            await _ftpClient.DisconnectAsync();
        }

        GC.SuppressFinalize(this);
    }
}