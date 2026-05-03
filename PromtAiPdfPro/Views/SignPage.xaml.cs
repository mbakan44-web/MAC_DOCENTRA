using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PromtAiPdfPro.Services;
using WUI = Wpf.Ui.Controls;
using PdfSharp.Drawing;

namespace PromtAiPdfPro.Views
{
    public partial class SignPage : Page
    {
        private readonly PdfService _pdfService = new PdfService();
        private string? _selectedPdfPath;
        private string? _uploadedImagePath;
        private uint _currentPageIndex = 0;
        private int _totalPageCount = 0;
        
        // Drag and Drop variables
        private bool _isDragging = false;
        private Point _clickPosition;
        private double _originalLeft, _originalTop;

        public SignPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private async void BtnSelectPdf_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf" };
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedPdfPath = openFileDialog.FileName;
                TxtSelectedPdf.Text = _selectedPdfPath;
                _currentPageIndex = 0;
                _totalPageCount = _pdfService.GetPageCount(_selectedPdfPath);
                
                await LoadPreview();
                StackPreviewEmpty.Visibility = Visibility.Collapsed;
                PanelPageNav.Visibility = Visibility.Visible;
                BorderSignPreview.Visibility = Visibility.Visible;
                
                // Position signature in middle-bottom initially
                Canvas.SetLeft(BorderSignPreview, 100);
                Canvas.SetTop(BorderSignPreview, 300);
            }
        }

        private async Task LoadPreview()
        {
            if (string.IsNullOrEmpty(_selectedPdfPath)) return;
            
            var bitmap = await _pdfService.GetPdfPageImageAsync(_selectedPdfPath, _currentPageIndex);
            if (bitmap != null)
            {
                ImgPdfPreview.Source = bitmap;
                TxtPageInfo.Text = $"Page {_currentPageIndex + 1} / {_totalPageCount}";
            }
        }

        private void RbSignMethod_Changed(object sender, RoutedEventArgs e)
        {
            if (PanelDraw == null || PanelUpload == null) return;

            if (RbDraw.IsChecked == true)
            {
                PanelDraw.Visibility = Visibility.Visible;
                PanelUpload.Visibility = Visibility.Collapsed;
                UpdateSignaturePreviewFromInk();
            }
            else
            {
                PanelDraw.Visibility = Visibility.Collapsed;
                PanelUpload.Visibility = Visibility.Visible;
                UpdateSignaturePreviewFromImage();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InkSign.Strokes.Clear();
            ImgSignOnPdf.Source = null;
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (openFileDialog.ShowDialog() == true)
            {
                _uploadedImagePath = openFileDialog.FileName;
                ImgUploaded.Source = new BitmapImage(new Uri(_uploadedImagePath));
                ImgUploaded.Visibility = Visibility.Visible;
                UpdateSignaturePreviewFromImage();
            }
        }

        private void UpdateSignaturePreviewFromImage()
        {
            if (!string.IsNullOrEmpty(_uploadedImagePath))
            {
                ImgSignOnPdf.Source = new BitmapImage(new Uri(_uploadedImagePath));
                BorderSignPreview.Width = SliderSize.Value;
                BorderSignPreview.Height = double.NaN; // Auto
            }
        }

        private void UpdateSignaturePreviewFromInk()
        {
            if (InkSign.Strokes.Count > 0)
            {
                ImgSignOnPdf.Source = GetInkImage();
                BorderSignPreview.Width = SliderSize.Value;
                BorderSignPreview.Height = double.NaN; // Auto
            }
        }

        private void InkSign_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            UpdateSignaturePreviewFromInk();
        }

        private BitmapSource GetInkImage()
        {
            Rect bounds = InkSign.Strokes.GetBounds();
            if (bounds.IsEmpty) return null!;

            // Create a bitmap of the ink
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)InkSign.ActualWidth, (int)InkSign.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(InkSign);
            
            // We could crop it to bounds here, but keeping it simple for now
            return rtb;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BorderSignPreview != null)
            {
                BorderSignPreview.Width = SliderSize.Value;
                BorderSignPreview.Opacity = SliderOpacity.Value;
            }
        }

        // --- Drag & Drop Logic ---
        private void CanvasOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if we clicked the signature border or anything inside it
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && dep != CanvasOverlay)
            {
                if (dep == BorderSignPreview)
                {
                    _isDragging = true;
                    _clickPosition = e.GetPosition(CanvasOverlay);
                    _originalLeft = Canvas.GetLeft(BorderSignPreview);
                    _originalTop = Canvas.GetTop(BorderSignPreview);
                    if (double.IsNaN(_originalLeft)) _originalLeft = 0;
                    if (double.IsNaN(_originalTop)) _originalTop = 0;
                    
                    BorderSignPreview.CaptureMouse();
                    e.Handled = true;
                    return;
                }
                dep = VisualTreeHelper.GetParent(dep);
            }
        }

        private void CanvasOverlay_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPos = e.GetPosition(CanvasOverlay);
                double deltaX = currentPos.X - _clickPosition.X;
                double deltaY = currentPos.Y - _clickPosition.Y;
                
                Canvas.SetLeft(BorderSignPreview, _originalLeft + deltaX);
                Canvas.SetTop(BorderSignPreview, _originalTop + deltaY);
            }
        }

        private void CanvasOverlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                BorderSignPreview.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private async void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                await LoadPreview();
            }
        }

        private async void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageIndex < _totalPageCount - 1)
            {
                _currentPageIndex++;
                await LoadPreview();
            }
        }

        private async void BtnSign_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPdfPath))
            {
                ShowMessage((string)FindResource("Sign_NoFile"), WUI.SymbolRegular.ErrorCircle24, WUI.ControlAppearance.Danger);
                return;
            }

            if (ImgSignOnPdf.Source == null)
            {
                ShowMessage((string)FindResource("Sign_NoSignature"), WUI.SymbolRegular.ErrorCircle24, WUI.ControlAppearance.Danger);
                return;
            }

            try
            {
                string targetPath = Path.Combine(Path.GetDirectoryName(_selectedPdfPath)!, 
                    Path.GetFileNameWithoutExtension(_selectedPdfPath) + "_Signed.pdf");

                // Get signature image as file path
                string tempSignPath = Path.Combine(Path.GetTempPath(), "temp_sign.png");
                SaveImageSourceToFile(ImgSignOnPdf.Source, tempSignPath);

                // Calculate real coordinates for PDF
                // We need to map Canvas coordinates to PDF points
                double canvasWidth = CanvasOverlay.ActualWidth;
                double canvasHeight = CanvasOverlay.ActualHeight;
                
                // Get image display rect within the Uniform stretch
                Rect imgRect = GetImageDisplayedRect(ImgPdfPreview);
                
                double signLeft = Canvas.GetLeft(BorderSignPreview);
                double signTop = Canvas.GetTop(BorderSignPreview);
                if (double.IsNaN(signLeft)) signLeft = 0;
                if (double.IsNaN(signTop)) signTop = 0;

                // Relative to image rect
                double relX = (signLeft - imgRect.X) / imgRect.Width;
                double relY = (signTop - imgRect.Y) / imgRect.Height;
                double relW = BorderSignPreview.ActualWidth / imgRect.Width;
                double relH = BorderSignPreview.ActualHeight / imgRect.Height;

                // range
                string range = "all";
                if (RbCurrent.IsChecked == true) range = (_currentPageIndex + 1).ToString();
                else if (RbCustom.IsChecked == true) range = TxtCustomRange.Text;

                // We need a helper in PdfService that uses relative coordinates or we calculate them here.
                // Let's modify AddImageWatermarkAsync to handle relative coordinates or just calculate points here.
                
                // For simplicity, I'll pass the relative rect to a new service method or use existing one with calculated points.
                // Let's use existing but we need to know PDF page size.
                
                // For now, let's just use a fixed target and I'll refine the PdfService if needed.
                // I'll add a 'Signed' version of Image Watermark that handles the coordinates better.
                
                // Actually, I'll just calculate the points here using iText logic (not yet in PdfService).
                // Let's add a specialized method to PdfService for this.
                
                bool success = await _pdfService.AddImageWatermarkAsync(_selectedPdfPath, targetPath, tempSignPath, 
                    new XRect(relX, relY, relW, relH), SliderOpacity.Value, range, true); // Added 'isRelative' flag

                if (success)
                {
                    if (MessageBox.Show((string)FindResource("Sign_SuccessWithOpen"), (string)FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(targetPath) { UseShellExecute = true });
                    }
                }
                else
                {
                    ShowMessage((string)FindResource("Msg_Error"), WUI.SymbolRegular.ErrorCircle24, WUI.ControlAppearance.Danger);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, WUI.SymbolRegular.ErrorCircle24, WUI.ControlAppearance.Danger);
            }
        }

        private void SaveImageSourceToFile(ImageSource source, string filePath)
        {
            var bitmap = (BitmapSource)source;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(filePath))
            {
                encoder.Save(stream);
            }
        }

        private Rect GetImageDisplayedRect(System.Windows.Controls.Image img)
        {
            if (img.Source == null) return new Rect();

            double sw = img.Source.Width;
            double sh = img.Source.Height;
            double aw = img.ActualWidth;
            double ah = img.ActualHeight;

            double scale = Math.Min(aw / sw, ah / sh);
            double dw = sw * scale;
            double dh = sh * scale;

            return new Rect((aw - dw) / 2, (ah - dh) / 2, dw, dh);
        }

        private void ShowMessage(string message, WUI.SymbolRegular icon, WUI.ControlAppearance appearance)
        {
            var messageBox = new WUI.MessageBox
            {
                Title = (string)FindResource("Sign_Title"),
                Content = message,
                CloseButtonText = (string)FindResource("Prem_Close")
            };
            messageBox.ShowDialogAsync();
        }
    }
}
