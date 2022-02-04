using System.Reactive;
using System.Threading.Tasks;
using OrbisDbTools.PS4.AppDb;
using ReactiveUI;

namespace OrbisDbTools.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> ConnectDb { get; }
    public ReactiveCommand<Unit, Unit> RecalculateDbContent { get; }
    public ReactiveCommand<Unit, Unit> AllowDeleteApps { get; }
    public ReactiveCommand<Unit, Unit> HidePsnApps { get; }
    public ReactiveCommand<Unit, Unit> ForceDc { get; }

    private readonly AppDbController _controller;

    public bool EnableDbActions { get => enabledbactions; set => this.RaiseAndSetIfChanged(ref enabledbactions, value); }
    private bool enabledbactions;

    public string ConsoleIpAddress { get => consoleIpAddress; set => this.RaiseAndSetIfChanged(ref consoleIpAddress, value); }
    private string consoleIpAddress = string.Empty;

    public MainWindowViewModel(AppDbController controller)
    {
        _controller = controller;

        ConnectDb = ReactiveCommand.CreateFromTask(DownloadDatabase);
        RecalculateDbContent = ReactiveCommand.CreateFromTask(RecalculateContent);
        AllowDeleteApps = ReactiveCommand.CreateFromTask(MarkCanRemoveInstalled);
        HidePsnApps = ReactiveCommand.CreateFromTask(HidePSNApps);
        ForceDc = ReactiveCommand.CreateFromTask(ForceDisconnect);
    }

    async Task ForceDisconnect()
    {
        await _controller.DisconnectFromConsole();
    }

    async Task DownloadDatabase()
    {
        EnableDbActions = await _controller.DownloadAndConnect(consoleIpAddress);
    }

    async Task HidePSNApps()
    {
        await _controller.HideAllKnownPsnApps();
    }

    async Task RecalculateContent()
    {
        await _controller.ReCalculateInstalledAppSizes();
    }

    async Task MarkCanRemoveInstalled()
    {
        await _controller.AllowDeleteInstalledApps();
    }
}