using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using OrbisDbTools.PS4.AppDb;
using OrbisDbTools.PS4.Discovery;
using OrbisDbTools.Utils;
using OrbisDbTools.PS4;
using ReactiveUI;

namespace OrbisDbTools.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> DownloadDb { get; }
    public ReactiveCommand<Unit, Unit> RecalculateDbContent { get; }
    public ReactiveCommand<Unit, Unit> AllowDeleteApps { get; }
    public ReactiveCommand<Unit, Unit> HidePsnApps { get; }
    public ReactiveCommand<Unit, Unit> ForceDc { get; }

    private readonly AppDbProvider _dbProvider;
    private readonly DiscoveryService _discovery;
    private Uri? _localAppDb;

    public bool EnableDbActions { get => enabledbactions; set => this.RaiseAndSetIfChanged(ref enabledbactions, value); }
    private bool enabledbactions;

    public MainWindowViewModel()
    {
        _dbProvider = new AppDbProvider();
        _discovery = new DiscoveryService();

        DownloadDb = ReactiveCommand.CreateFromTask(DownloadDatabase);
        RecalculateDbContent = ReactiveCommand.CreateFromTask(RecalculateContent);
        AllowDeleteApps = ReactiveCommand.CreateFromTask(MarkCanRemoveInstalled);
        HidePsnApps = ReactiveCommand.CreateFromTask(HidePSNApps);
        ForceDc = ReactiveCommand.CreateFromTask(ForceDisconnect);
    }

    async Task ForceDisconnect()
    {
        await _dbProvider.DisposeAsync();
        await _discovery.DisposeAsync();
        Console.WriteLine("Finished disconnect");
    }

    async Task DownloadDatabase()
    {
        _localAppDb = await _discovery.DownloadAppDb();
        if (_localAppDb is not null)
        {
            File.Copy(_localAppDb.LocalPath, $"{ClientConfig.TempDirectory.LocalPath}/app.db.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
            EnableDbActions = await _dbProvider.OpenDatabase(_localAppDb.LocalPath);
        }
    }

    async Task HidePSNApps()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetAllTitles(appTable);

            var knownPsnIds = KnownContent.KnownPsnApps.Select(x => x.TitleId);
            var installedPsnApps = installedTitles.Where(x => knownPsnIds.Contains(x.TitleId));

            var hidden = await _dbProvider.HideTitles(appTable, installedPsnApps);
            Console.WriteLine($"Hidden {hidden} apps in {appTable}");
        }
    }

    async Task RecalculateContent()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetInstalledTitles(appTable);

            var titleSizes = await _discovery.CalculateTitleSize(installedTitles);

            var updatedSizes = await _dbProvider.UpdateTitleSizes(appTable, titleSizes);
            Console.WriteLine($"Updated content size for {updatedSizes} titles in {appTable}");
        }
    }

    async Task MarkCanRemoveInstalled()
    {
        var userAppTables = await _dbProvider.GetAppTables();

        foreach (var appTable in userAppTables)
        {
            var installedTitles = await _dbProvider.GetInstalledTitles(appTable);
            var allowedDelete = await _dbProvider.EnableTitleDeletion(appTable, installedTitles);
            Console.WriteLine($"Enabled deletion for {allowedDelete} apps in {appTable}");
        }
    }
}