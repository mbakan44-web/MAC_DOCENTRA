using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Docentra_Mac.Views.Pages
{
    public partial class MergePage : UserControl
    {
        private readonly PdfService _pdfService;
        public ObservableCollection<string> Files { get; } = new ObservableCollection<string>();

        public MergePage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
            FileList.ItemsSource = Files;
        }

        private async void AddFiles_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select PDF Files",
                AllowMultiple = true,
                FileTypeFilter = new[] { new FilePickerFileType("PDF Files") { Patterns = new[] { "*.pdf" } } }
            });

            foreach (var file in files)
            {
                Files.Add(file.Path.LocalPath);
            }
        }

        private void RemoveFile_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filePath)
            {
                Files.Remove(filePath);
            }
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            Files.Clear();
        }

        private async void Merge_Click(object? sender, RoutedEventArgs e)
        {
            if (Files.Count < 2) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Merged PDF",
                DefaultExtension = "pdf",
                SuggestedFileName = "Merged_Document.pdf"
            });

            if (file != null)
            {
                bool success = await _pdfService.MergeFilesAsync(Files.ToList(), file.Path.LocalPath);
                if (success)
                {
                    // Show success notification (could use a dialog or a custom notification)
                }
            }
        }
    }
}
