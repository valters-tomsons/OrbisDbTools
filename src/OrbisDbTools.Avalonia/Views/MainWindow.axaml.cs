using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OrbisDbTools.Avalonia.ViewModels;

namespace OrbisDbTools.Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow() { }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}