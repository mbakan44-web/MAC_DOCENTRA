using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using PromtAiPdfPro.Services;
using System.IO;

namespace PromtAiPdfPro.Views
{
    public partial class ConvertPage : Page
    {
        private PdfService _pdfService = new PdfService();
        private OfficeService _officeService = new OfficeService();
        private LicenseService _licenseService = new LicenseService();

        public ConvertPage()
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

        private void BtnCleanOffice_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show((string)Application.Current.FindResource("Conv_CleanConfirm"), 
                                       (string)Application.Current.FindResource("Msg_Confirm"), 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _officeService.KillOfficeProcesses();
                MessageBox.Show((string)Application.Current.FindResource("Msg_CleanSuccess"), 
                                (string)Application.Current.FindResource("Msg_Success"), 
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // --- Image to PDF ---
        private async void BtnImgBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_ImageFilter"),
                Title = (string)Application.Current.FindResource("Conv_SelectImgTitle"),
                Multiselect = true
            };

            if (openDialog.ShowDialog() == true)
            {
                TxtImgSource.Text = string.Join(", ", openDialog.SafeFileNames);
                
                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                    Title = (string)Application.Current.FindResource("Common_SavePdfTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + ".pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (!_licenseService.ValidateOperation(openDialog.FileNames.Length))
                    {
                        MessageBox.Show("Free version limit exceeded! You can only process up to 5 items after trial. Upgrade to Premium.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool success = await _pdfService.ImagesToPdfAsync(openDialog.FileNames.ToList(), saveDialog.FileName);
                    HandleResult(success, saveDialog.FileName);
                }
            }
        }

        // --- PDF to Image ---
        private async void BtnPdfToImgBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                Title = (string)Application.Current.FindResource("Conv_SelectPdfToImgTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                TxtPdfToImgSource.Text = openDialog.SafeFileName;

                var folderDialog = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = (string)Application.Current.FindResource("Conv_SelectFolderTitle")
                };

                if (folderDialog.ShowDialog() == true)
                {
                    int pages = _pdfService.GetPageCount(openDialog.FileName);
                    if (!_licenseService.ValidateOperation(pages))
                    {
                        MessageBox.Show("Free version limit exceeded! Max 5 pages allowed. Upgrade to Premium.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool success = await _pdfService.PdfToImageAsync(openDialog.FileName, folderDialog.FolderName);
                    HandleResult(success, folderDialog.FolderName);
                }
            }
        }

        // --- Office to PDF ---
        private async void BtnOfficeBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_OfficeFilter"),
                Title = (string)Application.Current.FindResource("Conv_SelectOfficeTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                TxtOfficeSource.Text = openDialog.SafeFileName;

                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                    Title = (string)Application.Current.FindResource("Common_SavePdfTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + ".pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    bool success = await _pdfService.OfficeToPdfAsync(openDialog.FileName, saveDialog.FileName);
                    HandleResult(success, saveDialog.FileName);
                }
            }
        }

        // --- PDF to Word ---
        private async void BtnPdfBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                Title = (string)Application.Current.FindResource("Conv_SelectPdfToWordTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                TxtPdfSource.Text = openDialog.SafeFileName;

                var saveDialog = new SaveFileDialog
                {
                    Filter = (string)Application.Current.FindResource("Common_WordFilter"),
                    Title = (string)Application.Current.FindResource("Common_SaveWordTitle"),
                    FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + ".docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    int pages = _pdfService.GetPageCount(openDialog.FileName);
                    if (!_licenseService.ValidateOperation(pages))
                    {
                        MessageBox.Show("Free version limit exceeded! Max 5 pages allowed. Upgrade to Premium.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool success = await _pdfService.PdfToWordAsync(openDialog.FileName, saveDialog.FileName);
                    HandleResult(success, saveDialog.FileName);
                }
            }
        }

        private void HandleResult(bool success, string? outputPath)
        {
            if (success)
            {
                if (MessageBox.Show((string)Application.Current.FindResource("Conv_SuccessWithOpen"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && outputPath != null)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPath) { UseShellExecute = true });
                }
            }
            else
            {
                var msg = (string)Application.Current.FindResource("Conv_Error");
                var title = (string)Application.Current.FindResource("Msg_Error");
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Placeholder for legacy event compatibility if any
        private void BtnImgConvert_Click(object sender, RoutedEventArgs e) { }
        private void BtnPdfToImgConvert_Click(object sender, RoutedEventArgs e) { }
        private void BtnOfficeToPdf_Click(object sender, RoutedEventArgs e) { }
        private void BtnPdfToWord_Click(object sender, RoutedEventArgs e) { }
        private void BtnUniversalConvert_Click(object sender, RoutedEventArgs e) { }
    }
}
