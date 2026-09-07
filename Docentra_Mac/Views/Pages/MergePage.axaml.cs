using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Docentra_Mac.Views.Pages
{
    public partial class MergePage : UserControl
    {
        private readonly PdfService _pdfService;
        public ObservableCollection<string> Files { get; } = new ObservableCollection<string>();

        public MergePage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
            FileList.ItemsSource = Files;
        }

        private async void AddFiles_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Merge_AddFiles")!,
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf },
                AllowMultiple = true
            });

            foreach (var file in files)
            {
                Files.Add(file.Path.LocalPath);
            }
        }

        private void RemoveFile_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filePath)
            {
                Files.Remove(filePath);
            }
        }

        private void MoveUp_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filePath)
            {
                int index = Files.IndexOf(filePath);
                if (index > 0)
                {
                    Files.Move(index, index - 1);
                }
            }
        }

        private void MoveDown_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filePath)
            {
                int index = Files.IndexOf(filePath);
                if (index < Files.Count - 1)
                {
                    Files.Move(index, index + 1);
                }
            }
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            Files.Clear();
        }

        private async void Merge_Click(object? sender, RoutedEventArgs e)
        {
            if (Files.Count < 2)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                var msg = this.FindResource("Merge_SelectError")?.ToString() ?? "Please select at least two files.";
                var dialog = new Docentra_Mac.Views.Dialogs.MessageDialog(msg);
                await dialog.ShowDialog(topLevel as Window ?? (Window)topLevel);
                return;
            }

            var topLevelSave = TopLevel.GetTopLevel(this);
            var file = await topLevelSave.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = this.FindResource("Gen_SaveFile")?.ToString() ?? "Save File",
                DefaultExtension = "pdf",
                SuggestedFileName = "Merged_Document.pdf"
            });

            if (file != null)
            {
                bool success = await _pdfService.MergeFilesAsync(Files.ToList(), file.Path.LocalPath);
                if (success)
                {
                    var msg = this.FindResource("Gen_OpenQuestion")?.ToString() ?? "Process completed. Open file?";
                    var dialog = new Docentra_Mac.Views.Dialogs.MessageDialog(msg);
                    await dialog.ShowDialog(topLevelSave as Window ?? (Window)topLevelSave);
                    if (dialog.Result)
                    {
                        _pdfService.OpenFile(file.Path.LocalPath);
                    }
                }
            }
        }
    }
}
