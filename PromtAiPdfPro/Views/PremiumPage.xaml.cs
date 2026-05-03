using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class PremiumPage : Page
    {
        private readonly LicenseService _licenseService = new LicenseService();

        public PremiumPage()
        {
            InitializeComponent();
            LoadLicenseInfo();
            _ = LoadPricingAsync();
        }

        public class PlanInfo
        {
            public string Name { get; set; }
            public string Price { get; set; }
            public string Description { get; set; }
        }

        private async Task LoadPricingAsync()
        {
            try 
            {
                // WebView2 için özel bir veri klasörü oluştur
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userDataFolder = Path.Combine(localAppData, "DocentraPDF", "WebView2_Cache");
                
                if (!Directory.Exists(userDataFolder))
                    Directory.CreateDirectory(userDataFolder);

                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WvPricing.EnsureCoreWebView2Async(env);
                
                // Zoom seviyesini ayarla (%80 - Daha okunaklı)
                WvPricing.ZoomFactor = 0.8;

                // 1. Sertifika hatalarını görmezden gel
                WvPricing.CoreWebView2.ServerCertificateErrorDetected += (s, args) =>
                {
                    if (args.RequestUri.Contains("docentrapdf.com"))
                        args.Action = Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };

                // 2. Butona tıklandığında Etsy yerine zorla Web Sitesine yönlendir
                WvPricing.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    if (args.Uri != null && !args.Uri.EndsWith("#pricing") && args.Uri != "https://docentrapdf.com/")
                    {
                        args.Cancel = true;

                        // HWID'yi otomatik kopyala
                        try
                        {
                            string hwid = TxtHwid.Text;
                            Clipboard.SetText(hwid);

                            if (Application.Current.MainWindow is MainView mv && mv.SnackbarService != null)
                            {
                                mv.SnackbarService.Show("HWID Kopyalandı!", "Cihaz kimliğiniz kopyalandı. Lütfen Etsy mesaj kutusuna yapıştırarak bize gönderin.", Wpf.Ui.Controls.ControlAppearance.Success, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ClipboardTask24), TimeSpan.FromSeconds(8));
                            }
                        }
                        catch { }

                        // Etsy'ye veya ana siteye yönlendir
                        string targetUrl = "https://www.docentrapdf.com"; // Burası daha sonra Etsy linki ile güncellenebilir
                        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(targetUrl) { UseShellExecute = true }); }
                        catch { }
                    }
                };

                // 3. Yeni pencere isteklerini ana siteye yönlendir
                WvPricing.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://www.docentrapdf.com") { UseShellExecute = true });
                };

                // URL'yi yükle
                WvPricing.Source = new Uri("https://docentrapdf.com/#pricing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WebView2 Error: " + ex.Message);
            }
        }

        private async void WvPricing_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string script = @"
                    (function() {
                        var attempt = 0;
                        var checkExist = setInterval(function() {
                            var pricing = document.getElementById('pricing') || document.querySelector('.pricing-section');
                            attempt++;
                            if (pricing || attempt > 50) {
                                clearInterval(checkExist);
                                if (pricing) {
                                    // 1. Üst hiyerarşiyi koru
                                    var current = pricing;
                                    while (current && current !== document.body) {
                                        var parent = current.parentElement;
                                        if (parent) {
                                            var children = parent.children;
                                            for (var i = 0; i < children.length; i++) {
                                                if (children[i] !== current) children[i].style.display = 'none';
                                            }
                                            parent.style.display = 'block';
                                            parent.style.margin = '0';
                                            parent.style.padding = '0';
                                        }
                                        current = parent;
                                    }

                                    // 2. Arka planı koyu yap (Beyaz yazıların görünmesi için)
                                    document.body.style.backgroundColor = '#0b0f19'; // Koyu zemin
                                    document.body.style.color = '#ffffff';
                                    document.body.style.overflow = 'hidden';
                                    
                                    pricing.style.display = 'block';
                                    pricing.style.margin = '30px auto 0 auto'; // Üstten 30px boşluk
                                    pricing.style.padding = '20px';
                                    pricing.style.opacity = '1';
                                    pricing.style.visibility = 'visible';
                                    
                                    // 3. Yazıların görünürlüğünü zorla
                                    var allText = pricing.querySelectorAll('h1, h2, h3, p, span, li, div');
                                    allText.forEach(el => {
                                        if (window.getComputedStyle(el).color === 'rgb(255, 255, 255)' || el.style.color === 'white') {
                                            el.style.opacity = '1';
                                            el.style.visibility = 'visible';
                                        }
                                    });

                                    var reveals = pricing.querySelectorAll('.reveal, .pricing-card, [data-aos]');
                                    reveals.forEach(el => {
                                        el.style.opacity = '1';
                                        el.style.visibility = 'visible';
                                        el.style.transform = 'none';
                                        el.classList.add('revealed');
                                    });

                                    document.querySelectorAll('header, footer, nav').forEach(el => el.style.display = 'none');
                                }
                            }
                        }, 100);
                    })();
                ";
                await WvPricing.ExecuteScriptAsync(script);
                PricingLoading.Visibility = Visibility.Collapsed;
                WvPricing.Visibility = Visibility.Visible;
            }
            else
            {
                PricingLoading.Visibility = Visibility.Collapsed;
                if (Application.Current.MainWindow is MainView mv && mv.SnackbarService != null)
                {
                    string errorDetail = e.WebErrorStatus.ToString();
                    mv.SnackbarService.Show("Bağlantı Hatası", $"Fiyatlar yüklenemedi. (Hata: {errorDetail})", Wpf.Ui.Controls.ControlAppearance.Caution, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.WifiWarning24), TimeSpan.FromSeconds(7));
                }
            }
        }

        private async void LoadLicenseInfo()
        {
            var status = await _licenseService.CheckLicenseAsync();
            TxtHwid.Text = status.DeviceId;
            
            if (status.IsPremium)
            {
                TxtLicenseKey.Text = "********-********";
                TxtLicenseKey.IsEnabled = false;
                BtnActivate.IsEnabled = false;
                BtnActivate.Content = Application.Current.FindResource("Prem_ActivatedSuccess") ?? "Premium Active";
                
                PremiumStatusInfoBar.IsOpen = true;
                BtnHowToActivate.Visibility = Visibility.Collapsed;
            }
            else
            {
                PremiumStatusInfoBar.IsOpen = false;
                BtnHowToActivate.Visibility = Visibility.Visible;
            }
        }

        private void BtnCopyHwid_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TxtHwid.Text);
            if (Application.Current.MainWindow is MainView mv)
            {
                mv.SnackbarService.Show("Copied", "Hardware ID copied to clipboard.", Wpf.Ui.Controls.ControlAppearance.Info, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.Copy24), TimeSpan.FromSeconds(2));
            }
        }

        private void BtnHowToActivate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcıya kolaylık olması için HWID'yi kopyala
                Clipboard.SetText(TxtHwid.Text);

                // Uygulama içi şık rehber penceresini oluştur ve aç
                var helpWindow = new ActivationHelpWindow();
                helpWindow.Owner = Window.GetWindow(this); // Ana pencerenin üstünde kalması için
                helpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Guide window error: {ex.Message}");
            }
        }

        private async void BtnActivate_Click(object sender, RoutedEventArgs e)
        {
            string key = TxtLicenseKey.Text?.Trim();
            if (string.IsNullOrEmpty(key)) return;

            BtnActivate.IsEnabled = false;
            
            bool success = _licenseService.ActivatePremium(key);
            
            if (Application.Current.MainWindow is MainView mv)
            {
                if (success)
                {
                    string successTitle = Application.Current.FindResource("Msg_Success") as string ?? "Success";
                    string successMsg = Application.Current.FindResource("Prem_ActivatedSuccess") as string ?? "Premium activated!";
                    string thankYou = Application.Current.FindResource("Prem_ThankYou") as string ?? "Thank you for your purchase!";

                    mv.SnackbarService.Show(successTitle, $"{successMsg} {thankYou}", Wpf.Ui.Controls.ControlAppearance.Success, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24), TimeSpan.FromSeconds(5));
                    
                    LoadLicenseInfo();
                    mv.RefreshLocalization(); 
                }
                else
                {
                    mv.SnackbarService.Show("Failed", "Invalid license key for this device.", Wpf.Ui.Controls.ControlAppearance.Danger, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
                    BtnActivate.IsEnabled = true;
                }
            }
        }
    }
}
