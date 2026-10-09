using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using PromtAiPdfPro.Services;
using System.Threading.Tasks;

namespace PromtAiPdfPro.Views
{
    public partial class PdfAreaPage : Page
    {
        private PdfService _pdfService = new PdfService();

        public PdfAreaPage()
        {
            InitializeComponent();
            LoadRecentFiles();
        }

        private void LoadRecentFiles()
        {
            RecentFilesContainer.Children.Clear();
            var files = Services.RecentFilesService.GetRecentFiles();

            if (files.Count == 0)
            {
                TxtNoRecent.Visibility = Visibility.Visible;
                BtnClearAll.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoRecent.Visibility = Visibility.Collapsed;
                BtnClearAll.Visibility = Visibility.Visible;

                foreach (var file in files)
                {
                    var row = CreateFileRow(file);
                    RecentFilesContainer.Children.Add(row);
                    
                    // Önizlemeyi arka planda yükle
                    LoadThumbnailAsync(file.FullPath, row);
                }
            }
        }

        private async void LoadThumbnailAsync(string path, UIElement row)
        {
            try
            {
                if (!File.Exists(path)) return;
                
                var thumb = await _pdfService.GetPdfThumbnailAsync(path);
                if (thumb != null)
                {
                    // Satır içindeki Image kontrolünü bul ve güncelle
                    if (row is Border border && border.Child is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is Border thumbBorder && thumbBorder.Child is Grid thumbGrid)
                            {
                                foreach (var subChild in thumbGrid.Children)
                                {
                                    if (subChild is System.Windows.Controls.Image img)
                                    {
                                        img.Source = thumb;
                                        img.Visibility = Visibility.Visible;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private UIElement CreateFileRow(Services.RecentFileItem file)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(12, 10, 15, 10),
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = (System.Windows.Media.Brush)Application.Current.FindResource("ControlFillColorDefaultBrush")
            };
            border.SetResourceReference(Border.BorderBrushProperty, "ControlStrokeColorDefaultBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Thumbnail
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Info
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Date

            // Thumbnail Container
            var thumbContainer = new Border
            {
                Width = 45,
                Height = 60,
                CornerRadius = new CornerRadius(4),
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("ControlStrokeColorDefaultBrush"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 15, 0),
                ClipToBounds = true
            };

            var thumbGrid = new Grid();
            // Varsayılan İkon (Yüklenene kadar görünür)
            var placeholderIcon = new Wpf.Ui.Controls.SymbolIcon
            {
                Symbol = Wpf.Ui.Controls.SymbolRegular.DocumentPdf24,
                FontSize = 20,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("PremiumAccentBrush"),
                Opacity = 0.3
            };
            // Gerçek Önizleme
            var previewImg = new System.Windows.Controls.Image
            {
                Stretch = System.Windows.Media.Stretch.UniformToFill,
                Visibility = Visibility.Collapsed
            };
            
            thumbGrid.Children.Add(placeholderIcon);
            thumbGrid.Children.Add(previewImg);
            thumbContainer.Child = thumbGrid;
            Grid.SetColumn(thumbContainer, 0);

            // Info
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var nameTxt = new TextBlock
            {
                Text = file.FileName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            var pathTxt = new TextBlock
            {
                Text = file.FullPath,
                FontSize = 11,
                Opacity = 0.5,
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(nameTxt);
            infoStack.Children.Add(pathTxt);
            Grid.SetColumn(infoStack, 1);

            // Date
            var dateTxt = new TextBlock
            {
                Text = file.LastModified.ToString("d MMM"),
                FontSize = 11,
                Opacity = 0.7,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            Grid.SetColumn(dateTxt, 2);

            grid.Children.Add(thumbContainer);
            grid.Children.Add(infoStack);
            grid.Children.Add(dateTxt);

            border.Child = grid;

            // Events
            border.MouseEnter += (s, e) => border.SetResourceReference(Border.BackgroundProperty, "SubtleFillColorSecondaryBrush");
            border.MouseLeave += (s, e) => border.SetResourceReference(Border.BackgroundProperty, "ControlFillColorDefaultBrush");
            border.MouseLeftButtonUp += (s, e) => OpenFile(file.FullPath);

            return border;
        }

        private void OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            if (File.Exists(filePath))
            {
                Services.RecentFilesService.AddFile(filePath);

                if (PdfReaderWindow.Instance != null)
                {
                    PdfReaderWindow.Instance.AddNewTab(filePath);
                    PdfReaderWindow.Instance.Activate();
                }
                else
                {
                    var viewer = new PdfReaderWindow(filePath);
                    viewer.Show();
                }
                LoadRecentFiles();
            }
            else
            {
                MessageBox.Show((string)Application.Current.FindResource("Msg_Error") ?? "File not found.");
                Services.RecentFilesService.RemoveFile(filePath);
                LoadRecentFiles();
            }
        }

        private void BtnOpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void BtnClearRecent_Click(object sender, RoutedEventArgs e)
        {
            Services.RecentFilesService.ClearAll();
            LoadRecentFiles();
        }

        private void Card_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void Card_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    OpenFile(files[0]);
                }
            }
        }
    }
}
