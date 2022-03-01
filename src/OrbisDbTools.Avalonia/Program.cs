using System;
using System.Runtime.InteropServices;
using Autofac;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls;
using OrbisDbTools.Avalonia.ViewModels;
using OrbisDbTools.Avalonia.Views;
using OrbisDbTools.PS4.AppDb;
using OrbisDbTools.PS4.Discovery;

namespace OrbisDbTools.Avalonia;

static class Program
{
    internal static ILifetimeScope? Scope;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var app = BuildAvaloniaApp().SetupWithoutStarting().Instance;

        var container = RegisterServices();
        Scope = container.BeginLifetimeScope();

        var mainWindow = Scope.Resolve<MainWindow>();
        app.Run(mainWindow);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    [ComVisible(false)]
    private static IContainer RegisterServices()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MainWindow>().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().SingleInstance();

        builder.RegisterType<DiscoveryService>().SingleInstance();
        builder.RegisterType<AppDbProvider>().SingleInstance();
        builder.RegisterType<AppDbController>().SingleInstance();

        return builder.Build();
    }
}