using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Docentra_Mac.Views.Pages
{
    public partial class PdfViewerPage : UserControl
    {
        private string? _currentFile;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _zoomLevel = 100;
        private readonly PdfService _pdfService;

        public PdfViewerPage()
        {
            InitializeComponent();
            _pdfService = new PdfService();
        }

        public PdfViewerPage(string filePath) : this()
        {
            _currentFile = filePath;
            var _ = LoadPdfAsync(filePath);
        }

        private async void Open_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = this.FindResource("PdfViewer_Open")?.ToString() ?? "Open PDF",
                FileTypeFilter = new[] { FilePickerFileTypes.Pdf },
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                await LoadPdfAsync(files[0].Path.LocalPath);
            }
        }

        private async Task LoadPdfAsync(string filePath)
        {
            if (!File.Exists(filePath)) return;

            _currentFile = filePath;
            _currentPage = 1;
            _zoomLevel = 100;

            EmptyState.IsVisible = false;
            LoadingPanel.IsVisible = true;
            PageScrollViewer.IsVisible = false;

            try
            {
                await Task.Run(() =>
                {
                    _totalPages = _pdfService.GetPageCount(filePath);
                });

                double width = _pdfService.GetPageWidth(filePath);
                double height = _pdfService.GetPageHeight(filePath);
                
                // Format file size
                var fileInfo = new FileInfo(filePath);
                double sizeInMb = fileInfo.Length / (1024.0 * 1024.0);
                string sizeStr = sizeInMb < 0.1 
                    ? $"{fileInfo.Length / 1024.0:F1} KB" 
                    : $"{sizeInMb:F2} MB";

                TxtFileName.Text = Path.GetFileName(filePath);
                TxtDocTitle.Text = Path.GetFileName(filePath);
                TxtPageCount.Text = $"{_totalPages} Pages";
                TxtPageSize.Text = $"{width:F0} x {height:F0} pt";
                TxtFileSize.Text = sizeStr;

                TxtInfo.Text = $"Successfully analyzed. Ready to view or print.";

                LoadingPanel.IsVisible = false;
                PageScrollViewer.IsVisible = true;
                UpdateToolbar();
            }
            catch (Exception ex)
            {
                LoadingPanel.IsVisible = false;
                EmptyState.IsVisible = true;
                TxtFileName.Text = "Error loading file";
                Console.WriteLine($"Error reading PDF: {ex.Message}");
            }
        }

        private void ZoomIn_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentFile == null) return;
            _zoomLevel = Math.Min(_zoomLevel + 25, 200);
            UpdateToolbar();
        }

        private void ZoomOut_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentFile == null) return;
            _zoomLevel = Math.Max(_zoomLevel - 25, 50);
            UpdateToolbar();
        }

        private void NextPage_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentFile == null || _currentPage >= _totalPages) return;
            _currentPage++;
            UpdateToolbar();
        }

        private void PrevPage_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentFile == null || _currentPage <= 1) return;
            _currentPage--;
            UpdateToolbar();
        }

        private void OpenExternal_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentFile == null) return;
            _pdfService.OpenFile(_currentFile);
        }

        private void UpdateToolbar()
        {
            TxtZoom.Text = $"{_zoomLevel}%";
            TxtPage.Text = _currentFile != null ? $"{_currentPage} / {_totalPages}" : "— / —";

            bool hasFile = _currentFile != null;
            BtnZoomIn.IsEnabled = hasFile && _zoomLevel < 200;
            BtnZoomOut.IsEnabled = hasFile && _zoomLevel > 50;
            BtnNext.IsEnabled = hasFile && _currentPage < _totalPages;
            BtnPrev.IsEnabled = hasFile && _currentPage > 1;
            BtnExternal.IsEnabled = hasFile;
        }
    }
}
