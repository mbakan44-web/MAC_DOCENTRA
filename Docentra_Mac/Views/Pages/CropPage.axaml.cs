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
    public partial class CropPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedFile;
        private Rect _cropRect;
        private bool _applyToAll = true;

        public CropPage()
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

        private async void OpenEditor_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                StatusText.Text = this.FindResource("Crop_SelectError")?.ToString() ?? "Please select a file.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            double pw = 595, ph = 842;
            try
            {
                pw = _pdfService.GetPageWidth(_selectedFile);
                ph = _pdfService.GetPageHeight(_selectedFile);
            }
            catch { }

            var editor = new Docentra_Mac.Views.Dialogs.CropEditorWindow(pw, ph);
            await editor.ShowDialog(topLevel as Window ?? (Window)topLevel);

            if (editor.Success)
            {
                _cropRect = editor.ResultRect;
                _applyToAll = editor.ApplyToAll;
                StatusText.Text = this.FindResource("Sett_ThemeActive")?.ToString() ?? "Area selected.";
            }
        }

        private async void ApplyCrop_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || _cropRect.Width <= 0)
            {
                StatusText.Text = this.FindResource("Crop_SelectError")?.ToString() ?? "Please select a file and set crop area.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Cropped PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_cropped.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                // Note: Windows uses a dictionary for multi-page selections, 
                // but here we align with the basic "Apply to All" or "Current Page" logic.
                bool success = await _pdfService.CropPdfAsync(
                    _selectedFile,
                    file.Path.LocalPath,
                    _cropRect.X,
                    _cropRect.Y,
                    _cropRect.Width,
                    _cropRect.Height,
                    _applyToAll,
                    0 // Current page index (simplification for now)
                );

                if (success)
                {
                    StatusText.Text = this.FindResource("Crop_Success")?.ToString() ?? "File cropped successfully!";
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
