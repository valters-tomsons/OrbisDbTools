using System.IO;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OrbisDbTools.Avalonia.ViewModels;
using System.Linq;
using System.Collections.Generic;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;

namespace OrbisDbTools.Avalonia.Views;

public class MainWindow : Window
{
    public MainWindow() { }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        viewModel.OpenLocalDbDialogAction = new Func<Task<Uri?>>(ShowFileBrowserDialogWindow);
        viewModel.SaveDbLocallyDialogAction = new Func<Task<Uri?>>(ShowSaveFileDialogWindow);

        DataContext = viewModel;

        var dbGrid = this.FindControl<DataGrid>("DbGrid")!;
        dbGrid.CellEditEnded += viewModel.CellEditEnded;
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
        var saveDialog = new SaveFileDialog
        {
            Title = "Save the modified app.db file locally",
            InitialFileName = "app",
            DefaultExtension = ".db",
            Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter() { Name = "SQLite database (.db)", Extensions = new List<string>() { "db" } },
                }
        };

        var resultFilePath = await saveDialog.ShowAsync(this).ConfigureAwait(true);

        if (string.IsNullOrWhiteSpace(resultFilePath))
        {
            return null;
        }

        if (File.Exists(resultFilePath))
        {
            return await PromptFileOverwrite(resultFilePath, saveDialog);
        }

        return new(resultFilePath, UriKind.Absolute);
    }

    private async Task<Uri?> PromptFileOverwrite(string filePath, SaveFileDialog saveDialog)
    {
        // Yes, this is a bit cursed
        // No, I don't care

        while (true)
        {
            var overwritePrompt = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = "Overwrite?",
                ContentMessage = $"File `{Path.GetFileName(filePath)}` already exists. Overwrite?",
                Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                ShowInCenter = true,
            });

            var buttonResult = await overwritePrompt.ShowDialog(this);

            if (buttonResult == ButtonResult.Ok)
            {
                return new(filePath, UriKind.Absolute);
            }
            else if (buttonResult == ButtonResult.Cancel)
            {
                var resultFilePath = await saveDialog.ShowAsync(this).ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(resultFilePath))
                {
                    return null;
                }
                if (File.Exists(resultFilePath))
                {
                    continue;
                }

                return new Uri(resultFilePath, UriKind.Absolute);
            }
            else if (buttonResult == ButtonResult.None)
            {
                return null;
            }
        }
    }
}