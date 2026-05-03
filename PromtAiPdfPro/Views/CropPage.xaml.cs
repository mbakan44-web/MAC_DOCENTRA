using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public class CenterConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double d) return (d / 2) - 6;
            return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }

    public partial class CropPage : Page
    {
        private PdfService _pdfService = new PdfService();
        private string _sourceFile = "";
        private int _currentPage = 0;
        private int _totalPages = 0;
        private double _zoom = 1.0;
        private Rect _selectionRect;
        private Dictionary<int, Rect> _pageSelections = new Dictionary<int, Rect>();

        public CropPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
            }
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter") };
            if (dialog.ShowDialog() == true)
            {
                _sourceFile = dialog.FileName;
                TxtFileName.Text = Path.GetFileName(_sourceFile);
                _totalPages = await _pdfService.GetPageCountAsync(_sourceFile);
                _currentPage = 0;
                _pageSelections.Clear();
                _selectionRect = Rect.Empty;
                await LoadPage();
            }
        }

        private async System.Threading.Tasks.Task LoadPage()
        {
            if (string.IsNullOrEmpty(_sourceFile)) return;
            
            var bitmap = await _pdfService.GetPdfPageImageAsync(_sourceFile, (uint)_currentPage);
            if (bitmap != null)
            {
                ImgPreview.Source = bitmap;
                TxtPageInfo.Text = $"{_currentPage + 1} / {_totalPages}";
                
                // Ensure layout is updated before calculation. Background priority ensures UI is rendered.
                await Dispatcher.InvokeAsync(async () => {
                    await FitToScreenAsync();
                    SelectionCanvas.Visibility = Visibility.Visible;
                    
                    if (ChkApplyToAll.IsChecked == true)
                    {
                        // Use a global selection if set, or create one
                        if (_selectionRect.IsEmpty)
                        {
                            _selectionRect = new Rect(ImgPreview.Width * 0.1, ImgPreview.Height * 0.1, ImgPreview.Width * 0.8, ImgPreview.Height * 0.8);
                        }
                    }
                    else
                    {
                        // Use per-page selection
                        if (_pageSelections.ContainsKey(_currentPage))
                        {
                            _selectionRect = _pageSelections[_currentPage];
                        }
                        else
                        {
                            // Default for this page
                            _selectionRect = new Rect(ImgPreview.Width * 0.1, ImgPreview.Height * 0.1, ImgPreview.Width * 0.8, ImgPreview.Height * 0.8);
                            _pageSelections[_currentPage] = _selectionRect;
                        }
                    }

                    // Bounds check for different page sizes
                    _selectionRect.Width = Math.Min(_selectionRect.Width, ImgPreview.Width);
                    _selectionRect.Height = Math.Min(_selectionRect.Height, ImgPreview.Height);
                    _selectionRect.X = Math.Min(_selectionRect.X, ImgPreview.Width - _selectionRect.Width);
                    _selectionRect.Y = Math.Min(_selectionRect.Y, ImgPreview.Height - _selectionRect.Height);

                    UpdateSelectionUI();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async Task FitToScreenAsync()
        {
            if (ImgPreview.Source is BitmapSource bitmap)
            {
                // Set natural size
                ImgPreview.Width = bitmap.PixelWidth;
                ImgPreview.Height = bitmap.PixelHeight;
                SelectionCanvas.Width = bitmap.PixelWidth;
                SelectionCanvas.Height = bitmap.PixelHeight;
                OverlayFullRect.Rect = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);

                // Wait up to 500ms for layout to provide valid dimensions
                double availableW = 0, availableH = 0;
                for (int i = 0; i < 5; i++)
                {
                    availableW = PreviewScrollViewer.ViewportWidth;
                    availableH = PreviewScrollViewer.ViewportHeight;
                    if (availableW <= 0) availableW = PreviewScrollViewer.ActualWidth;
                    if (availableH <= 0) availableH = PreviewScrollViewer.ActualHeight;

                    if (availableW > 50) break; // Found valid size
                    await Task.Delay(100);
                }

                availableW -= 40;
                availableH -= 40;

                if (availableW > 0 && availableH > 0)
                {
                    double scaleX = availableW / bitmap.PixelWidth;
                    double scaleY = availableH / bitmap.PixelHeight;
                    _zoom = Math.Min(scaleX, scaleY);
                    UpdateZoom();
                }
                else
                {
                    // Fallback to a reasonable default if still 0
                    _zoom = 0.5;
                    UpdateZoom();
                }
            }
        }

        private void PreviewScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Optional: Auto-refit on window resize if you want
            // FitToScreen();
        }

        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newX = _selectionRect.X + e.HorizontalChange;
            double newY = _selectionRect.Y + e.VerticalChange;

            // Bounds check
            newX = Math.Max(0, Math.Min(newX, SelectionCanvas.ActualWidth - _selectionRect.Width));
            newY = Math.Max(0, Math.Min(newY, SelectionCanvas.ActualHeight - _selectionRect.Height));

            _selectionRect.X = newX;
            _selectionRect.Y = newY;
            
            if (ChkApplyToAll.IsChecked != true)
            {
                _pageSelections[_currentPage] = _selectionRect;
            }
            
            UpdateSelectionUI();
        }

        private void ResizeHandle_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                string tag = thumb.Tag.ToString() ?? "";
                double newX = _selectionRect.X;
                double newY = _selectionRect.Y;
                double newWidth = _selectionRect.Width;
                double newHeight = _selectionRect.Height;

                if (tag.Contains("W"))
                {
                    double change = Math.Min(e.HorizontalChange, newWidth - 10);
                    newX += change;
                    newWidth -= change;
                }
                if (tag.Contains("E"))
                {
                    newWidth = Math.Max(10, newWidth + e.HorizontalChange);
                }
                if (tag.Contains("N"))
                {
                    double change = Math.Min(e.VerticalChange, newHeight - 10);
                    newY += change;
                    newHeight -= change;
                }
                if (tag.Contains("S"))
                {
                    newHeight = Math.Max(10, newHeight + e.VerticalChange);
                }

                // Bounds check
                if (newX < 0) { newWidth += newX; newX = 0; }
                if (newY < 0) { newHeight += newY; newY = 0; }
                if (newX + newWidth > SelectionCanvas.ActualWidth) newWidth = SelectionCanvas.ActualWidth - newX;
                if (newY + newHeight > SelectionCanvas.ActualHeight) newHeight = SelectionCanvas.ActualHeight - newY;

                _selectionRect = new Rect(newX, newY, newWidth, newHeight);

                if (ChkApplyToAll.IsChecked != true)
                {
                    _pageSelections[_currentPage] = _selectionRect;
                }

                UpdateSelectionUI();
            }
        }

        private void UpdateSelectionUI()
        {
            if (SelectionCanvas == null || SelectionBox == null) return;

            double canvasW = SelectionCanvas.Width > 0 ? SelectionCanvas.Width : SelectionCanvas.ActualWidth;
            double canvasH = SelectionCanvas.Height > 0 ? SelectionCanvas.Height : SelectionCanvas.ActualHeight;

            if (canvasW <= 0 || canvasH <= 0) return;

            Canvas.SetLeft(SelectionBox, _selectionRect.X);
            Canvas.SetTop(SelectionBox, _selectionRect.Y);
            SelectionBox.Width = Math.Max(10, _selectionRect.Width);
            SelectionBox.Height = Math.Max(10, _selectionRect.Height);

            // Update overlay paths
            if (OverlayFullRect != null) OverlayFullRect.Rect = new Rect(0, 0, canvasW, canvasH);
            if (OverlaySelectionRect != null) OverlaySelectionRect.Rect = _selectionRect;
        }

        private async void BtnCrop_Click(object sender, RoutedEventArgs e)
        {
            if (ImgPreview.Source == null) return;

            // Ask where to save
            var settings = SettingsService.Instance.Current;
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = Path.GetFileNameWithoutExtension(_sourceFile) + "_Cropped.pdf",
                InitialDirectory = string.IsNullOrEmpty(settings.DefaultOutputPath) ? "" : settings.DefaultOutputPath
            };

            if (sfd.ShowDialog() != true) return;

            string targetPath = sfd.FileName;

            var bitmap = (BitmapSource)ImgPreview.Source;
            double x = _selectionRect.X;
            double y = _selectionRect.Y;
            double w = _selectionRect.Width;
            double h = _selectionRect.Height;

            bool success = await _pdfService.CropPdfAsync(_sourceFile, targetPath, x, y, w, h, ChkApplyToAll.IsChecked == true, _currentPage, _pageSelections);
            
            if (success)
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Success"), (string)Application.Current.FindResource("Msg_ProcessSuccess"), Wpf.Ui.Controls.ControlAppearance.Success);
                
                // Ask to open
                var result = await (new Wpf.Ui.Controls.MessageBox
                {
                    Title = (string)Application.Current.FindResource("Msg_Success"),
                    Content = (string)Application.Current.FindResource("Crop_SuccessWithOpen"),
                    PrimaryButtonText = (string)Application.Current.FindResource("Btn_Open"),
                    CloseButtonText = (string)Application.Current.FindResource("Btn_Close")
                }).ShowDialogAsync();

                if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(targetPath) { UseShellExecute = true });
                }
            }
            else
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Error"), (string)Application.Current.FindResource("Msg_ProcessError"), Wpf.Ui.Controls.ControlAppearance.Danger);
            }
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e) { _zoom += 0.1; UpdateZoom(); }
        private void BtnZoomOut_Click(object sender, RoutedEventArgs e) { if (_zoom > 0.1) _zoom -= 0.1; UpdateZoom(); }
        
        private void UpdateZoom() 
        { 
            if (CropGridTransform != null)
            {
                CropGridTransform.ScaleX = _zoom;
                CropGridTransform.ScaleY = _zoom;
                TxtZoom.Text = $"{(int)(_zoom * 100)}%"; 
            }
        }

        private async void BtnPrevPage_Click(object sender, RoutedEventArgs e) { if (_currentPage > 0) { _currentPage--; await LoadPage(); } }
        private async void BtnNextPage_Click(object sender, RoutedEventArgs e) { if (_currentPage < _totalPages - 1) { _currentPage++; await LoadPage(); } }

        private void ShowMessage(string title, string message, Wpf.Ui.Controls.ControlAppearance appearance)
        {
            if (Application.Current.MainWindow is MainView mv)
            {
                mv.SnackbarService.Show(title, message, appearance, new Wpf.Ui.Controls.SymbolIcon(appearance == Wpf.Ui.Controls.ControlAppearance.Success ? Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24 : Wpf.Ui.Controls.SymbolRegular.Warning24), TimeSpan.FromSeconds(3));
            }
        }
    }
}
