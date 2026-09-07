using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;

namespace Docentra_Mac.Views.Pages
{
    public partial class MetadataEditorPage : UserControl
    {
        private string? _currentFilePath;
        private readonly PdfService _pdfService = new PdfService();

        public MetadataEditorPage()
        {
            InitializeComponent();
        }

        private async void BtnSelectFile_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select PDF File",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf },
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                await LoadFileAsync(files[0].Path.LocalPath);
            }
        }

        private async System.Threading.Tasks.Task LoadFileAsync(string filePath)
        {
            _currentFilePath = filePath;
            TxtSelectedFile.Text = Path.GetFileName(filePath);

            LoadingRing.IsVisible = true;
            SettingsPanel.IsEnabled = false;
            BtnSave.IsEnabled = false;

            var metadata = await _pdfService.GetMetadataAsync(filePath);

            LoadingRing.IsVisible = false;

            if (metadata != null)
            {
                TxtTitle.Text = metadata.Title;
                TxtAuthor.Text = metadata.Author;
                TxtSubject.Text = metadata.Subject;
                TxtKeywords.Text = metadata.Keywords;
                TxtCreator.Text = metadata.Creator;
                TxtProducer.Text = metadata.Producer;
                TxtCreationDate.Text = string.IsNullOrEmpty(metadata.CreationDate) ? "-" : metadata.CreationDate;
                TxtModificationDate.Text = string.IsNullOrEmpty(metadata.ModificationDate) ? "-" : metadata.ModificationDate;

                SettingsPanel.IsEnabled = true;
                BtnSave.IsEnabled = true;
            }
        }

        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save PDF",
                DefaultExtension = "pdf",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(Path.GetDirectoryName(_currentFilePath)!)),
                SuggestedFileName = Path.GetFileNameWithoutExtension(_currentFilePath) + "_metadata.pdf",
                FileTypeChoices = new[] { FilePickerFileTypes.Pdf }
            });

            if (file != null)
            {
                string targetPath = file.Path.LocalPath;

                var metadata = new Models.PdfMetadataModel
                {
                    Title = TxtTitle.Text ?? "",
                    Author = TxtAuthor.Text ?? "",
                    Subject = TxtSubject.Text ?? "",
                    Keywords = TxtKeywords.Text ?? "",
                    Creator = TxtCreator.Text ?? "",
                    Producer = TxtProducer.Text ?? ""
                };

                LoadingRing.IsVisible = true;
                SettingsPanel.IsEnabled = false;
                BtnSave.IsEnabled = false;

                bool result = await _pdfService.SetMetadataAsync(_currentFilePath, targetPath, metadata);

                LoadingRing.IsVisible = false;
                SettingsPanel.IsEnabled = true;
                BtnSave.IsEnabled = true;

                if (result)
                {
                    _pdfService.OpenFile(targetPath);
                }
            }
        }
    }
}
