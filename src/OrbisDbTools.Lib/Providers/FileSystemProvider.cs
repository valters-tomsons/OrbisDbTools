using OrbisDbTools.PS4.Models;
using OrbisDbTools.PS4.Constants;
using OrbisDbTools.Utils;
using OrbisDbTools.PS4.Enums;
using OrbisDbTools.Lib.Abstractions;
using OrbisDbTools.Utils.Extensions;

namespace OrbisDbTools.Lib.Providers;

public class FileSystemProvider
{
    private readonly OrbisFtp _ftpClient;

    public FileSystemProvider(OrbisFtp ftpClient)
    {
        _ftpClient = ftpClient;
    }

    public async Task<bool> UploadAppDb(Uri localPath)
    {
        const string remotePath = OrbisSystemPaths.MmsFolderPath + OrbisSystemPaths.AppDbFileName;
        var uploadSuccess = await _ftpClient.UploadFile(localPath.LocalPath, remotePath);
        return uploadSuccess == FluentFTP.FtpStatus.Success;
    }

    public async Task<Uri?> DownloadAppDb(CancellationToken cts = default)
    {
        var localPath = new Uri($"{ClientConfig.TempDirectory.LocalPath}/{OrbisSystemPaths.AppDbFileName}");
        const string remotePath = OrbisSystemPaths.MmsFolderPath + OrbisSystemPaths.AppDbFileName;

        var downloadSuccess = await _ftpClient.DownloadFile(localPath, remotePath, cts);

        return downloadSuccess
            ? localPath
            : null;
    }

    public async Task<Uri?> DownloadAddContDb(CancellationToken cts = default)
    {
        var localPath = new Uri($"{ClientConfig.TempDirectory.LocalPath}/{OrbisSystemPaths.AddContDbFileName}");
        const string remotePath = OrbisSystemPaths.MmsFolderPath + OrbisSystemPaths.AddContDbFileName;

        var downloadSuccess = await _ftpClient.DownloadFile(localPath, remotePath, cts);

        return downloadSuccess
            ? localPath
            : null;
    }

    public async Task<IReadOnlyCollection<FsTitle>> ScanFileSystemTitles()
    {
        var results = new List<FsTitle>();

        foreach (var dir in new[] { OrbisSystemPaths.UserAppPath, OrbisSystemPaths.UserExternalAppPath })
        {
            var listingResult = await _ftpClient.ListFilesAndSizes(dir, false);
            var fileList = listingResult?.Select(x => x.Key).ToList();

            if (fileList is null)
            {
                continue;
            }

            var titles = ParseFileList(fileList)!;
            results.AddRange(titles);
        }

        return results;
    }

    private static IReadOnlyCollection<FsTitle> ParseFileList(IReadOnlyCollection<string> contentPaths)
    {
        var results = new List<FsTitle>(contentPaths.Count);

        foreach (var path in contentPaths)
        {
            var titleId = path.GetTitleIdFromGamePkgPath();
            if (titleId is null) continue;
            var fsTitle = new FsTitle(titleId, path);
            results.Add(fsTitle);
        }

        return results;
    }

    public async Task<IReadOnlyCollection<Uri>> DownloadTitleSfos(IReadOnlyCollection<FsTitle> titles)
    {
        var results = new List<Uri>(titles.Count);

        foreach (var title in titles)
        {
            var localPath = new Uri($"{ClientConfig.TempDirectory.LocalPath}/sfo/{title.TitleId}.sfo");

            var appMetaPath = title.ExternalStorage ? OrbisSystemPaths.AppMetaExternalFolderPath : OrbisSystemPaths.AppMetaFolderPath;
            var remoteSfoPath = $"{appMetaPath}{title.TitleId}/{OrbisSystemPaths.SfoFileName}";

            var downloadSuccess = await _ftpClient.DownloadFile(localPath, remoteSfoPath);

            if (downloadSuccess)
            {
                results.Add(localPath);
            }
        }

        return results;
    }

    public async Task<ContentSizeDto> CalculateAppSize(AppInfoDto appInfo)
    {
        var fakeTitle = new AppTitle(appInfo.TitleId, appInfo.Title)
        {
            MetaDataPath = appInfo.MetaDataPath
        };

        var result = await CalculateTitleSizes(new List<AppTitle>(1) { fakeTitle });
        return result.FirstOrDefault() ?? new ContentSizeDto(appInfo.TitleId, 0);
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

        var contentFileSize = await _ftpClient.FileSizeInBytes(contentDataPath);

        return contentFileSize.GetValueOrDefault() > 1
            ? new ContentSizeDto(title.TitleId, contentFileSize.Value)
            : null;
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

        return contentPkgs?.Any() == true
            ? new ContentSizeDto(title.TitleId, contentPkgs.Sum(x => x.Value))
            : null;
    }
}