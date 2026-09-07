using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;

namespace Docentra_Mac.Views.Pages
{
    public partial class CompressPage : UserControl
    {
        private readonly PdfService _pdfService = new PdfService();
        private string? _selectedFile;
        private string _compressionLevel = "Low";

        public CompressPage()
        {
            InitializeComponent();
        }

        private async void BtnBrowse_Click(object? sender, RoutedEventArgs e)
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
                _selectedFile = files[0].Path.LocalPath;
                TxtSourceFile.Text = Path.GetFileName(_selectedFile);
            }
        }

        private void CardLevel_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string level)
            {
                _compressionLevel = level;
                UpdateLevelUI();
            }
        }

        private void UpdateLevelUI()
        {
            // Reset borders
            CardLow.BorderThickness = new Avalonia.Thickness(1);
            CardLow.BorderBrush = new SolidColorBrush(Color.Parse("#15FFFFFF"));

            CardMed.BorderThickness = new Avalonia.Thickness(1);
            CardMed.BorderBrush = new SolidColorBrush(Color.Parse("#15FFFFFF"));

            CardHigh.BorderThickness = new Avalonia.Thickness(1);
            CardHigh.BorderBrush = new SolidColorBrush(Color.Parse("#15FFFFFF"));

            // Highlights
            var premiumAccent = (ISolidColorBrush)this.FindResource("PremiumAccentBrush")!;
            
            if (_compressionLevel == "Low")
            {
                CardLow.BorderThickness = new Avalonia.Thickness(2);
                CardLow.BorderBrush = premiumAccent;
            }
            else if (_compressionLevel == "Medium")
            {
                CardMed.BorderThickness = new Avalonia.Thickness(2);
                CardMed.BorderBrush = premiumAccent;
            }
            else if (_compressionLevel == "High")
            {
                CardHigh.BorderThickness = new Avalonia.Thickness(2);
                CardHigh.BorderBrush = premiumAccent;
            }
        }

        private async void BtnCompress_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile)) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Compressed PDF",
                DefaultExtension = "pdf",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(Path.GetDirectoryName(_selectedFile)!)),
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_Compressed.pdf",
                FileTypeChoices = new[] { FilePickerFileTypes.Pdf }
            });

            if (file != null)
            {
                BtnCompress.IsEnabled = false;
                ProgressRing.IsVisible = true;

                bool success = await _pdfService.CompressPdfAsync(_selectedFile, file.Path.LocalPath, _compressionLevel);

                BtnCompress.IsEnabled = true;
                ProgressRing.IsVisible = false;

                if (success)
                {
                    _pdfService.OpenFile(file.Path.LocalPath);
                }
            }
        }
    }
}
