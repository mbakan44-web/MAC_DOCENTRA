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
    public partial class SignPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _selectedPdf;
        private string? _selectedImage;
        private Rect _signRect;
        private double _opacity = 1.0;

        public SignPage()
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
                _selectedPdf = files[0].Path.LocalPath;
                SelectedFileText.Text = files[0].Name;
            }
        }

        private async void SelectImage_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Sign_SelectImage")!,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                _selectedImage = files[0].Path.LocalPath;
                SelectedImageText.Text = files[0].Name;
            }
        }

        private async void OpenEditor_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPdf) || string.IsNullOrEmpty(_selectedImage))
            {
                StatusText.Text = this.FindResource("Sign_SelectError")?.ToString() ?? "Please select PDF and image.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            double pw = 595, ph = 842;
            try
            {
                pw = _pdfService.GetPageWidth(_selectedPdf);
                ph = _pdfService.GetPageHeight(_selectedPdf);
            }
            catch { }

            var editor = new Docentra_Mac.Views.Dialogs.SignEditorWindow(_selectedImage, pw, ph);
            await editor.ShowDialog(topLevel as Window ?? (Window)topLevel);

            if (editor.Success)
            {
                _signRect = editor.ResultRect;
                _opacity = editor.OpacityValue;
                StatusText.Text = this.FindResource("Sett_ThemeActive")?.ToString() ?? "Position set.";
            }
        }

        private async void Sign_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPdf) || string.IsNullOrEmpty(_selectedImage) || _signRect.Width <= 0)
            {
                StatusText.Text = this.FindResource("Sign_SelectError")?.ToString() ?? "Please select file and set position.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Signed PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedPdf) + "_signed.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                // AddImageWatermarkAsync actually handles placing images/logos on PDF
                bool success = await _pdfService.AddImageWatermarkAsync(
                    _selectedPdf,
                    file.Path.LocalPath,
                    _selectedImage,
                    new Avalonia.Rect(_signRect.X, _signRect.Y, _signRect.Width, _signRect.Height),
                    _opacity,
                    "all" // Default to all pages for now, or we can add a toggle
                );

                if (success)
                {
                    StatusText.Text = this.FindResource("Sign_Success")?.ToString() ?? "File signed successfully!";
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
