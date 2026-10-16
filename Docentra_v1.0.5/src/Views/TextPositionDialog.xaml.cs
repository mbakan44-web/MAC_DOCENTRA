using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PdfSharp.Drawing;
using Wpf.Ui.Controls;

namespace PromtAiPdfPro.Views
{
    public partial class TextPositionDialog : FluentWindow
    {
        private bool _isDragging = false;
        private Point _clickPosition;
        private double _textLeft = 0;
        private double _textTop = 0;
        private double _pageWidth;
        private double _pageHeight;

        public XRect ResultRect { get; private set; }
        public double ResultOpacity { get; private set; }
        public double ResultFontSize { get; private set; }
        public double ResultRotation { get; private set; }
        public bool Success { get; private set; }

        public TextPositionDialog(string watermarkText, double pageWidth = 595, double pageHeight = 842)
        {
            InitializeComponent();
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;
            DraggableText.Text = watermarkText;

            Loaded += (s, e) => 
            {
                // Initial position (center)
                if (DraggableText.ActualWidth > 0 && _textLeft == 0)
                {
                    _textLeft = (PageCanvas.ActualWidth - DraggableText.ActualWidth) / 2;
                    _textTop = (PageCanvas.ActualHeight - DraggableText.ActualHeight) / 2;
                    UpdatePosition();
                }
            };
        }

        private void Text_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _clickPosition = e.GetPosition(DraggableText);
            DraggableText.CaptureMouse();
        }

        private void Text_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPos = e.GetPosition(PageCanvas);
                _textLeft = currentPos.X - _clickPosition.X;
                _textTop = currentPos.Y - _clickPosition.Y;

                // Bounds check
                _textLeft = Math.Max(0, Math.Min(_textLeft, PageCanvas.ActualWidth - DraggableText.ActualWidth));
                _textTop = Math.Max(0, Math.Min(_textTop, PageCanvas.ActualHeight - DraggableText.ActualHeight));

                UpdatePosition();
            }
        }

        private void Text_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            DraggableText.ReleaseMouseCapture();
        }

        private void UpdatePosition()
        {
            Canvas.SetLeft(DraggableText, _textLeft);
            Canvas.SetTop(DraggableText, _textTop);
        }

        private void SldFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DraggableText != null)
            {
                DraggableText.FontSize = e.NewValue;
                UpdateRotationCenter();
                UpdatePosition();
            }
        }

        private void SldRotation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextRotate != null)
            {
                TextRotate.Angle = e.NewValue;
                UpdateRotationCenter();
            }
        }

        private void UpdateRotationCenter()
        {
            if (DraggableText != null && TextRotate != null)
            {
                DraggableText.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private void SldOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DraggableText != null)
                DraggableText.Opacity = e.NewValue / 100.0;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            // Dynamic scaling based on actual PDF page size
            double scaleX = _pageWidth / PageCanvas.ActualWidth;
            double scaleY = _pageHeight / PageCanvas.ActualHeight;

            // Important: We need to pass the font size scaled up to the real PDF resolution too
            ResultFontSize = SldFontSize.Value * scaleY; 

            ResultRect = new XRect(
                _textLeft * scaleX,
                _textTop * scaleY,
                DraggableText.ActualWidth * scaleX,
                DraggableText.ActualHeight * scaleY
            );

            ResultOpacity = SldOpacity.Value / 100.0;
            ResultRotation = SldRotation.Value;
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
