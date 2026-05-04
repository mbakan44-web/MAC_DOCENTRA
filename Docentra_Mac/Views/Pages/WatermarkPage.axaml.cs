using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;

namespace Docentra_Mac.Views.Pages
{
    public partial class WatermarkPage : UserControl
    {
        private string? _selectedFile;
        private readonly PdfService _pdfService = new PdfService();

        public WatermarkPage()
        {
            InitializeComponent();
        }

        private async void SelectFile_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select PDF File",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf },
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                _selectedFile = files[0].Path.LocalPath;
                SelectedFileText.Text = Path.GetFileName(_selectedFile);
            }
        }

        private async void Apply_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || string.IsNullOrWhiteSpace(WatermarkText.Text))
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Watermarked PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_watermarked.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                double opacity = (double)(OpacityValue.Value ?? 50) / 100.0;
                double rotation = (double)(RotationValue.Value ?? 45);

                bool success = await _pdfService.AddTextWatermarkAsync(
                    _selectedFile, 
                    file.Path.LocalPath, 
                    WatermarkText.Text, 
                    opacity, 
                    rotation);

                if (success)
                {
                    StatusText.Text = (string)this.FindResource("Watermark_Success")!;
                }
                else
                {
                    StatusText.Text = (string)this.FindResource("Gen_Error")!;
                }
            }
        }
    }
}
