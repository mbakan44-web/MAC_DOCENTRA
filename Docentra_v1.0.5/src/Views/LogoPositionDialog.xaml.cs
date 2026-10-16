using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using PdfSharp.Drawing;
using Wpf.Ui.Controls;

namespace PromtAiPdfPro.Views
{
    public partial class LogoPositionDialog : FluentWindow
    {
        private bool _isDragging = false;
        private Point _clickPosition;
        private double _logoLeft = 0;
        private double _logoTop = 0;
        private double _baseWidth = 0;
        private double _pageWidth;
        private double _pageHeight;

        public XRect ResultRect { get; private set; }
        public double ResultOpacity { get; private set; }
        public bool Success { get; private set; }

        public LogoPositionDialog(string imagePath, double pageWidth = 595, double pageHeight = 842)
        {
            InitializeComponent();
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;
            var bitmap = new BitmapImage(new Uri(imagePath));
            DraggableLogo.Source = bitmap;
            
            Loaded += (s, e) => 
            {
                if (DraggableLogo.ActualWidth > 0 && _logoLeft == 0)
                {
                    _baseWidth = DraggableLogo.ActualWidth;
                    _logoLeft = (PageCanvas.ActualWidth - DraggableLogo.ActualWidth) / 2;
                    _logoTop = (PageCanvas.ActualHeight - DraggableLogo.ActualHeight) / 2;
                    UpdateLogoPosition();
                }
            };
        }

        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(DraggableLogo);
            DraggableLogo.CaptureMouse();
        }

        private void Logo_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPos = e.GetPosition(PageCanvas);
                _logoLeft = currentPos.X - _clickPosition.X;
                _logoTop = currentPos.Y - _clickPosition.Y;

                // Bounds
                _logoLeft = Math.Max(0, Math.Min(_logoLeft, PageCanvas.ActualWidth - DraggableLogo.ActualWidth));
                _logoTop = Math.Max(0, Math.Min(_logoTop, PageCanvas.ActualHeight - DraggableLogo.ActualHeight));

                UpdateLogoPosition();
            }
        }

        private void Logo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            DraggableLogo.ReleaseMouseCapture();
        }

        private void UpdateLogoPosition()
        {
            Canvas.SetLeft(DraggableLogo, _logoLeft);
            Canvas.SetTop(DraggableLogo, _logoTop);
        }

        private void SldLogoScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DraggableLogo != null && _baseWidth > 0)
            {
                DraggableLogo.Width = _baseWidth * (e.NewValue / 100.0);
                UpdateLogoPosition();
            }
        }

        private void SldOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DraggableLogo != null)
                DraggableLogo.Opacity = e.NewValue / 100.0;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            // Dynamic scaling based on actual PDF page size
            double scaleX = _pageWidth / PageCanvas.ActualWidth;
            double scaleY = _pageHeight / PageCanvas.ActualHeight;

            ResultRect = new XRect(
                _logoLeft * scaleX,
                _logoTop * scaleY,
                DraggableLogo.ActualWidth * scaleX,
                DraggableLogo.ActualHeight * scaleY
            );

            ResultOpacity = SldOpacity.Value / 100.0;
            Success = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Success = false;
            Close();
        }
    }
}
