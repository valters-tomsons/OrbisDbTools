using System.Data;
using System.Net;
using LibOrbisPkg.SFO;
using OrbisDbTools.Lib.Abstractions;
using OrbisDbTools.Lib.Providers;
using OrbisDbTools.PS4;
using OrbisDbTools.PS4.Constants;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils;
using OrbisDbTools.Utils.Extensions;

namespace OrbisDbTools.Lib.Controllers;

public class MainWindowController
{
    private readonly OrbisFtp _ftp;
    private readonly AppDbProvider _dbProvider;
    private readonly FileSystemProvider _discovery;
    private readonly GameDataProvider _sfoReader;

    private Uri? _localAppDb;

    public MainWindowController(FileSystemProvider discoveryService, AppDbProvider dbProvider, OrbisFtp ftp, GameDataProvider sfoReader)
    {
        _discovery = discoveryService;
        _ftp = ftp;
        _dbProvider = dbProvider;
        _sfoReader = sfoReader;
    }

    public async Task<bool> PromptAndOpenLocalDatabase(Func<Task<Uri>> fileDialogPromptFunc)
    {
        _localAppDb = await fileDialogPromptFunc().ConfigureAwait(true);
        if (_localAppDb is not null)
        {
            var fileDirectory = Path.GetDirectoryName(_localAppDb.LocalPath);
            var fileName = Path.GetFileName(_localAppDb.LocalPath);

            File.Copy(_localAppDb.LocalPath, $"{fileDirectory}/{fileName}.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
            return await _dbProvider.OpenDatabase(_localAppDb.LocalPath).ConfigureAwait(true);
        }

        return false;
    }

    public async Task<bool> ConnectAndDownload(string consoleIp)
    {
        if (!IPAddress.TryParse(consoleIp, out var _))
        {
            throw new Exception("Not a valid IP address");
        }

        if (!string.IsNullOrWhiteSpace(consoleIp))
        {
            var connected = await _ftp.OpenConnection(consoleIp);

            if (!connected)
            {
                return false;
            }

            _localAppDb = await _discovery.DownloadAppDb();
            if (_localAppDb is not null)
            {
                File.Copy(_localAppDb.LocalPath, $"{ClientConfig.TempDirectory.LocalPath}/app.db.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
                return await _dbProvider.OpenDatabase(_localAppDb.LocalPath);
            }
        }

        return false;
    }

    public async Task DisconnectRemoteAndPromptSave(Func<Task<Uri>> fileDialogAction)
    {
        await _ftp.DisposeAsync();
        await _dbProvider.DisposeAsync();

        var targetPath = await fileDialogAction().ConfigureAwait(true);
        if (targetPath is not null)
        {
            File.Copy($"{ClientConfig.TempDirectory.LocalPath}/app.db", targetPath.LocalPath, true);
        }
    }

    public async Task CloseLocalDb()
    {
        await _dbProvider.DisposeAsync();
        Console.WriteLine("Finished disconnect");
    }

    public async Task<IList<AppTitle>> QueryInstalledApps()
    {
        var appTables = await _dbProvider.GetAppTables();

        if (!appTables.Any())
        {
            return Array.Empty<AppTitle>();
        }

        var titles = await _dbProvider
            .GetInstalledTitles(appTables.First());

        if (titles is null)
        {
            return Array.Empty<AppTitle>();
        }

        return titles.ToList();
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

    public async Task FixMissingAppTitles()
    {
        var userAppTables = await _dbProvider.GetAppTables();
        var installedTitles = await _dbProvider.GetAllTitles(userAppTables.First());

        var titlesOnFilesystem = await _discovery.ScanFileSystemTitles();
        var missingTitles = titlesOnFilesystem.Where(x => !installedTitles.Any(y => y.TitleId == x.TitleId)).ToList();
        var localSfoPaths = await _discovery.DownloadTitleSfos(missingTitles);

        var parseSfoTasks = localSfoPaths.Select(async x => await _sfoReader.ReadSfo(x));
        var parseSfoResults = await Task.WhenAll(parseSfoTasks);

        missingTitles.ForEach(x => x.SFO = Array.Find(parseSfoResults, y => (y!["TITLE_ID"] as Utf8Value)!.Value == x.TitleId));

        var appBrowseRows = new List<AppBrowseTblRow>(missingTitles.Count);
        var appInfoRows = new List<AppInfoTblRow>(missingTitles.Count * 40);

        foreach (var missingFsTitle in missingTitles.Where(x => x.SFO != null))
        {
            var appMetaPath = missingFsTitle.ExternalStorage
                ? OrbisSystemPaths.UserExternalAppMetadata + missingFsTitle.TitleId
                : OrbisSystemPaths.UserAppMetadataPath + missingFsTitle.TitleId;

            var appRow = new AppBrowseTblRow()
            {
                titleId = (missingFsTitle.SFO!["TITLE_ID"] as Utf8Value)!.Value,
                contentId = (missingFsTitle.SFO!["CONTENT_ID"] as Utf8Value)!.Value,
                titleName = (missingFsTitle.SFO!["TITLE"] as Utf8Value)!.Value,
                metaDataPath = appMetaPath,
                lastAccessTime = DateTime.UtcNow.ToOrbisDateTime(),
                contentSize = 100000,
                installDate = DateTime.UtcNow.ToOrbisDateTime()
            };

            appBrowseRows.Add(appRow);
            appInfoRows.AddRange(GenerateInfoRows(missingFsTitle));
        }

        await _dbProvider.InsertAppBrowseRows(userAppTables.First(), appBrowseRows);
        await _dbProvider.InsertAppInfoRows(appInfoRows);
    }

    private IReadOnlyCollection<AppInfoTblRow> GenerateInfoRows(FsTitle title)
    {
        var appPath = title.ExternalStorage
            ? OrbisSystemPaths.UserExternalAppPath + title.TitleId
            : OrbisSystemPaths.UserAppPath + title.TitleId;

        var metaDataPath = title.ExternalStorage
            ? OrbisSystemPaths.UserExternalAppMetadata + title.TitleId
            : OrbisSystemPaths.UserAppMetadataPath + title.TitleId;

        var appType = (title.SFO!["APP_TYPE"] as IntegerValue)!.Value;
        var appVer = (title.SFO!["APP_VER"] as Utf8Value)!.Value;
        var attribute = (title.SFO!["ATTRIBUTE"] as IntegerValue)!.Value;
        var category = (title.SFO!["CATEGORY"] as Utf8Value)!.Value;
        var contentId = (title.SFO!["CONTENT_ID"] as Utf8Value)!.Value;
        var titleName = (title.SFO!["TITLE"] as Utf8Value)!.Value;
        var version = (title.SFO!["VERSION"] as Utf8Value)!.Value;

        return new List<AppInfoTblRow>
        {
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_access_index", Val = "67" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_last_access_time", Val = DateTime.UtcNow.ToOrbisDateTime() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_contents_status", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_mtime", Val = DateTime.UtcNow.ToOrbisDateTime() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_update_index", Val = "74" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#exit_type", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "ATTRIBUTE_INTERNAL", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DISP_LOCATION_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DISP_LOCATION_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DOWNLOAD_DATA_SIZE", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "FORMAT", Val = "obs" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "PARENTAL_LEVEL", Val = "1" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_3", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_4", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_5", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_6", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_7", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SYSTEM_VER", Val = "33751040" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_3", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_4", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_contents_ext_type", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_contents_location", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_current_slot", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_disable_live_detail", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_hdd_location", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_path_info", Val = "3113537756987392" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_path_info_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_size_other_hdd", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_sort_priority", Val = "100" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_uninstallable", Val = "1" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_view_category", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_working_status", Val = "0" },

            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_size", Val = "100000" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_org_path", Val = appPath },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_metadata_path", Val = metaDataPath},

            new AppInfoTblRow() { TitleId = title.TitleId, Key = "APP_TYPE", Val = appType.ToString()},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "APP_VER", Val = appVer},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "ATTRIBUTE", Val = attribute.ToString()},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "CATEGORY", Val = category},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "CONTENT_ID", Val = contentId},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "TITLE", Val = titleName},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "TITLE_ID", Val = title.TitleId},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "VERSION", Val = version},
        };
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
}