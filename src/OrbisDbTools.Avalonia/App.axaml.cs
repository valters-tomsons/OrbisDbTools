using Avalonia;
using Avalonia.Markup.Xaml;

namespace OrbisDbTools.Avalonia;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}