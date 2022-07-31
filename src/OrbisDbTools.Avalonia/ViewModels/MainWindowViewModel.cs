using System.Threading;
using System;
using System.Reactive;
using System.Threading.Tasks;
using OrbisDbTools.Lib.Constants;
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
    public ReactiveCommand<Unit, Unit> AddMissingDLC { get; }
    public ReactiveCommand<Unit, Unit> RecalculateDbContent { get; }
    public ReactiveCommand<Unit, Unit> AllowDeleteApps { get; }
    public ReactiveCommand<Unit, Unit> HidePsnApps { get; }
    public ReactiveCommand<Unit, Unit> ForceDc { get; }
    public ReactiveCommand<Unit, Unit> CancelProgress { get; }
    public ReactiveCommand<AppTitle, Unit> DeleteAppCommand { get; }

    public EventHandler<DataGridCellEditEndedEventArgs> CellEditEnded { get; }
    public EventHandler<DataGridCellPointerPressedEventArgs> CellPointerPressed { get; }

    private readonly MainWindowController _controller;

    public bool DbConnected { get => dbConnected; set => this.RaiseAndSetIfChanged(ref dbConnected, value); }
    private bool dbConnected;

    public bool IsLocalDb { get => isLocalDb; set => this.RaiseAndSetIfChanged(ref isLocalDb, value); }
    private bool isLocalDb;

    public string ConsoleIpAddress { get => consoleIpAddress; set => this.RaiseAndSetIfChanged(ref consoleIpAddress, value); }
    private string consoleIpAddress = string.Empty;

    public bool ShowProgressBar { get => showProgressBar; set => this.RaiseAndSetIfChanged(ref showProgressBar, value); }
    private bool showProgressBar;

    public bool AllowProgressCancel { get => allowProgressCancel; set => this.RaiseAndSetIfChanged(ref allowProgressCancel, value); }
    private bool allowProgressCancel;

    public string ProgressText { get => progressText; set => this.RaiseAndSetIfChanged(ref progressText, value); }
    private string progressText = string.Empty;

    public string ConnectionError { get => connectionError; set => this.RaiseAndSetIfChanged(ref connectionError, value); }
    private string connectionError = string.Empty;

    public ObservableCollection<AppTitle> AppDbItems { get => appDbItems; set => this.RaiseAndSetIfChanged(ref appDbItems, value); }
    private ObservableCollection<AppTitle> appDbItems = new();

    public ObservableCollection<AddContTblRow> DlcDbItems { get => dlcDbItems; set => this.RaiseAndSetIfChanged(ref dlcDbItems, value); }
    private ObservableCollection<AddContTblRow> dlcDbItems = new();

    public Func<Task<Uri?>> OpenLocalDbDialogAction = null!;
    public Func<Task<Uri?>> SaveDbLocallyDialogAction = null!;
    public Func<string, Task<bool>> ShowWarningDialogAction = null!;
    public Func<string, Task> ShowInfoDialogAction = null!;

    private CancellationTokenSource? _progressCancelation;

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
        AddMissingDLC = ReactiveCommand.CreateFromTask(FixDlcs);
        CancelProgress = ReactiveCommand.CreateFromTask(CancelProgressTask);
        DeleteAppCommand = ReactiveCommand.CreateFromTask<AppTitle>(DeleteApp);

        CellEditEnded += OnCellEditEnded;
        CellPointerPressed += OnCellPointerPressed;
    }

    private void OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.Row.DataContext is not AppTitle title) return;

#pragma warning disable CS0618 // I don't want to rewrite this, so we'll just use this for now
        if (e.PointerPressedEventArgs.MouseButton.ToString() != "Right") return;
#pragma warning restore CS0618

        var flyout = new MenuFlyout
        {
            Items = new[]
            {
                new MenuItem { Header = "Delete App", Command = DeleteAppCommand, CommandParameter = title }
            }
        };

        flyout.ShowAt(e.Cell, true);
    }

    private async void OnCellEditEnded(object? _, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (e.Row.DataContext is not AppTitle editedApp) return;

        await _controller.UpdateEditedApp(editedApp);
    }

    async Task FixDatabase()
    {
        var warningAccepted = await ShowWarningDialogAction(PromptMessages.FixDb);
        if (!warningAccepted) return;

        ShowSpinner("Rebuilding missing app entries...");
        await _controller.FixMissingAppTitles();

        await UpdateDbViewItems();
        ShowProgressBar = false;
    }

    async Task FixDlcs()
    {
        var warningAccepted = await ShowWarningDialogAction(PromptMessages.FixDlcs);
        if (!warningAccepted) return;

        ShowSpinner("Rebuilding missing DLC entries...");
        await _controller.RebuildAddCont();

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
                IsLocalDb = true;
                await UpdateDbViewItems();
            }
        }
        catch (Exception e)
        {
            ConnectionError = $"Failed to connect: {e.Message}";
        }
    }

    async Task UpdateDbViewItems()
    {
        var appDbItems = await _controller.QueryInstalledApps();
        AppDbItems = new(appDbItems);

        if (!IsLocalDb)
        {
            var dlcDbItems = await _controller.QueryInstalledDlc();
            DlcDbItems = new(dlcDbItems);
        }
    }

    async Task ForceDisconnect()
    {
        var prompt = IsLocalDb ? PromptMessages.LocalOverwriteWarning : PromptMessages.FtpUploadWarning;
        var warningAccepted = await ShowWarningDialogAction(prompt);
        if (!warningAccepted) return;

        ShowSpinner("Finishing...");

        if (IsLocalDb)
        {
            await _controller.CloseLocalDb();
        }
        else
        {
            await _controller.WriteChangesAndDisconnect().ConfigureAwait(true);
            await ShowInfoDialogAction(PromptMessages.UploadFinished);
        }

        DbConnected = false;
        ShowProgressBar = false;
    }

    async Task DownloadDatabase()
    {
        var cts = ShowSpinner("Connecting, please wait...", true) ?? default;

        try
        {
            DbConnected = await _controller.ConnectAndDownload(consoleIpAddress, cts).ConfigureAwait(false);
            IsLocalDb = false;
            await UpdateDbViewItems();
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
        var warningAccepted = await ShowWarningDialogAction(PromptMessages.CalculateSize);
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

    async Task DeleteApp(AppTitle title)
    {
        var warningAccepted = await ShowWarningDialogAction($"Really want to delete '{title.TitleName}' from database?");
        if (!warningAccepted) return;

        await _controller.DeleteApp(title);
        await UpdateDbViewItems();
    }

    Task CancelProgressTask()
    {
        _progressCancelation?.Cancel();
        return Task.CompletedTask;
    }

    private CancellationToken? ShowSpinner(string text, bool allowCancel = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ProgressText = "Please wait...";
        }

        ProgressText = text;
        ConnectionError = string.Empty;
        ShowProgressBar = true;
        AllowProgressCancel = allowCancel;

        if (!allowCancel)
        {
            return null;
        }

        _progressCancelation = new CancellationTokenSource();
        return _progressCancelation.Token;
    }

    private void HideSpinner()
    {
        ShowProgressBar = false;
    }
}