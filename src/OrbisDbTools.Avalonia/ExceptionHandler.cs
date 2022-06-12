using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Text.Json;
using ReactiveUI;

namespace OrbisDbTools.Avalonia;

public class ExceptionHandler : IObserver<Exception>
{
    public void OnNext(Exception value)
    {
        HandleException(value);
    }

    public void OnError(Exception error)
    {
        HandleException(error);
    }

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();
        RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }

    private static void HandleException(Exception e)
    {
        var info = new Dictionary<string, string> {
            {"Type", e.GetType().ToString()},
            {"Message", e.Message},
            {"StackTrace", e.StackTrace ?? "N/A" }};

        PrintDictionary(info);

        if (Debugger.IsAttached) Debugger.Break();

        RxApp.MainThreadScheduler.Schedule(() => throw e);
    }

    private static void PrintDictionary(IDictionary<string, string> dictionary)
    {
        var infoJson = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions() { WriteIndented = true });
        Console.WriteLine(infoJson);
    }
}