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
    public partial class ProtectPage : UserControl
    {
        private readonly PdfService _pdfService;
        private string? _protectFile;
        private string? _unlockFile;

        public ProtectPage()
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
                Title = this.FindResource("Gen_SelectFile")?.ToString() ?? "Select File",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf }
            });

            if (files.Count > 0)
            {
                _protectFile = files[0].Path.LocalPath;
                SelectedFileText.Text = files[0].Name;
            }
        }

        private async void SelectFileForUnlock_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = this.FindResource("Gen_SelectFile")?.ToString() ?? "Select File",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf }
            });

            if (files.Count > 0)
            {
                _unlockFile = files[0].Path.LocalPath;
                SelectedUnlockFileText.Text = files[0].Name;
            }
        }

        private async void Protect_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_protectFile) || string.IsNullOrWhiteSpace(UserPassword.Text))
            {
                StatusText.Text = this.FindResource("Protect_SelectError")?.ToString() ?? "Please select a file and enter a password.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Protected PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_protectFile) + "_protected.pdf"
            });

            if (file != null)
            {
                StatusText.Text = this.FindResource("Gen_Processing")?.ToString() ?? "Processing...";
                bool success = await _pdfService.ProtectPdfAsync(_protectFile, file.Path.LocalPath, UserPassword.Text);
                
                if (success)
                {
                    StatusText.Text = this.FindResource("Protect_Success")?.ToString() ?? "PDF encrypted successfully!";
                    ShowSuccessDialog(file.Path.LocalPath);
                }
                else
                {
                    StatusText.Text = this.FindResource("Protect_Error")?.ToString() ?? "Error: Encryption failed!";
                }
            }
        }

        private async void Unlock_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_unlockFile) || string.IsNullOrWhiteSpace(UnlockPassword.Text))
            {
                StatusText.Text = this.FindResource("Unlock_SelectError")?.ToString() ?? "Please select a file and password.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Unlocked PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_unlockFile) + "_unlocked.pdf"
            });

            if (file != null)
            {
                StatusText.Text = this.FindResource("Gen_Processing")?.ToString() ?? "Processing...";
                bool success = await _pdfService.UnlockPdfAsync(_unlockFile, file.Path.LocalPath, UnlockPassword.Text);
                
                if (success)
                {
                    StatusText.Text = this.FindResource("Unlock_Success")?.ToString() ?? "Password removed successfully!";
                    ShowSuccessDialog(file.Path.LocalPath);
                }
                else
                {
                    StatusText.Text = this.FindResource("Unlock_Error")?.ToString() ?? "Error: Wrong password!";
                }
            }
        }

        private async void ShowSuccessDialog(string path)
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
