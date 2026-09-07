using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;
using Wpf.Ui.Controls;
using System.Linq;

namespace PromtAiPdfPro.Views
{
    public partial class MetadataEditorPage : Page
    {
        private string? _currentFilePath;
        private readonly PdfService _pdfService = new PdfService();
        public static string? PendingFilePath { get; set; }

        public MetadataEditorPage()
        {
            InitializeComponent();
            Loaded += MetadataEditorPage_Loaded;
        }

        private async void MetadataEditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PendingFilePath) && File.Exists(PendingFilePath))
            {
                await LoadFileAsync(PendingFilePath);
                PendingFilePath = null;
            }
            else if (!string.IsNullOrEmpty(Helpers.NavigationHelper.PendingFilePath) && File.Exists(Helpers.NavigationHelper.PendingFilePath))
            {
                if (Helpers.NavigationHelper.TargetAction == "Metadata")
                {
                    await LoadFileAsync(Helpers.NavigationHelper.PendingFilePath);
                    Helpers.NavigationHelper.TargetAction = null;
                }
            }
        }

        private async void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Select PDF File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadFileAsync(openFileDialog.FileName);
            }
        }

        private async void FileDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].ToLower().EndsWith(".pdf"))
                {
                    await LoadFileAsync(files[0]);
                }
            }
        }

        private async System.Threading.Tasks.Task LoadFileAsync(string filePath)
        {
            _currentFilePath = filePath;
            TxtSelectedFile.Text = Path.GetFileName(filePath);
            TxtSelectedFile.Foreground = System.Windows.Media.Brushes.MediumSeaGreen;
            TxtSelectedFile.FontWeight = FontWeights.Bold;

            LoadingRing.Visibility = Visibility.Visible;
            SettingsPanel.IsEnabled = false;

            var metadata = await _pdfService.GetMetadataAsync(filePath);
            
            LoadingRing.Visibility = Visibility.Collapsed;
            
            if (metadata != null)
            {
                TxtTitle.Text = metadata.Title;
                TxtAuthor.Text = metadata.Author;
                TxtSubject.Text = metadata.Subject;
                TxtKeywords.Text = metadata.Keywords;
                TxtCreator.Text = metadata.Creator;
                TxtProducer.Text = metadata.Producer;
                TxtCreationDate.Text = string.IsNullOrEmpty(metadata.CreationDate) ? "-" : metadata.CreationDate;
                TxtModificationDate.Text = string.IsNullOrEmpty(metadata.ModificationDate) ? "-" : metadata.ModificationDate;
                
                SettingsPanel.IsEnabled = true;
            }
            else
            {
                MainView? mv = Application.Current.Windows.OfType<MainView>().FirstOrDefault();
                mv?.SnackbarService.Show("Error", "Could not read PDF metadata.", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Save PDF",
                FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + "_metadata.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string targetPath = saveFileDialog.FileName;
                
                var metadata = new Models.PdfMetadataModel
                {
                    Title = TxtTitle.Text,
                    Author = TxtAuthor.Text,
                    Subject = TxtSubject.Text,
                    Keywords = TxtKeywords.Text,
                    Creator = TxtCreator.Text,
                    Producer = TxtProducer.Text
                };

                LoadingRing.Visibility = Visibility.Visible;
                SettingsPanel.IsEnabled = false;

                bool result = await _pdfService.SetMetadataAsync(_currentFilePath, targetPath, metadata);

                LoadingRing.Visibility = Visibility.Collapsed;
                SettingsPanel.IsEnabled = true;

                MainView? mv = Application.Current.Windows.OfType<MainView>().FirstOrDefault();
                if (result)
                {
                    mv?.SnackbarService.Show(
                        Application.Current.TryFindResource("Viewer_SuccessTitle") as string ?? "Success",
                        Application.Current.TryFindResource("Metadata_SuccessMsg") as string ?? "Metadata saved successfully.",
                        ControlAppearance.Success,
                        new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                        TimeSpan.FromSeconds(3)
                    );
                    
                    Services.RecentFilesService.AddFile(targetPath);
                }
                else
                {
                    mv?.SnackbarService.Show(
                        "Error",
                        "Could not save metadata.",
                        ControlAppearance.Danger,
                        new SymbolIcon(SymbolRegular.ErrorCircle24),
                        TimeSpan.FromSeconds(3)
                    );
                }
            }
        }
    }
}
