using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;

namespace Docentra_Mac.Views.Dialogs
{
    public partial class SignEditorWindow : Window
    {
        private bool _isDragging = false;
        private bool _isResizing = false;
        private Point _clickPosition;
        private double _posX = 150;
        private double _posY = 267;
        private double _width = 150;
        private double _height = 100;
        private double _pageWidth;
        private double _pageHeight;

        public bool Success { get; private set; }
        public Rect ResultRect { get; private set; }
        public double OpacityValue { get; private set; } = 1.0;

        public SignEditorWindow()
        {
            InitializeComponent();
        }

        public SignEditorWindow(string imagePath, double pageWidth = 595, double pageHeight = 842) : this()
        {
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;
            try
            {
                SignPreviewImage.Source = new Bitmap(imagePath);
            }
            catch { }
        }

        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source == DraggableImageBorder || e.Source == SignPreviewImage)
            {
                _isDragging = true;
                _clickPosition = e.GetPosition(DraggableImageBorder);
                e.Pointer.Capture(SignCanvas);
            }
        }

        private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isResizing = true;
            _clickPosition = e.GetPosition(this);
            e.Pointer.Capture(SignCanvas);
            e.Handled = true;
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var currentPos = e.GetPosition(SignCanvas);
                _posX = currentPos.X - _clickPosition.X;
                _posY = currentPos.Y - _clickPosition.Y;

                // Bounds
                _posX = Math.Max(0, Math.Min(_posX, 450 - _width));
                _posY = Math.Max(0, Math.Min(_posY, 635 - _height));

                UpdateUI();
            }
            else if (_isResizing)
            {
                var currentPos = e.GetPosition(SignCanvas);
                _width = Math.Max(20, currentPos.X - _posX);
                _height = Math.Max(20, currentPos.Y - _posY);

                // Bounds
                _width = Math.Min(_width, 450 - _posX);
                _height = Math.Min(_height, 635 - _posY);

                UpdateUI();
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            e.Pointer.Capture(null);
        }

        private void UpdateUI()
        {
            Canvas.SetLeft(DraggableImageBorder, _posX);
            Canvas.SetTop(DraggableImageBorder, _posY);
            DraggableImageBorder.Width = _width;
            DraggableImageBorder.Height = _height;
        }

        private void Control_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (DraggableImageBorder != null)
                DraggableImageBorder.Opacity = e.NewValue / 100.0;
        }

        private void Apply_Click(object? sender, RoutedEventArgs e)
        {
            double scaleX = _pageWidth / 450.0;
            double scaleY = _pageHeight / 635.0;

            ResultRect = new Rect(
                _posX * scaleX,
                _posY * scaleY,
                _width * scaleX,
                _height * scaleY
            );
            OpacityValue = SldOpacity.Value / 100.0;
            Success = true;
            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Success = false;
            Close();
        }
    }
}
