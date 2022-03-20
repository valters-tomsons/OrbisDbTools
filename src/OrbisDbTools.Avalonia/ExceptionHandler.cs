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
        if (Debugger.IsAttached) Debugger.Break();

        var info = new Dictionary<string, string> {
            {"Type", value.GetType().ToString()},
            {"Message", value.Message}};

        PrintDictionary(info);

        RxApp.MainThreadScheduler.Schedule(() => throw value);
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

        var info = new Dictionary<string, string>() {
            {"Type", error.GetType().ToString()},
            {"Message", error.Message}};

        PrintDictionary(info);

        RxApp.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();
        RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }

    private static void PrintDictionary(IDictionary<string, string> dictionary)
    {
        var infoJson = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions() { WriteIndented = true });
        Console.WriteLine(infoJson);
    }
}