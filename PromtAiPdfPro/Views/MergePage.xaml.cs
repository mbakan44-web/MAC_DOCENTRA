using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class MergePage : Page
    {
        private ObservableCollection<string> _files = new ObservableCollection<string>();
        private PdfService _pdfService = new PdfService();
        private LicenseService _licenseService = new LicenseService();

        public MergePage()
        {
            InitializeComponent();
            LstFiles.ItemsSource = _files;
            _files.CollectionChanged += (s, e) => UpdateEmptyState();
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            EmptyState.Visibility = _files.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".pdf")
                    {
                        if (!_files.Contains(file))
                            _files.Add(file);
                    }
                }
            }
            e.Handled = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    if (!_files.Contains(file))
                        _files.Add(file);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _files.Clear();
        }

        private async void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (_files.Count < 2)
            {
                MessageBox.Show((string)Application.Current.FindResource("Msg_SelectAtLeastTwo"));
                return;
            }

            var settings = SettingsService.Instance.Current;
            var dialog = new SaveFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter"),
                FileName = (string)Application.Current.FindResource("Merge_DefaultFileName"),
                InitialDirectory = string.IsNullOrEmpty(settings.DefaultOutputPath) ? "" : settings.DefaultOutputPath
            };

            if (dialog.ShowDialog() == true)
            {
                // Sayfa sınırı kontrolü (14 günden sonra)
                int totalPages = 0;
                foreach (var file in _files)
                {
                    try { totalPages += _pdfService.GetPageCount(file); } catch { }
                }

                if (!_licenseService.ValidateOperation(totalPages))
                {
                    MessageBox.Show("Free version limit exceeded! After 14 days of trial, you can only process up to 5 pages. Please upgrade to Premium to remove limits.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (Application.Current.MainWindow is MainView mv)
                    {
                        mv.RootNavigation.Navigate(typeof(PremiumPage));
                    }
                    return;
                }

                bool success = await _pdfService.MergeFilesAsync(_files.ToList(), dialog.FileName);
                if (success)
                {
                    if (MessageBox.Show((string)Application.Current.FindResource("Merge_SuccessWithOpen"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                    }
                }
                else
                {
                    MessageBox.Show((string)Application.Current.FindResource("Merge_ErrorMsg"), (string)Application.Current.FindResource("Msg_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
