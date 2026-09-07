using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Linq;

namespace Docentra_Mac.Views.Dialogs
{
    public partial class WatermarkEditorWindow : Window
    {
        private bool _isDragging = false;
        private Point _clickPosition;
        private double _textLeft = 0;
        private double _textTop = 0;
        private double _pageWidth;
        private double _pageHeight;

        public bool Success { get; private set; }
        public Rect ResultRect { get; private set; }
        public double FontSize { get; private set; }
        public double OpacityValue { get; private set; }
        public double Rotation { get; private set; }
        public string FontColor { get; private set; } = "#808080";
        public bool ApplyToAll { get; private set; } = true;

        public WatermarkEditorWindow()
        {
            InitializeComponent();
        }

        public WatermarkEditorWindow(string text, double pageWidth = 595, double pageHeight = 842) : this()
        {
            DraggableText.Text = text;
            _pageWidth = pageWidth;
            _pageHeight = pageHeight;

            this.Opened += (s, e) =>
            {
                // Center text initially
                _textLeft = (PageCanvas.Bounds.Width - DraggableText.Bounds.Width) / 2;
                _textTop = (PageCanvas.Bounds.Height - DraggableText.Bounds.Height) / 2;
                UpdatePosition();
            };
        }

        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source == DraggableText)
            {
                var point = e.GetCurrentPoint(DraggableText);
                if (point.Properties.IsLeftButtonPressed)
                {
                    _isDragging = true;
                    _clickPosition = e.GetPosition(DraggableText);
                    e.Pointer.Capture(PageCanvas);
                }
            }
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var currentPos = e.GetPosition(PageCanvas);
                _textLeft = currentPos.X - _clickPosition.X;
                _textTop = currentPos.Y - _clickPosition.Y;

                // Simple bounds
                _textLeft = Math.Max(-100, Math.Min(_textLeft, PageCanvas.Bounds.Width));
                _textTop = Math.Max(-100, Math.Min(_textTop, PageCanvas.Bounds.Height));

                UpdatePosition();
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }

        private void UpdatePosition()
        {
            Canvas.SetLeft(DraggableText, _textLeft);
            Canvas.SetTop(DraggableText, _textTop);
        }

        private void Control_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (DraggableText == null || sender is not Slider sld) return;

            if (sld.Name == "SldFontSize")
            {
                DraggableText.FontSize = sld.Value;
                UpdatePosition();
            }
            else if (sld.Name == "SldRotation")
            {
                if (DraggableText.RenderTransform is TransformGroup group)
                {
                    foreach (var transform in group.Children)
                    {
                        if (transform is RotateTransform rotate)
                        {
                            rotate.Angle = sld.Value;
                        }
                    }
                }
            }
            else if (sld.Name == "SldOpacity")
            {
                DraggableText.Opacity = sld.Value / 100.0;
            }
        }

        private void Apply_Click(object? sender, RoutedEventArgs e)
        {
            double scaleX = _pageWidth / PageCanvas.Bounds.Width;
            double scaleY = _pageHeight / PageCanvas.Bounds.Height;

            FontSize = SldFontSize.Value * scaleY;
            ResultRect = new Rect(
                _textLeft * scaleX,
                _textTop * scaleY,
                DraggableText.Bounds.Width * scaleX,
                DraggableText.Bounds.Height * scaleY
            );
            OpacityValue = SldOpacity.Value / 100.0;
            
            double angle = 0;
            if (DraggableText.RenderTransform is TransformGroup group)
            {
                foreach (var transform in group.Children)
                {
                    if (transform is RotateTransform rotate) angle = rotate.Angle;
                }
            }
            Rotation = angle;
            ApplyToAll = true; // Default to true or check a checkbox if exists

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
