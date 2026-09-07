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
        public static string? PendingFilePath { get; set; }

        public ConvertPage()
        {
            InitializeComponent();
            Loaded += ConvertPage_Loaded;
        }

        private void ConvertPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Helpers.NavigationHelper.PendingFilePath))
            {
                string path = Helpers.NavigationHelper.PendingFilePath;
                string? action = Helpers.NavigationHelper.TargetAction;
                Helpers.NavigationHelper.PendingFilePath = null;
                Helpers.NavigationHelper.TargetAction = null;

                if (action == "PdfToWord")
                {
                    ProcessPdfToWord(path);
                }
                else if (action == "ImageToPdf")
                {
                    ProcessImageConversion(new List<string> { path });
                }
                else if (action == "OfficeToPdf")
                {
                    ProcessOfficeConversion(path);
                }
                else
                {
                    SetSourceFile(path);
                }
            }
        }

        private async void ProcessPdfToWord(string path)
        {
            TxtPdfSource.Text = Path.GetFileName(path);

            var saveDialog = new SaveFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_WordFilter"),
                Title = (string)Application.Current.FindResource("Common_SaveWordTitle"),
                FileName = Path.GetFileNameWithoutExtension(path) + ".docx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                int pages = _pdfService.GetPageCount(path);
                if (!_licenseService.ValidateOperation(pages))
                {
                    MessageBox.Show("Free version limit exceeded! Max 5 pages allowed. Upgrade to Premium.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                bool success = await _pdfService.PdfToWordAsync(path, saveDialog.FileName);
                HandleResult(success, saveDialog.FileName);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else if (Application.Current.MainWindow is MainView mainWindow)
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

        public async void SetSourceFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            string ext = Path.GetExtension(filePath).ToLower();
            string[] imgExts = { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
            string[] officeExts = { ".doc", ".docx", ".rtf", ".xls", ".xlsx", ".ppt", ".pptx" };

            if (imgExts.Contains(ext))
            {
                await ProcessImageConversion(new List<string> { filePath });
            }
            else if (officeExts.Contains(ext))
            {
                await ProcessOfficeConversion(filePath);
            }
        }

        private async Task ProcessImageConversion(List<string> filePaths)
        {
            TxtImgSource.Text = string.Join(", ", filePaths.Select(Path.GetFileName));

            var saveDialog = new SaveFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                Title = (string)Application.Current.FindResource("Common_SavePdfTitle"),
                FileName = Path.GetFileNameWithoutExtension(filePaths[0]) + ".pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                if (!_licenseService.ValidateOperation(filePaths.Count))
                {
                    MessageBox.Show("Free version limit exceeded! You can only process up to 5 items after trial. Upgrade to Premium.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                bool success = await _pdfService.ImagesToPdfAsync(filePaths, saveDialog.FileName);
                HandleResult(success, saveDialog.FileName);
            }
        }

        private async Task ProcessOfficeConversion(string filePath)
        {
            TxtOfficeSource.Text = Path.GetFileName(filePath);

            var saveDialog = new SaveFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                Title = (string)Application.Current.FindResource("Common_SavePdfTitle"),
                FileName = Path.GetFileNameWithoutExtension(filePath) + ".pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                bool success = await _pdfService.OfficeToPdfAsync(filePath, saveDialog.FileName);
                HandleResult(success, saveDialog.FileName);
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
                await ProcessImageConversion(openDialog.FileNames.ToList());
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
                await ProcessOfficeConversion(openDialog.FileName);
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
