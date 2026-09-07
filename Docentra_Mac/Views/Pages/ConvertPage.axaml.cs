using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Docentra_Mac.Views.Pages
{
    public partial class ConvertPage : UserControl
    {
        private readonly PdfService _pdfService;

        public ConvertPage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
        }

        private async void ImageToPdf_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Dash_ImgTitle")!,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll },
                AllowMultiple = true
            });

            if (files.Count > 0)
            {
                var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save PDF",
                    DefaultExtension = "pdf",
                    SuggestedFileName = "Images_Converted.pdf"
                });

                if (saveFile != null)
                {
                    bool success = await _pdfService.ImagesToPdfAsync(files.Select(f => f.Path.LocalPath).ToList(), saveFile.Path.LocalPath);
                    ShowResult(success, saveFile.Path.LocalPath);
                }
            }
        }

        private async void PdfToImage_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = this.FindResource("Gen_SelectFile")?.ToString() ?? "Select PDF",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf }
            });

            if (files.Count > 0)
            {
                var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = this.FindResource("Gen_SaveFile")?.ToString() ?? "Select Folder"
                });

                if (folder.Count > 0)
                {
                    bool success = await _pdfService.PdfToImageAsync(files[0].Path.LocalPath, folder[0].Path.LocalPath);
                    if (success) _pdfService.OpenFile(folder[0].Path.LocalPath);
                }
            }
        }

        private async void OfficeToPdf_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Dash_OfficeTitle")!,
                FileTypeFilter = new[] { 
                    new FilePickerFileType("Office Documents") { Patterns = new[] { "*.docx", "*.doc", "*.xlsx", "*.xls", "*.pptx", "*.ppt" } } 
                }
            });

            if (files.Count > 0)
            {
                var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save PDF",
                    DefaultExtension = "pdf",
                    SuggestedFileName = Path.GetFileNameWithoutExtension(files[0].Name) + ".pdf"
                });

                if (saveFile != null)
                {
                    bool success = await _pdfService.OfficeToPdfAsync(files[0].Path.LocalPath, saveFile.Path.LocalPath);
                    ShowResult(success, saveFile.Path.LocalPath);
                }
            }
        }

        private async void PdfToWord_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)this.FindResource("Dash_WordTitle")!,
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf }
            });

            if (files.Count > 0)
            {
                var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Word Document",
                    DefaultExtension = "docx",
                    SuggestedFileName = Path.GetFileNameWithoutExtension(files[0].Name) + ".docx"
                });

                if (saveFile != null)
                {
                    bool success = await _pdfService.PdfToWordAsync(files[0].Path.LocalPath, saveFile.Path.LocalPath);
                    ShowResult(success, saveFile.Path.LocalPath);
                }
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
