using System;
using System.Reactive;
using System.Threading.Tasks;
using OrbisDbTools.Lib.Controllers;
using OrbisDbTools.PS4.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace OrbisDbTools.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ConnectDb { get; }
    public ReactiveCommand<Unit, Unit> BrowseDb { get; }

    public ReactiveCommand<Unit, Unit> AddMissingTitles { get; }
    public ReactiveCommand<Unit, Unit> RecalculateDbContent { get; }
    public ReactiveCommand<Unit, Unit> AllowDeleteApps { get; }
    public ReactiveCommand<Unit, Unit> HidePsnApps { get; }
    public ReactiveCommand<Unit, Unit> ForceDc { get; }

    public EventHandler<DataGridCellEditEndedEventArgs> CellEditEnded { get; }

    private readonly MainWindowController _controller;

    public bool DbConnected { get => dbConnected; set => this.RaiseAndSetIfChanged(ref dbConnected, value); }
    private bool dbConnected;

    public bool IsLocalDb { get => isLocalDb; set => this.RaiseAndSetIfChanged(ref isLocalDb, value); }
    private bool isLocalDb;

    public string ConsoleIpAddress { get => consoleIpAddress; set => this.RaiseAndSetIfChanged(ref consoleIpAddress, value); }
    private string consoleIpAddress = string.Empty;

    public bool ShowProgressBar { get => showProgressBar; set => this.RaiseAndSetIfChanged(ref showProgressBar, value); }
    private bool showProgressBar;

    public string ProgressText { get => progressText; set => this.RaiseAndSetIfChanged(ref progressText, value); }
    private string progressText = string.Empty;

    public string ConnectionError { get => connectionError; set => this.RaiseAndSetIfChanged(ref connectionError, value); }
    private string connectionError = string.Empty;

    public ObservableCollection<AppTitle> DbItems { get => dbItems; set => this.RaiseAndSetIfChanged(ref dbItems, value); }
    private ObservableCollection<AppTitle> dbItems = new();

    public Func<Task<Uri?>> OpenLocalDbDialogAction = null!;
    public Func<Task<Uri?>> SaveDbLocallyDialogAction = null!;
    public Func<string, Task<bool>> ShowWarningDialogAction = null!;

    public MainWindowViewModel(MainWindowController controller)
    {
        _controller = controller;

        ConnectDb = ReactiveCommand.CreateFromTask(DownloadDatabase);
        RecalculateDbContent = ReactiveCommand.CreateFromTask(RecalculateContent);
        AllowDeleteApps = ReactiveCommand.CreateFromTask(MarkCanRemoveInstalled);
        HidePsnApps = ReactiveCommand.CreateFromTask(HidePSNApps);
        ForceDc = ReactiveCommand.CreateFromTask(ForceDisconnect);
        BrowseDb = ReactiveCommand.CreateFromTask(BrowseLocalDatabase);
        AddMissingTitles = ReactiveCommand.CreateFromTask(FixDatabase);

        CellEditEnded += OnCellEditEnded;
    }

    private async void OnCellEditEnded(object? _, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (e.Row.DataContext is not AppTitle editedApp) return;

        await _controller.UpdateEditedApp(editedApp);
    }

    async Task FixDatabase()
    {
        var warningAccepted = await ShowWarningDialogAction(
@"This action will populate database with applications that are installed on your internal drive, but missing from main system menu.
Might take a while, depending on your storage size.
Application from extended storage will not be added.

This only works on FW >= 6.72!

Continue?"
        );

        if (!warningAccepted) return;

        ShowSpinner("Rebuilding missing app entries...");
        _ = await _controller.FixMissingAppTitles();
        await UpdateDbViewItems();
        ShowProgressBar = false;
    }

    async Task BrowseLocalDatabase()
    {
        try
        {
            DbConnected = await _controller.PromptAndOpenLocalDatabase(OpenLocalDbDialogAction!).ConfigureAwait(true);
            if (DbConnected)
            {
                await UpdateDbViewItems();
                IsLocalDb = true;
            }
        }
        catch (Exception e)
        {
            ConnectionError = $"Failed to connect: {e.Message}";
        }
    }

    async Task UpdateDbViewItems()
    {
        var items = await _controller.QueryInstalledApps();
        DbItems = new(items);
    }

    async Task ForceDisconnect()
    {
        ShowSpinner("Disconnecting, please wait...");

        if (IsLocalDb)
        {
            await _controller.CloseLocalDb();
        }
        else
        {
            await _controller.DisconnectRemoteAndPromptSave(SaveDbLocallyDialogAction!).ConfigureAwait(true);
        }

        DbConnected = false;
        ShowProgressBar = false;
    }

    async Task DownloadDatabase()
    {
        ShowSpinner("Connecting, please wait...");

        try
        {
            DbConnected = await _controller.ConnectAndDownload(consoleIpAddress).ConfigureAwait(false);
            await UpdateDbViewItems();
            IsLocalDb = false;
        }
        catch (Exception e)
        {
            ConnectionError = $"Failed to connect: {e.Message}";
        }

        HideSpinner();
    }

    async Task HidePSNApps()
    {
        ShowSpinner("Looking for PSN apps, please wait...");
        await _controller.HideAllKnownPsnApps();
        await UpdateDbViewItems();
        HideSpinner();
    }

    async Task RecalculateContent()
    {
        var warningAccepted = await ShowWarningDialogAction(
@"This action will update database with appropriate installation sizes, changes will only be visible in system storage settings menu.
Might take a while, depending on your storage size.

Continue?"
            );
        if (!warningAccepted) return;

        ShowSpinner("Calculating, please wait...");
        await _controller.ReCalculateInstalledAppSizes();
        await UpdateDbViewItems();
        HideSpinner();
    }

    async Task MarkCanRemoveInstalled()
    {
        ShowSpinner("Allowing deletion, please wait...");
        await _controller.AllowDeleteInstalledApps();
        await UpdateDbViewItems();
        HideSpinner();
    }

    private void ShowSpinner(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ProgressText = "Please wait...";
        }

        ProgressText = text;
        ConnectionError = string.Empty;
        ShowProgressBar = true;
    }

    private void HideSpinner()
    {
        ShowProgressBar = false;
    }
}