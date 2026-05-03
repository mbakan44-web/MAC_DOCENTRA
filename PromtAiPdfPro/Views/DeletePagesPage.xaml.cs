using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class DeletePagesPage : Page
    {
        private PdfService _pdfService = new PdfService();

        public DeletePagesPage()
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
                else
                {
                    PreviewContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSourceFile.Text))
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Warning"), (string)Application.Current.FindResource("Msg_SelectFile"), Wpf.Ui.Controls.ControlAppearance.Caution);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtPageRange.Text))
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Warning"), (string)Application.Current.FindResource("DeletePages_InputLabel"), Wpf.Ui.Controls.ControlAppearance.Caution);
                return;
            }

            // Ask where to save
            var settings = SettingsService.Instance.Current;
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = Path.GetFileNameWithoutExtension(TxtSourceFile.Text) + "_Deleted.pdf",
                InitialDirectory = string.IsNullOrEmpty(settings.DefaultOutputPath) ? "" : settings.DefaultOutputPath
            };

            if (sfd.ShowDialog() != true) return;

            string targetPath = sfd.FileName;
            
            bool success = await _pdfService.DeletePagesAsync(TxtSourceFile.Text, targetPath, TxtPageRange.Text);
            
            if (success)
            {
                ShowMessage((string)Application.Current.FindResource("Msg_Success"), (string)Application.Current.FindResource("Msg_ProcessSuccess"), Wpf.Ui.Controls.ControlAppearance.Success);
                
                // Ask to open
                var result = await (new Wpf.Ui.Controls.MessageBox
                {
                    Title = (string)Application.Current.FindResource("Msg_Success"),
                    Content = (string)Application.Current.FindResource("Delete_SuccessWithOpen"),
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
