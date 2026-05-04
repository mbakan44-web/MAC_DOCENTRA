using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System.IO;

namespace Docentra_Mac.Views.Pages
{
    public partial class ProtectPage : UserControl
    {
        private string? _selectedFile;
        private readonly PdfService _pdfService = new PdfService();

        public ProtectPage()
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

        private async void Protect_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || string.IsNullOrWhiteSpace(UserPassword.Text))
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Protected PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_protected.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                string ownerPass = string.IsNullOrWhiteSpace(OwnerPassword.Text) ? UserPassword.Text : OwnerPassword.Text;

                bool success = await _pdfService.ProtectPdfAsync(
                    _selectedFile, 
                    file.Path.LocalPath, 
                    UserPassword.Text, 
                    ownerPass);

                if (success)
                {
                    StatusText.Text = (string)this.FindResource("Protect_Success")!;
                }
                else
                {
                    StatusText.Text = (string)this.FindResource("Gen_Error")!;
                }
            }
        }
    }
}
