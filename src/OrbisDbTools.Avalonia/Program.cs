using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OrbisDbTools.Avalonia.Views;
using OrbisDbTools.Avalonia.ViewModels;
using OrbisDbTools.Lib.Abstractions;
using OrbisDbTools.Lib.Controllers;
using OrbisDbTools.Lib.Providers;
using ReactiveUI;
using System.Reactive;

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
        RxApp.DefaultExceptionHandler = Observer.AsObserver(new ExceptionHandler());

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

    // AutoFac configuration, registers all components
    [ComVisible(false)]
    private static IContainer RegisterServices()
    {
        var builder = new ContainerBuilder();

        RegisterSingleInstance(builder, new List<Type>
        {
            typeof(MainWindow),
            typeof(MainWindowController),
            typeof(MainWindowViewModel),

            typeof(OrbisFtp),
            typeof(FileSystemProvider),
            typeof(AppDbProvider),
        });

        return builder.Build();
    }

    private static void RegisterSingleInstance(ContainerBuilder builder, IReadOnlyCollection<Type> types)
    {
        foreach (var type in types)
        {
            builder.RegisterType(type).SingleInstance();
        }
    }
}