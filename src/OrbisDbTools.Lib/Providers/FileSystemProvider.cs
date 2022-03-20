using OrbisDbTools.PS4.Models;
using OrbisDbTools.PS4.Constants;
using OrbisDbTools.Utils;
using OrbisDbTools.PS4.Enums;
using OrbisDbTools.Lib.Abstractions;

namespace OrbisDbTools.Lib.Providers;

public class FileSystemProvider
{
    private readonly OrbisFtp _ftpClient;

    public FileSystemProvider(OrbisFtp ftpClient)
    {
        _ftpClient = ftpClient;
    }

    public async Task<Uri?> DownloadAppDb()
    {
        var localPath = new Uri($"{ClientConfig.TempDirectory.LocalPath}/{OrbisSystemPaths.AppDbFileName}");
        const string remotePath = OrbisSystemPaths.MmsFolderPath + OrbisSystemPaths.AppDbFileName;

        var status = await _ftpClient.DownloadFile(localPath, remotePath);

        if (status)
        {
            return new Uri($"{ClientConfig.TempDirectory.LocalPath}/{OrbisSystemPaths.AppDbFileName}");
        }

        return null;
    }

    public async Task<IEnumerable<ContentSizeDto>> CalculateTitleSizes(IEnumerable<AppTitle> titles)
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
                var totalSize = appSize.TotalSizeInBytes +
                                    (patchSize?.TotalSizeInBytes ?? 0) +
                                    (dlcSize?.TotalSizeInBytes ?? 0);

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
            contentDataPath = OrbisSystemPaths.ExternalDriveMountPoint0 + contentDataPath;
        }

        var fileSize = await _ftpClient.FileSizeInBytes(contentDataPath);
        if (!fileSize.HasValue || fileSize == 0)
        {
            return null;
        }

        return new ContentSizeDto(title.TitleId, fileSize.Value);
    }

    private async Task<ContentSizeDto?> CalculateDlcContentSize(AppTitle title)
    {
        var dlcDataPath = $"/user/addcont/{title.TitleId}/";
        if (title.ExternalStorage)
        {
            dlcDataPath = OrbisSystemPaths.ExternalDriveMountPoint0 + dlcDataPath;
        }

        var listing = await _ftpClient.ListFilesAndSizes(dlcDataPath, true);
        var contentPkgs = listing?.Where(x => x.Key.EndsWith("ac.pkg"));

        if (contentPkgs?.Any() != true)
        {
            return null;
        }

        var dlcsTotalSize = contentPkgs.Sum(x => x.Value);
        return new ContentSizeDto(title.TitleId, dlcsTotalSize);
    }
}