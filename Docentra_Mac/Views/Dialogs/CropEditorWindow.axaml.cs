using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace Docentra_Mac.Views.Dialogs
{
    public partial class CropEditorWindow : Window
    {
        private bool _isDragging = false;
        private bool _isResizing = false;
        private Point _clickPosition;
        private double _boxLeft = 125;
        private double _boxTop = 217;
        private double _boxWidth = 200;
        private double _boxHeight = 200;
        private double _pageWidth;
        private double _pageHeight;

        public bool Success { get; private set; }
        public Rect ResultRect { get; private set; }
        public bool ApplyToAll { get; private set; }

        public CropEditorWindow()
        {
            InitializeComponent();
        }

        public CropEditorWindow(double pageWidth = 595, double pageHeight = 842) : this()
        {
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;
        }

        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source == SelectionBox)
            {
                _isDragging = true;
                _clickPosition = e.GetPosition(SelectionBox);
                e.Pointer.Capture(CropCanvas);
            }
        }

        private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isResizing = true;
            _clickPosition = e.GetPosition(this);
            e.Pointer.Capture(CropCanvas);
            e.Handled = true;
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var currentPos = e.GetPosition(CropCanvas);
                _boxLeft = currentPos.X - _clickPosition.X;
                _boxTop = currentPos.Y - _clickPosition.Y;

                // Bounds
                _boxLeft = Math.Max(0, Math.Min(_boxLeft, 450 - _boxWidth));
                _boxTop = Math.Max(0, Math.Min(_boxTop, 635 - _boxHeight));

                UpdateUI();
            }
            else if (_isResizing)
            {
                var currentPos = e.GetPosition(CropCanvas);
                _boxWidth = Math.Max(50, currentPos.X - _boxLeft);
                _boxHeight = Math.Max(50, currentPos.Y - _boxTop);

                // Bounds
                _boxWidth = Math.Min(_boxWidth, 450 - _boxLeft);
                _boxHeight = Math.Min(_boxHeight, 635 - _boxTop);

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
            Canvas.SetLeft(SelectionBox, _boxLeft);
            Canvas.SetTop(SelectionBox, _boxTop);
            SelectionBox.Width = _boxWidth;
            SelectionBox.Height = _boxHeight;
        }

        private void Apply_Click(object? sender, RoutedEventArgs e)
        {
            double scaleX = _pageWidth / 450.0;
            double scaleY = _pageHeight / 635.0;

            ResultRect = new Rect(
                _boxLeft * scaleX,
                _boxTop * scaleY,
                _boxWidth * scaleX,
                _boxHeight * scaleY
            );
            ApplyToAll = ChkApplyToAll.IsChecked ?? true;
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
