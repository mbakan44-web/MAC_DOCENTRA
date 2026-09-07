using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docentra_Mac.Views.Pages
{
    public partial class DeletePagesPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedFile;

        public DeletePagesPage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
        }

        private async void SelectFile_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Gen_SelectFile")!,
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf }
            });

            if (files.Count > 0)
            {
                _selectedFile = files[0].Path.LocalPath;
                SelectedFileText.Text = files[0].Name;
            }
        }

        private async void Delete_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                StatusText.Text = this.FindResource("Delete_SelectError")?.ToString() ?? "Please select a file.";
                return;
            }

            string range = TxtRange.Text ?? "";
            if (string.IsNullOrWhiteSpace(range))
            {
                StatusText.Text = this.FindResource("Split_RangeHint")?.ToString() ?? "Please enter pages (e.g. 1, 3, 5-8).";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_deleted.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                bool success = await _pdfService.DeletePagesAsync(_selectedFile, file.Path.LocalPath, range);
                
                if (success)
                {
                    StatusText.Text = this.FindResource("Delete_Success")?.ToString() ?? "Pages deleted successfully!";
                    var msg = this.FindResource("Gen_OpenQuestion")?.ToString() ?? "Process completed. Open file?";
                    var dialog = new Docentra_Mac.Views.Dialogs.MessageDialog(msg);
                    await dialog.ShowDialog(topLevel as Window ?? (Window)topLevel);
                    if (dialog.Result)
                    {
                        _pdfService.OpenFile(file.Path.LocalPath);
                    }
                }
                else
                {
                    StatusText.Text = this.FindResource("Gen_Error")?.ToString() ?? "Error!";
                }
            }
        }
    }
}
