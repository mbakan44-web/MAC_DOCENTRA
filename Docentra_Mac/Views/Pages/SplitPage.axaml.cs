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
    public partial class SplitPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedFile;

        public SplitPage()
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

        private async void Split_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                StatusText.Text = this.FindResource("Split_SelectError")?.ToString() ?? "Please select a file.";
                return;
            }

            string? range = null;
            if (RbSplitRange.IsChecked == true)
            {
                range = TxtRange.Text;
                if (string.IsNullOrWhiteSpace(range))
                {
                    StatusText.Text = this.FindResource("Split_RangeHint")?.ToString() ?? "Please enter a range.";
                    return;
                }
            }

            StatusText.Text = (string)this.FindResource("Gen_Processing")!;
            bool success = await _pdfService.SplitPagesAsync(_selectedFile, range);
            
            if (success)
            {
                StatusText.Text = this.FindResource("Split_Success")?.ToString() ?? "File split successfully!";
                var topLevel = TopLevel.GetTopLevel(this);
                var msg = this.FindResource("Gen_OpenQuestion")?.ToString() ?? "Process completed. Open folder?";
                var dialog = new Docentra_Mac.Views.Dialogs.MessageDialog(msg);
                await dialog.ShowDialog(topLevel as Window ?? (Window)topLevel);
                if (dialog.Result)
                {
                    string dir = Path.GetDirectoryName(_selectedFile) ?? "";
                    _pdfService.OpenFile(dir);
                }
            }
            else
            {
                StatusText.Text = this.FindResource("Gen_Error")?.ToString() ?? "Error!";
            }
        }
    }
}
