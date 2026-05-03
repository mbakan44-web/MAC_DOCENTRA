using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class AddPageNumbersPage : Page
    {
        private PdfService _pdfService = new PdfService();

        public AddPageNumbersPage()
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
            var dialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter")
            };
            if (dialog.ShowDialog() == true)
            {
                TxtSourceFile.Text = dialog.FileName;
                var thumbnail = await _pdfService.GetPdfThumbnailAsync(dialog.FileName);
                if (thumbnail != null)
                {
                    ImgPreview.Source = thumbnail;
                    PreviewContainer.Visibility = Visibility.Visible;
                }
                int count = await _pdfService.GetPageCountAsync(dialog.FileName);
                TxtEndPage.Text = count.ToString();
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string hex)
            {
                TxtColor.Text = hex;
                TxtSelectedColor.Text = hex;
            }
        }

        private void BtnMoreColors_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = dialog.Color;
                    string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    TxtColor.Text = hex;
                    TxtSelectedColor.Text = hex;
                }
            }
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSourceFile.Text))
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Warning"), (string)Application.Current.FindResource("Msg_SelectFile"), Wpf.Ui.Controls.ControlAppearance.Caution);
                return;
            }

            string pos = (ComboPosition.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "BottomCenter";
            string font = (ComboFont.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Arial";
            double.TryParse(TxtFontSize.Text, out double fontSize);
            if (fontSize == 0) fontSize = 12;

            string userText = TxtFormat.Text.Trim();
            string smartFormat = string.IsNullOrEmpty(userText) ? "#" : $"{userText} #";

            // Show Visual Approval Dialog (A4 Blank as requested)
            var previewDialog = new NumberPreviewDialog(smartFormat, font, fontSize, TxtColor.Text, pos, null);
            previewDialog.Owner = Application.Current.MainWindow;
            previewDialog.ShowDialog();

            if (!previewDialog.Result) return; // Kullanıcı onaylamadı

            var saveDialog = new SaveFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                FileName = Path.GetFileNameWithoutExtension(TxtSourceFile.Text) + "_Numbered.pdf"
            };

            if (saveDialog.ShowDialog() != true) return;
            string targetPath = saveDialog.FileName;
            
            // Arka plan formatı: # -> {n}
            string finalFormat = smartFormat.Replace("#", "{n}");
            
            int.TryParse(TxtStartPage.Text, out int startPage);
            int.TryParse(TxtEndPage.Text, out int endPage);
            int.TryParse(TxtStartingValue.Text, out int startingValue);

            bool success = await _pdfService.AddPageNumbersAsync(
                TxtSourceFile.Text, 
                targetPath, 
                startPage == 0 ? 1 : startPage, 
                endPage == 0 ? 1000 : endPage, 
                pos, 
                finalFormat, 
                font, 
                fontSize, 
                TxtColor.Text, 
                30.0, // margin
                startingValue == 0 ? 1 : startingValue
            );
            
            if (success)
            {
                var result = MessageBox.Show(
                    (string)Application.Current.FindResource("Msg_ProcessSuccess") + "\n\n" + (string)Application.Current.FindResource("Msg_AskOpenFile"),
                    (string)Application.Current.FindResource("Msg_Success"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(targetPath) { UseShellExecute = true });
                }
            }
            else
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Error"), (string)Application.Current.FindResource("Msg_ProcessError"), Wpf.Ui.Controls.ControlAppearance.Danger);
            }
        }

        private void ShowMessage(string title, string message, Wpf.Ui.Controls.ControlAppearance appearance)
        {
            if (Application.Current.MainWindow is MainView mv)
            {
                mv.SnackbarService.Show(
                    title,
                    message,
                    appearance,
                    new Wpf.Ui.Controls.SymbolIcon(appearance == Wpf.Ui.Controls.ControlAppearance.Success ? Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24 : Wpf.Ui.Controls.SymbolRegular.Warning24),
                    TimeSpan.FromSeconds(3)
                );
            }
        }
    }
}
