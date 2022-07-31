using System.Data;
using System.Net;
using LibOrbisPkg.PKG;
using LibOrbisPkg.SFO;
using OrbisDbTools.Lib.Abstractions;
using OrbisDbTools.Lib.Helpers;
using OrbisDbTools.Lib.Providers;
using OrbisDbTools.PS4;
using OrbisDbTools.PS4.Constants;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils;

namespace OrbisDbTools.Lib.Controllers;

public class MainWindowController
{
    private readonly OrbisFtp _ftp;
    private readonly AppDbProvider _dbProvider;
    private readonly AddContDbProvider _dlcProvider;
    private readonly FileSystemProvider _discovery;
    private readonly GameDataProvider _sfoReader;

    public MainWindowController(FileSystemProvider discoveryService, AppDbProvider dbProvider, OrbisFtp ftp, GameDataProvider sfoReader, AddContDbProvider dlcProvider)
    {
        _discovery = discoveryService;
        _ftp = ftp;
        _dbProvider = dbProvider;
        _sfoReader = sfoReader;
        _dlcProvider = dlcProvider;
    }

    public async Task<bool> PromptAndOpenLocalDatabase(Func<Task<Uri>> fileDialogPromptFunc)
    {
        var localAppDb = await fileDialogPromptFunc().ConfigureAwait(true);
        if (localAppDb is not null)
        {
            var fileDirectory = Path.GetDirectoryName(localAppDb.LocalPath);
            var fileName = Path.GetFileName(localAppDb.LocalPath);

            File.Copy(localAppDb.LocalPath, $"{fileDirectory}/{fileName}.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
            return await _dbProvider.OpenDatabase(localAppDb.LocalPath).ConfigureAwait(true);
        }

        return false;
    }

    public async Task<bool> ConnectAndDownload(string consoleIp, CancellationToken cts = default)
    {
        if (string.IsNullOrWhiteSpace(consoleIp))
        {
            return false;
        }

        var parsed = IPEndPoint.TryParse(consoleIp, out var endpoint);
        if (!parsed || endpoint is null)
        {
            throw new Exception("Not a valid IP address");
        }

        var connected = await _ftp.OpenConnection(endpoint, cts);
        if (!connected)
        {
            return false;
        }

        var localAppDb = await _discovery.DownloadAppDb(cts);
        if (localAppDb is null)
        {
            await _ftp.DisposeAsync();
            throw new Exception("Failed to download app.db, cannot continue");
        }

        var localAddContDb = await _discovery.DownloadAddContDb(cts);
        if (localAddContDb is null)
        {
            await _ftp.DisposeAsync();
            throw new Exception("Failed to download addcont.db, cannot continue");
        }

        File.Copy(localAppDb.LocalPath, $"{ClientConfig.TempDirectory.LocalPath}/app.db.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
        File.Copy(localAddContDb.LocalPath, $"{ClientConfig.TempDirectory.LocalPath}/addcont.db.{DateTimeOffset.Now.ToUnixTimeSeconds()}");

        var appDbConnected = await _dbProvider.OpenDatabase(localAppDb.LocalPath, cts);
        var addContConnected = await _dlcProvider.OpenDatabase(localAddContDb.LocalPath, cts);

        return appDbConnected && addContConnected;
    }

    public async Task DisconnectRemoteAndPromptSave(Func<Task<Uri>> fileDialogAction)
    {
        await _ftp.DisposeAsync();
        await _dbProvider.DisposeAsync();
        await _dlcProvider.DisposeAsync();

        var targetPath = await fileDialogAction().ConfigureAwait(true);
        if (targetPath is not null)
        {
            File.Copy($"{ClientConfig.TempDirectory.LocalPath}/app.db", targetPath.LocalPath, true);
        }
    }

    public async Task CloseLocalDb()
    {
        await _dbProvider.DisposeAsync();
        await _dlcProvider.DisposeAsync();
        Console.WriteLine("Finished disconnect");
    }

    public async Task<IList<AppTitle>> QueryInstalledApps()
    {
        var appTables = await _dbProvider.GetAppTables();

        if (!appTables.Any())
        {
            return Array.Empty<AppTitle>();
        }

        var titles = await _dbProvider.GetInstalledTitles(appTables.First());

        if (titles is null)
        {
            return Array.Empty<AppTitle>();
        }

        return titles.ToList();
    }

    public async Task<IEnumerable<AddContTblRow>> QueryInstalledDlc()
    {
        return await _dlcProvider.GetInstalledContent();
    }

    public async Task<int> HideAllKnownPsnApps()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        var count = 0;

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetAllTitles(appTable);

            var knownPsnIds = KnownContent.KnownPsnApps.Select(x => x.TitleId);
            var installedPsnApps = installedTitles.Where(x => knownPsnIds.Contains(x.TitleId));

            var hidden = await _dbProvider.HideTitles(appTable, installedPsnApps);
            Console.WriteLine($"Hidden {hidden} apps in {appTable}");
            count += hidden;
        }

        return count;
    }

