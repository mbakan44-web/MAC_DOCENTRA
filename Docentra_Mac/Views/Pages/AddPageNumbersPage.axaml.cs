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
    public partial class AddPageNumbersPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedFile;
        private string _selectedColor = "#000000";

        public AddPageNumbersPage()
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

        private void Color_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string hex)
            {
                _selectedColor = hex;
                TxtSelectedColor.Text = hex;
            }
        }

        private async void AddNumbers_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                StatusText.Text = this.FindResource("PageNum_SelectError")?.ToString() ?? "Please select a file.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save PDF with Page Numbers",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_numbered.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                string format = string.IsNullOrWhiteSpace(TxtFormat.Text) ? "Page {n} of {total}" : TxtFormat.Text;
                string font = (ComboFont.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Arial";
                string pos = (ComboPosition.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BottomCenter";

                bool success = await _pdfService.AddPageNumbersAsync(
                    _selectedFile,
                    file.Path.LocalPath,
                    (int)(NumStart.Value ?? 1),
                    (int)(NumEnd.Value ?? 1000),
                    pos,
                    format,
                    font,
                    (double)(NumFontSize.Value ?? 12),
                    _selectedColor,
                    30, // margin
                    (int)(NumStartValue.Value ?? 1)
                );

                if (success)
                {
                    StatusText.Text = this.FindResource("PageNum_Success")?.ToString() ?? "Page numbers added successfully!";
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
