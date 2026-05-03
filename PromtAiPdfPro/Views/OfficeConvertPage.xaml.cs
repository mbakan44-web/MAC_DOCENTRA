using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class OfficeConvertPage : Page
    {
        private readonly OfficeService _officeService;

        public OfficeConvertPage()
        {
            InitializeComponent();
            _officeService = new OfficeService();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
            }
        }

        private async void CardWordToExcel_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_WordFilter"),
                Title = (string)Application.Current.FindResource("Office_SelectWordTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_ExcelFilter"),
                    Title = (string)Application.Current.FindResource("Office_SaveExcelTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + "_Converted.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    OverlayProcessing.Visibility = Visibility.Visible;
                    
                    bool success = await _officeService.WordToExcelAsync(openDialog.FileName, saveDialog.FileName);
                    
                    OverlayProcessing.Visibility = Visibility.Collapsed;

                    if (success)
                    {
                        var msg = (string)Application.Current.FindResource("Msg_AskOpenFile");
                        var title = (string)Application.Current.FindResource("Msg_Success");
                        if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        var msg = (string)Application.Current.FindResource("Conv_OfficeError");
                        var title = (string)Application.Current.FindResource("Msg_Error");
                        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void CardExcelToWord_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_ExcelFilter"),
                Title = (string)Application.Current.FindResource("Office_SelectExcelTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_WordFilter"),
                    Title = (string)Application.Current.FindResource("Office_SaveWordTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + "_Converted.docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    OverlayProcessing.Visibility = Visibility.Visible;
                    
                    bool success = await _officeService.ExcelToWordAsync(openDialog.FileName, saveDialog.FileName);
                    
                    OverlayProcessing.Visibility = Visibility.Collapsed;

                    if (success)
                    {
                        var msg = (string)Application.Current.FindResource("Msg_AskOpenFile");
                        var title = (string)Application.Current.FindResource("Msg_Success");
                        if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        var msg = (string)Application.Current.FindResource("Conv_OfficeError");
                        var title = (string)Application.Current.FindResource("Msg_Error");
                        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void CardPptToWord_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_OfficeFilter"),
                Title = (string)Application.Current.FindResource("Office_SelectPptTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_WordFilter"),
                    Title = (string)Application.Current.FindResource("Office_SaveWordTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + "_Converted.docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    OverlayProcessing.Visibility = Visibility.Visible;
                    bool success = await _officeService.PowerPointToWordAsync(openDialog.FileName, saveDialog.FileName);
                    OverlayProcessing.Visibility = Visibility.Collapsed;

                    if (success)
                    {
                        var msg = (string)Application.Current.FindResource("Msg_AskOpenFile");
                        var title = (string)Application.Current.FindResource("Msg_Success");
                        if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        var msg = (string)Application.Current.FindResource("Conv_OfficeError");
                        var title = (string)Application.Current.FindResource("Msg_Error");
                        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void CardPptToExcel_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_OfficeFilter"),
                Title = (string)Application.Current.FindResource("Office_SelectPptTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_ExcelFilter"),
                    Title = (string)Application.Current.FindResource("Office_SaveExcelTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + "_Converted.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    OverlayProcessing.Visibility = Visibility.Visible;
                    bool success = await _officeService.PowerPointToExcelAsync(openDialog.FileName, saveDialog.FileName);
                    OverlayProcessing.Visibility = Visibility.Collapsed;

                    if (success)
                    {
                        var msg = (string)Application.Current.FindResource("Msg_AskOpenFile");
                        var title = (string)Application.Current.FindResource("Msg_Success");
                        if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        var msg = (string)Application.Current.FindResource("Conv_OfficeError");
                        var title = (string)Application.Current.FindResource("Msg_Error");
                        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
