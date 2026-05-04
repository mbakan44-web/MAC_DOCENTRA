using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System.IO;

namespace Docentra_Mac.Views.Pages
{
    public partial class AddPageNumbersPage : UserControl
    {
        private string? _selectedFile;
        private readonly PdfService _pdfService = new PdfService();

        public AddPageNumbersPage()
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

        private async void Add_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save PDF with Page Numbers",
                DefaultExtension = "pdf",
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_numbered.pdf"
            });

            if (file != null)
            {
                StatusText.Text = (string)this.FindResource("Gen_Processing")!;
                
                string position = "BottomCenter";
                if (PositionList.SelectedIndex == 1) position = "BottomRight";
                else if (PositionList.SelectedIndex == 2) position = "TopCenter";

                bool success = await _pdfService.AddPageNumbersAsync(
                    _selectedFile, 
                    file.Path.LocalPath, 
                    position);

                if (success)
                {
                    StatusText.Text = (string)this.FindResource("PageNum_Success")!;
                }
                else
                {
                    StatusText.Text = (string)this.FindResource("Gen_Error")!;
                }
            }
        }
    }
}
