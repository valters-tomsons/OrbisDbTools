using System.Net;
using OrbisDbTools.Lib.Abstractions;
using OrbisDbTools.Lib.Providers;
using OrbisDbTools.PS4;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils;

namespace OrbisDbTools.Lib.Controllers;

public class MainWindowController
{
    private readonly OrbisFtp _ftp;
    private readonly AppDbProvider _dbProvider;
    private readonly OrbisFileSystemProvider _discovery;

    private Uri? _localAppDb;

    public MainWindowController(OrbisFileSystemProvider discoveryService, AppDbProvider dbProvider, OrbisFtp ftp)
    {
        _discovery = discoveryService;
        _ftp = ftp;
        _dbProvider = dbProvider;
    }

    public async Task<bool> PrompAndOpenLocalDatabase(Func<Task<Uri>> fileDialogPromptFunc)
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

    public async Task<bool> DownloadAndConnect(string consoleIp)
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