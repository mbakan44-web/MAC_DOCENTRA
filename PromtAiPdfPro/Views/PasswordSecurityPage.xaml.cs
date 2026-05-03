using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class PasswordSecurityPage : Page
    {
        private readonly PdfService _pdfService = new PdfService();
        
        private string _passSourcePdf = string.Empty;
        private string _unlockSourcePdf = string.Empty;

        public PasswordSecurityPage()
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

        #region Password Protection Flow

        private void BtnSelectPdfForPass_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter") };
            if (dialog.ShowDialog() == true)
            {
                _passSourcePdf = dialog.FileName;
                TxtPassSelectedPdf.Text = dialog.SafeFileName;
                TxtPassSelectedPdf.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
                GridPassStep2.IsEnabled = true;
                UpdateProtectButtonState();
            }
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateProtectButtonState();
        }

        private void UpdateProtectButtonState()
        {
            BtnProtectSave.IsEnabled = !string.IsNullOrEmpty(_passSourcePdf) && !string.IsNullOrEmpty(TxtPassword.Password);
        }

        private async void BtnProtect_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter"), FileName = (string)Application.Current.FindResource("Sec_SaveEncryptedFileName") + System.IO.Path.GetFileName(_passSourcePdf) };
            if (saveDialog.ShowDialog() == true)
            {
                bool result = await _pdfService.ProtectPdfAsync(_passSourcePdf, saveDialog.FileName, TxtPassword.Password);
                if (result)
                {
                    if (MessageBox.Show((string)Application.Current.FindResource("Msg_FileEncryptedSuccess"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                    ResetPasswordFlow();
                }
                else
                {
                    MessageBox.Show((string)Application.Current.FindResource("Msg_EncryptionError"), (string)Application.Current.FindResource("Msg_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetPasswordFlow()
        {
            _passSourcePdf = string.Empty;
            TxtPassSelectedPdf.Text = (string)Application.Current.FindResource("Common_PathPlaceholder");
            TxtPassSelectedPdf.Foreground = System.Windows.Media.Brushes.Gray;
            TxtPassword.Password = string.Empty;
            GridPassStep2.IsEnabled = false;
            BtnProtectSave.IsEnabled = false;
        }

        #endregion

        #region PDF Unlock Flow

        private void BtnSelectPdfForUnlock_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter") };
            if (dialog.ShowDialog() == true)
            {
                _unlockSourcePdf = dialog.FileName;
                TxtUnlockSelectedPdf.Text = dialog.SafeFileName;
                TxtUnlockSelectedPdf.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
                GridUnlockStep2.IsEnabled = true;
                UpdateUnlockButtonState();
            }
        }

        private void TxtUnlockPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateUnlockButtonState();
        }

        private void UpdateUnlockButtonState()
        {
            BtnUnlockSave.IsEnabled = !string.IsNullOrEmpty(_unlockSourcePdf) && !string.IsNullOrEmpty(TxtUnlockPassword.Password);
        }

        private async void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog 
            { 
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"), 
                FileName = "Unlocked_" + System.IO.Path.GetFileName(_unlockSourcePdf) 
            };

            if (saveDialog.ShowDialog() == true)
            {
                bool result = await _pdfService.UnlockPdfAsync(_unlockSourcePdf, saveDialog.FileName, TxtUnlockPassword.Password);
                if (result)
                {
                    var successTitle = (string)Application.Current.FindResource("Msg_Success");
                    var successMsg = (string)Application.Current.FindResource("Unlock_SuccessWithOpen");
                    
                    if (MessageBox.Show(successMsg, successTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                    ResetUnlockFlow();
                }
                else
                {
                    var errMsg = (string)Application.Current.FindResource("Msg_UnlockError");
                    var errTitle = (string)Application.Current.FindResource("Msg_Error");
                    MessageBox.Show(errMsg, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetUnlockFlow()
        {
            _unlockSourcePdf = string.Empty;
            TxtUnlockSelectedPdf.Text = (string)Application.Current.FindResource("Common_PathPlaceholder");
            TxtUnlockSelectedPdf.Foreground = System.Windows.Media.Brushes.Gray;
            TxtUnlockPassword.Password = string.Empty;
            GridUnlockStep2.IsEnabled = false;
            BtnUnlockSave.IsEnabled = false;
        }

        #endregion

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string resourceKey)
            {
                var helpText = (string)Application.Current.FindResource(resourceKey);
                var title = (string)Application.Current.FindResource("Nav_PasswordSecurity");
                MessageBox.Show(helpText, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
