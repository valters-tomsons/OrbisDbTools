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

            viewModel.OpenLocalDbDialogAction = new Func<Task<Uri?>>(ShowFileBrowserDialogWindow);
            viewModel.SaveDbLocallyDialogAction = new Func<Task<Uri?>>(ShowSaveFileDialogWindow);

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
                Title = "Select app.db file that you recently dumped from your PS4",
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

        public async Task<Uri?> ShowSaveFileDialogWindow()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save the modified app.db file locally",
                InitialFileName = "app",
                DefaultExtension = ".db",
                Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter() { Name = "SQLite database (.db)", Extensions = new List<string>() { "db" } },
                }
            };

            var result = await dialog.ShowAsync(this).ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }

            if (File.Exists(result))
            {
                throw new Exception("Selected file already exists");
            }

            return new(result, UriKind.Absolute);
        }
    }
}