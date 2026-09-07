using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using Docentra_Mac.Views.Dialogs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docentra_Mac.Views.Pages
{
    public partial class WatermarkPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedFile;
        private string? _selectedImage;

        public WatermarkPage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
        }

        private async void SelectFile_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

        private async void SelectImage_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = this.FindResource("Gen_SelectFile")?.ToString() ?? "Select Image",
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                _selectedImage = files[0].Path.LocalPath;
                SelectedImageText.Text = files[0].Name;
            }
        }

        private async void Apply_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || string.IsNullOrEmpty(WatermarkText.Text))
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            var editor = new WatermarkEditorWindow(WatermarkText.Text);
            await editor.ShowDialog(topLevel as Window ?? (Window)topLevel);

            if (editor.Success)
            {
                await ProcessWatermark(editor.ResultRect, editor.OpacityValue, editor.Rotation, editor.FontSize, editor.FontColor, editor.ApplyToAll);
            }
        }

        private async void OpenImageEditor_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || string.IsNullOrEmpty(_selectedImage))
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            double pw = 595, ph = 842;
            try
            {
                pw = _pdfService.GetPageWidth(_selectedFile);
                ph = _pdfService.GetPageHeight(_selectedFile);
            }
            catch { }

            // Reusing SignEditorWindow for image placement
            var editor = new SignEditorWindow(_selectedImage, pw, ph);
            await editor.ShowDialog(topLevel as Window ?? (Window)topLevel);

            if (editor.Success)
            {
                await ProcessImageWatermark(editor.ResultRect, editor.OpacityValue);
            }
        }

        private async Task ProcessWatermark(Rect rect, double opacity, double rotation, double fontSize, string color, bool allPages)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = this.FindResource("Gen_SaveFile")?.ToString() ?? "Save File",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_watermarked.pdf"
            });

            if (file != null)
            {
                bool success = await _pdfService.AddTextWatermarkAsync(_selectedFile!, file.Path.LocalPath, WatermarkText.Text!, rect, opacity, rotation, fontSize, color, allPages ? "all" : "0");
                ShowResult(success, file.Path.LocalPath);
            }
        }

        private async Task ProcessImageWatermark(Rect rect, double opacity)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = this.FindResource("Gen_SaveFile")?.ToString() ?? "Save File",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_logo.pdf"
            });

            if (file != null)
            {
                bool success = await _pdfService.AddImageWatermarkAsync(_selectedFile!, file.Path.LocalPath, _selectedImage!, rect, opacity, "all");
                ShowResult(success, file.Path.LocalPath);
            }
        }

        private async void ShowResult(bool success, string path)
        {
            if (success)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                var msg = this.FindResource("Gen_OpenQuestion")?.ToString() ?? "Process completed. Open file?";
                var dialog = new Docentra_Mac.Views.Dialogs.MessageDialog(msg);
                await dialog.ShowDialog(topLevel as Window ?? (Window)topLevel);
                if (dialog.Result)
                {
                    _pdfService.OpenFile(path);
                }
            }
        }
    }
}
