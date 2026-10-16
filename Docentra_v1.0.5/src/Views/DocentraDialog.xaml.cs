using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PromtAiPdfPro.Views
{
    public partial class DocentraDialog : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public DocentraDialog(
            string message,
            string title,
            MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            InitializeComponent();

            TxtTitle.Text = string.IsNullOrWhiteSpace(title) ? "Docentra" : title;
            TxtMessage.Text = message;

            ConfigureIcon(icon);
            ConfigureButtons(buttons);

            // Allow dragging the dialog window
            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };

            // Keyboard shortcuts
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    if (BtnCancel.Visibility == Visibility.Visible)
                    {
                        Result = MessageBoxResult.Cancel;
                        Close();
                    }
                    else if (BtnNo.Visibility == Visibility.Visible)
                    {
                        Result = MessageBoxResult.No;
                        Close();
                    }
                    else
                    {
                        Result = MessageBoxResult.OK;
                        Close();
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    if (BtnYes.Visibility == Visibility.Visible)
                    {
                        Result = MessageBoxResult.Yes;
                        Close();
                    }
                    else if (BtnOk.Visibility == Visibility.Visible)
                    {
                        Result = MessageBoxResult.OK;
                        Close();
                    }
                }
            };
        }

        private void ConfigureIcon(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Error:
                    TxtIconSymbol.Text = "❌";
                    IconBadge.Background = new SolidColorBrush(Color.FromRgb(80, 20, 20));
                    break;
                case MessageBoxImage.Warning:
                    TxtIconSymbol.Text = "⚠️";
                    IconBadge.Background = new SolidColorBrush(Color.FromRgb(75, 55, 15));
                    break;
                case MessageBoxImage.Question:
                    TxtIconSymbol.Text = "❓";
                    IconBadge.Background = new SolidColorBrush(Color.FromRgb(20, 50, 80));
                    break;
                case MessageBoxImage.Information:
                default:
                    TxtIconSymbol.Text = "ℹ️";
                    IconBadge.Background = new SolidColorBrush(Color.FromRgb(25, 45, 65));
                    break;
            }
        }

        private void ConfigureButtons(MessageBoxButton buttons)
        {
            BtnOk.Visibility = Visibility.Collapsed;
            BtnYes.Visibility = Visibility.Collapsed;
            BtnNo.Visibility = Visibility.Collapsed;
            BtnCancel.Visibility = Visibility.Collapsed;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnOk.Focus();
                    break;

                case MessageBoxButton.OKCancel:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnOk.Focus();
                    break;

                case MessageBoxButton.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnYes.Focus();
                    break;

                case MessageBoxButton.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnYes.Focus();
                    break;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
