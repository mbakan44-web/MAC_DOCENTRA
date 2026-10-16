using System;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PdfiumViewer;

namespace PromtAiPdfPro.Views
{
    public class PreviewPageItem
    {
        public BitmapImage Image { get; set; }
        public int PageNumber { get; set; }
    }

    public partial class PrintPreviewWindow : Wpf.Ui.Controls.FluentWindow
    {
        private PdfDocument _pdfDoc;
        public ObservableCollection<PreviewPageItem> PreviewPages { get; set; } = new ObservableCollection<PreviewPageItem>();

        public PrintPreviewWindow(PdfDocument document)
        {
            InitializeComponent();
            _pdfDoc = document;
            
            // Veri Bağlama (Binding) için ItemsSource ayarlaması
            PreviewItemsControl.ItemsSource = PreviewPages;
            
            LoadPrinters();
            GeneratePreviewAsync();
        }

        private void LoadPrinters()
        {
            try
            {
                var printers = PrinterSettings.InstalledPrinters;
                foreach (string printer in printers)
                {
                    CmbPrinters.Items.Add(printer);
                }

                if (CmbPrinters.Items.Count > 0)
                {
                    // Varsayılan yazıcıyı otomatik seç
                    var settings = new PrinterSettings();
                    foreach (string printer in CmbPrinters.Items)
                    {
                        if (printer == settings.PrinterName)
                        {
                            CmbPrinters.SelectedItem = printer;
                            break;
                        }
                    }
                    if (CmbPrinters.SelectedItem == null)
                        CmbPrinters.SelectedIndex = 0;
                }
                
                LoadPaperSizes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Yazıcı yükleme hatası: " + ex.Message);
            }
        }

        private void CmbPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPaperSizes();
        }

        private void LoadPaperSizes()
        {
            if (CmbPrinters.SelectedItem == null) return;
            
            CmbPaperSize.Items.Clear();
            try
            {
                var settings = new PrinterSettings { PrinterName = CmbPrinters.SelectedItem.ToString() };
                foreach (PaperSize paperSize in settings.PaperSizes)
                {
                    CmbPaperSize.Items.Add(paperSize.PaperName);
                }
                if (CmbPaperSize.Items.Count > 0)
                    CmbPaperSize.SelectedIndex = 0;
            }
            catch { }
        }

        private async void GeneratePreviewAsync()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            PreviewItemsControl.Visibility = Visibility.Collapsed;

            // Çok uzun PDF'lerde UI'ın donmasını engellemek için ilk 50 sayfayı ön izlemede gösteriyoruz
            int maxPreviewPages = Math.Min(_pdfDoc.PageCount, 50);

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < maxPreviewPages; i++)
                    {
                        var size = _pdfDoc.PageSizes[i];
                        
                        // Ön izleme için yeterli olan orta kalite bir çözünürlük (150 DPI) kullanıyoruz
                        int renderWidth = (int)(size.Width * (150.0 / 72.0));
                        int renderHeight = (int)(size.Height * (150.0 / 72.0));

                        using (var img = _pdfDoc.Render(i, renderWidth, renderHeight, 150, 150, false))
                        {
                            using (var ms = new MemoryStream())
                            {
                                // Jpeg formatı hızlı işlem (decode/encode) için en iyi seçenektir
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ms.Position = 0;

                                Dispatcher.Invoke(() =>
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.StreamSource = ms;
                                    bitmap.EndInit();
                                    bitmap.Freeze(); // Çapraz-thread (cross-thread) sorunlarını çözer

                                    PreviewPages.Add(new PreviewPageItem { Image = bitmap, PageNumber = i + 1 });
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ön izleme oluşturma hatası: " + ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                PreviewItemsControl.Visibility = Visibility.Visible;
            }
        }

        private void CmbPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtCustomPages == null) return;
            
            if (CmbPages.SelectedIndex == 1) // Özel Aralık
                TxtCustomPages.Visibility = Visibility.Visible;
            else
                TxtCustomPages.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPrinters.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir yazıcı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var printDocument = _pdfDoc.CreatePrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = CmbPrinters.SelectedItem.ToString();
                    printDocument.PrinterSettings.Copies = (short)NumCopies.Value;
                    
                    if (CmbColor.SelectedIndex == 1) // Siyah Beyaz
                        printDocument.DefaultPageSettings.Color = false;
                    else
                        printDocument.DefaultPageSettings.Color = true;

                    // Sayfa aralığı (Temel 1-5 gibi aralıkları destekler)
                    if (CmbPages.SelectedIndex == 1 && !string.IsNullOrWhiteSpace(TxtCustomPages.Text))
                    {
                        try 
                        {
                            var parts = TxtCustomPages.Text.Split('-');
                            if (parts.Length == 2)
                            {
                                printDocument.PrinterSettings.PrintRange = PrintRange.SomePages;
                                printDocument.PrinterSettings.FromPage = int.Parse(parts[0].Trim());
                                printDocument.PrinterSettings.ToPage = int.Parse(parts[1].Trim());
                            }
                        } catch { }
                    }

                    bool isPrinted = false;
                    printDocument.EndPrint += (s, ev) => {
                        if (!ev.Cancel) isPrinted = true;
                    };

                    // Yazdırma İşlemini Başlat
                    printDocument.Print();

                    if (isPrinted)
                    {
                        // Üst düzey programa yakışır, premium bilgilendirme metinleri (Çoklu Dil Uyumlu)
                        string successTitle = Application.Current.TryFindResource("Msg_PrintSuccessTitle") as string ?? "Docentra - İşlem Başarılı";
                        string successMsg = Application.Current.TryFindResource("Msg_PrintSuccess") as string ?? "Belgeniz yüksek kalitede işlendi ve hedefe kusursuz bir şekilde aktarıldı.\n\nİşleminiz başarıyla tamamlanmıştır.";
                        
                        MessageBox.Show(successMsg, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdırma Hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
