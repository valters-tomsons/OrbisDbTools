using System.IO;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OrbisDbTools.Avalonia.ViewModels;
using System.Linq;
using System.Collections.Generic;

namespace OrbisDbTools.Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow() { }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            viewModel.OpenFileDialogAction = new Func<Task<Uri?>>(ShowFileBrowserDialogWindow);

            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async Task<Uri?> ShowFileBrowserDialogWindow()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Browse for local database...",
                Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() { Name = "Database files (.db)", Extensions = new List<string>() { "db*" } },
                    new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } },
                },
                AllowMultiple = false
            };

            var result = await dialog.ShowAsync(this).ConfigureAwait(true);
            var resultStr = result?.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(resultStr) && File.Exists(resultStr))
            {
                return new Uri(resultStr, UriKind.Absolute);
            }

            return null;
        }
    }
}