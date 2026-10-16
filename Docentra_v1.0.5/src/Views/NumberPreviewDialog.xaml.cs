using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PromtAiPdfPro.Views
{
    public partial class NumberPreviewDialog : Wpf.Ui.Controls.FluentWindow
    {
        public bool Result { get; private set; }

        public NumberPreviewDialog(string format, string font, double size, string color, string position, ImageSource background = null)
        {
            InitializeComponent();
            Result = false;

            Loaded += (s, e) =>
            {
                // Formatı kullanıcıya gösterilecek şekilde ayarla (# -> 1)
                string previewText = format.Contains("#") ? format.Replace("#", "1") : $"{format} 1";
                SampleNumber.Text = previewText;
                
                try
                {
                    SampleNumber.FontFamily = new FontFamily(font);
                    SampleNumber.FontSize = size * 1.5; // Scale for screen visibility
                    SampleNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                }
                catch { }

                // Force layout update before positioning
                SampleNumber.UpdateLayout();
                UpdatePosition(position);
            };

            // Also update on size change to be sure
            PreviewCanvas.SizeChanged += (s, e) => UpdatePosition(position);
        }

        private void UpdatePosition(string position)
        {
            double canvasW = PreviewCanvas.ActualWidth;
            double canvasH = PreviewCanvas.ActualHeight;
            double textW = SampleNumber.ActualWidth;
            double textH = SampleNumber.ActualHeight;
            double margin = 20;

            switch (position)
            {
                case "TopLeft":
                    Canvas.SetLeft(SampleNumber, margin);
                    Canvas.SetTop(SampleNumber, margin);
                    break;
                case "TopCenter":
                    Canvas.SetLeft(SampleNumber, (canvasW - textW) / 2);
                    Canvas.SetTop(SampleNumber, margin);
                    break;
                case "TopRight":
                    Canvas.SetLeft(SampleNumber, canvasW - textW - margin);
                    Canvas.SetTop(SampleNumber, margin);
                    break;
                case "BottomLeft":
                    Canvas.SetLeft(SampleNumber, margin);
                    Canvas.SetTop(SampleNumber, canvasH - textH - margin);
                    break;
                case "BottomCenter":
                    Canvas.SetLeft(SampleNumber, (canvasW - textW) / 2);
                    Canvas.SetTop(SampleNumber, canvasH - textH - margin);
                    break;
                case "BottomRight":
                    Canvas.SetLeft(SampleNumber, canvasW - textW - margin);
                    Canvas.SetTop(SampleNumber, canvasH - textH - margin);
                    break;
            }
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