    public async Task<IReadOnlyCollection<FsTitle>> FixMissingAppTitles()
    {
        var userAppTables = await _dbProvider.GetAppTables();
        var installedTitles = await _dbProvider.GetAllTitles(userAppTables.First());

        var titlesOnFilesystem = await _discovery.ScanFileSystemTitles();

        var missingTitles = titlesOnFilesystem.Where(x => !installedTitles.Any(y => y.TitleId == x.TitleId)).ToList();

        // Filter out titles from external storage for now
        missingTitles = missingTitles.Where(x => !x.ExternalStorage).DistinctBy(x => x.TitleId).ToList();

        var localSfoPaths = await _discovery.DownloadTitleSfos(missingTitles);

        var parseSfoTasks = localSfoPaths.Select(async x => await _sfoReader.ParseLocalSfo(x));
        var parseSfoResults = await Task.WhenAll(parseSfoTasks);

        missingTitles.ForEach(x => x.SFO = Array.Find(parseSfoResults, y => (y!["TITLE_ID"] as Utf8Value)!.Value == x.TitleId));

        var appBrowseRows = new List<AppBrowseTblRow>(missingTitles.Count);
        var appInfoRows = new List<AppInfoTblRow>(missingTitles.Count * 40);

        var missingTitlesWithSfo = missingTitles.Where(x => x.SFO != null).ToList();
        var externalHddId = await _dbProvider.GetExternalHddId();

        foreach (var missingFsTitle in missingTitlesWithSfo)
        {
            Console.WriteLine($"Forging entries for: {missingFsTitle.TitleId}");

            var appInfo = new AppInfoDto(missingFsTitle, externalHddId);
            appInfo.ContentSize = (await _discovery.CalculateAppSize(appInfo)).TotalSizeInBytes;

            var browseRow = DbRowGenerator.GenerateAppBrowseRow(appInfo);
            var infoRows = DbRowGenerator.GenerateAppInfoRows(missingFsTitle, appInfo);

            appBrowseRows.Add(browseRow);
            appInfoRows.AddRange(infoRows);

            Console.WriteLine($"Game info parsed: {browseRow.titleName}");
        }

        foreach (var table in userAppTables)
        {
            var appRowsInserted = await _dbProvider.InsertAppBrowseRows(table, appBrowseRows);
            Console.WriteLine($"Added {appRowsInserted} new apps to {table}");
        }

        var infoRowsInserted = await _dbProvider.InsertAppInfoRows(appInfoRows);
        Console.WriteLine($"Created {infoRowsInserted} new app_info entries");

        return missingTitlesWithSfo?.Where(x => appBrowseRows.Any(y => y.titleId.Equals(x.TitleId))).ToList() ?? new List<FsTitle>();
    }

    public async Task RebuildAddCont()
    {
        var userAppTables = await _dbProvider.GetAppTables();
        var installedTitles = await _dbProvider.GetInstalledTitles(userAppTables.First());

        var installedDlc = await _dlcProvider.GetInstalledContent();

        var newDlcInfo = new List<DlcPkgDataDto>();

        foreach (var title in installedTitles)
        {
            var dlcDataPath = $"/user/addcont/{title.TitleId}/";
            if (title.ExternalStorage)
            {
                dlcDataPath = OrbisSystemPaths.ExternalDriveMountPoint0 + dlcDataPath;
            }

            Console.WriteLine($"Enumerating DLC files for {title.TitleId} in '{dlcDataPath}'");

            var dlcFiles = await _ftp.ListTitleDlcFiles(dlcDataPath);

            // Filter out already installed DLC
            var ignoredFiles = dlcFiles.Where(x => installedDlc.Any(y => x.Contains($"{y.title_id}/{y.dir_name}")));
            dlcFiles = dlcFiles.Except(ignoredFiles).ToList();

            Console.WriteLine($"{dlcFiles.Count} new DLC files found for {title.TitleId}");
            if (dlcFiles.Count == 0)
            {
                continue;
            }

            var titleDlcInfo = new List<DlcPkgDataDto>();
            foreach (var dlcPkg in dlcFiles)
            {
                using var pkgStream = await _ftp.OpenFileStream(dlcPkg);

                if (pkgStream is null)
                {
                    Console.WriteLine($"Failed to open pkg stream: {dlcPkg}");
                    continue;
                }

                var pkgInfo = await _sfoReader.GetDlcPkgData(pkgStream);
                if (pkgInfo.header is null)
                {
                    Console.WriteLine($"Failed to read pkg header: {dlcPkg}");
                    continue;
                }

                pkgStream.Close();
                await pkgStream.DisposeAsync();
                await Task.Delay(500);

                var dlcData = new DlcPkgDataDto(title.TitleId, dlcPkg, pkgInfo.header.Value, pkgInfo.sfo);

                if (string.IsNullOrWhiteSpace(dlcData.DirName))
                {
                    Console.WriteLine($"Not valid dlc data, skipping: {dlcPkg}");
                    continue;
                }

                titleDlcInfo.Add(dlcData);
            }

            newDlcInfo.AddRange(titleDlcInfo);
        }

        await _dlcProvider.InsertAddContDlcItems(newDlcInfo);
    }

    public async Task<int> ReCalculateInstalledAppSizes()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        var count = 0;

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetInstalledTitles(appTable);
            var titleSizes = await _discovery.CalculateTitleSizes(installedTitles);

            var updatedSizes = await _dbProvider.UpdateTitleSizes(appTable, titleSizes);

            Console.WriteLine($"Updated content size for {updatedSizes} titles in {appTable}");
            count += updatedSizes;
        }

        return count;
    }

    public async Task<int> AllowDeleteInstalledApps()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        var count = 0;

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetInstalledTitles(appTable);
            var allowedDelete = await _dbProvider.EnableTitleDeletion(appTable, installedTitles);
            Console.WriteLine($"Enabled deletion for {allowedDelete} apps in {appTable}");
            count += allowedDelete;
        }

        return count;
    }

    public async Task UpdateEditedApp(AppTitle app)
    {
        var userAppTables = await _dbProvider.GetAppTables();

        foreach (var appTable in userAppTables)
        {
            try
            {
                await _dbProvider.WriteTitleChanges(appTable, app);
                Console.WriteLine($"Changes written for {app.TitleId} in '{appTable}'");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to update {app.TitleId} in '{appTable}':");
                Console.WriteLine(e.Message);
            }
        }
    }
}