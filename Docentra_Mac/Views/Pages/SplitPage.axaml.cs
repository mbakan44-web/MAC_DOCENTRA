using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
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
                Title = "Select PDF File",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("PDF Files") { Patterns = new[] { "*.pdf" } } }
            });

            if (files.Count > 0)
            {
                _selectedFile = files[0].Path.LocalPath;
                SelectedFilePath.Text = _selectedFile;
            }
        }

        private async void Split_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile)) return;

            bool success = await _pdfService.SplitPagesAsync(_selectedFile, RangeInput.Text);
            if (success)
            {
                // Success logic
            }
        }
    }
}
