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
                WvPricing.ZoomFactor = 0.82;

                // 1. Sertifika hatalarını görmezden gel
                WvPricing.CoreWebView2.ServerCertificateErrorDetected += (s, args) =>
                {
                    if (args.RequestUri.Contains("docentrapdf.com"))
                        args.Action = Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };

                // 2. Butona tıklandığında in-app checkout penceresini aç
                WvPricing.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    if (args.Uri != null && 
                        !args.Uri.Equals("https://docentrapdf.com/", StringComparison.OrdinalIgnoreCase) && 
                        !args.Uri.Equals("https://docentrapdf.com", StringComparison.OrdinalIgnoreCase) &&
                        !args.Uri.Contains("docentrapdf.com/#pricing"))
                    {
                        args.Cancel = true;
                        Dispatcher.Invoke(() => OpenCheckout());
                    }
                };

                // 3. Yeni pencere isteklerinde de in-app checkout aç
                WvPricing.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                    Dispatcher.Invoke(() => OpenCheckout());
                };

                // URL'yi yükle
                WvPricing.Source = new Uri("https://docentrapdf.com/");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("WebView2 Error: " + ex.Message);
                PricingLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnBuyInApp_Click(object sender, RoutedEventArgs e)
        {
            OpenCheckout();
        }

        private void OpenCheckout()
        {
            try
            {
                string hwid = TxtHwid.Text;
                if (!string.IsNullOrEmpty(hwid))
                {
                    Clipboard.SetText(hwid);
                }

                var checkoutWin = new CheckoutWindow(hwid);
                Window? ownerWin = Window.GetWindow(this) ?? Application.Current.MainWindow;
                if (ownerWin != null && ownerWin.IsVisible)
                {
                    checkoutWin.Owner = ownerWin;
                }
                checkoutWin.ShowDialog();

                if (checkoutWin.IsPurchaseCompleted)
                {
                    if (Application.Current.MainWindow is MainView mv && mv.SnackbarService != null)
                    {
                        mv.SnackbarService.Show(
                            "Siparişiniz Alındı!",
                            "Ödemeniz başarıyla tamamlandı. Cihaz kimliğiniz (HWID) siparişinize eklendi. Lisans anahtarınız e-posta adresinize gönderilecektir.",
                            Wpf.Ui.Controls.ControlAppearance.Success,
                            new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24),
                            TimeSpan.FromSeconds(10));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Checkout error: {ex.Message}");
                try
                {
                    if (!string.IsNullOrEmpty(TxtHwid.Text))
                        Clipboard.SetText(TxtHwid.Text);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://docentra.gumroad.com/l/docentra") { UseShellExecute = true });
                }
                catch { }
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
                                    // 1. Üst hiyerarşiyi koru ve kardeşleri gizle
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

                                    // 2. Sayfa ve zemin ayarları
                                    document.body.style.backgroundColor = '#0b0f19';
                                    document.body.style.color = '#ffffff';
                                    document.body.style.overflow = 'hidden';
                                    document.body.style.margin = '0';
                                    document.body.style.padding = '0';
                                    
                                    pricing.style.display = 'block';
                                    pricing.style.margin = '0 auto';
                                    pricing.style.padding = '0';
                                    pricing.style.opacity = '1';
                                    pricing.style.visibility = 'visible';

                                    // 3. Bölüm üst başlığını (section-header) gizle - WPF zaten başlığı gösteriyor
                                    var secHeader = pricing.querySelector('.section-header');
                                    if (secHeader) secHeader.style.display = 'none';

                                    // 4. Fiyat kartını tam sığacak şekilde düzenle
                                    var card = pricing.querySelector('.pricing-card');
                                    if (card) {
                                        card.style.margin = '0 auto';
                                        card.style.padding = '18px 24px';
                                        card.style.borderRadius = '20px';
                                        card.style.maxWidth = '100%';
                                        card.style.boxSizing = 'border-box';
                                        card.style.boxShadow = 'none';
                                    }

                                    // Fiyat başlığı ve tutarı (h3, price $19.90, subtitle)
                                    var pHeader = pricing.querySelector('.pricing-header');
                                    if (pHeader) {
                                        pHeader.style.marginBottom = '0.8rem';
                                    }
                                    var priceEl = pricing.querySelector('.price');
                                    if (priceEl) {
                                        priceEl.style.fontSize = '2.8rem';
                                        priceEl.style.margin = '0.3rem 0';
                                        priceEl.style.color = '#ffffff';
                                    }
                                    var pSubtitle = pricing.querySelector('.pricing-header p');
                                    if (pSubtitle) {
                                        pSubtitle.style.fontSize = '0.85rem';
                                        pSubtitle.style.opacity = '0.75';
                                    }

                                    // Özellik maddelerini kompakt yap
                                    var pFeatures = pricing.querySelector('.pricing-features');
                                    if (pFeatures) {
                                        pFeatures.style.marginBottom = '1rem';
                                    }
                                    var lis = pricing.querySelectorAll('.pricing-features li');
                                    lis.forEach(function(li) {
                                        li.style.padding = '0.45rem 0';
                                        li.style.fontSize = '0.9rem';
                                    });

                                    // Satın al butonu ve vergi uyarısı
                                    var btnBuy = pricing.querySelector('.btn-primary');
                                    if (btnBuy) {
                                        btnBuy.style.padding = '0.75rem 1.5rem';
                                        btnBuy.style.fontSize = '1rem';
                                    }
                                    var taxNotice = pricing.querySelector('.tax-notice');
                                    if (taxNotice) {
                                        taxNotice.style.marginTop = '0.5rem';
                                        taxNotice.style.fontSize = '0.75rem';
                                    }

                                    // 5. Görünürlüğü zorla
                                    var allText = pricing.querySelectorAll('h1, h2, h3, p, span, li, div');
                                    allText.forEach(function(el) {
                                        el.style.opacity = '1';
                                        el.style.visibility = 'visible';
                                    });

                                    var reveals = pricing.querySelectorAll('.reveal, .pricing-card, [data-aos]');
                                    reveals.forEach(function(el) {
                                        el.style.opacity = '1';
                                        el.style.visibility = 'visible';
                                        el.style.transform = 'none';
                                        el.classList.add('revealed');
                                    });

                                    document.querySelectorAll('header, footer, nav, canvas').forEach(function(el) {
                                        el.style.display = 'none';
                                    });

                                    // 6. Sayfa kaydırmasını en başa al
                                    window.scrollTo(0, 0);
                                    document.documentElement.scrollTop = 0;
                                    document.body.scrollTop = 0;
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
                BtnBuyInApp.Visibility = Visibility.Collapsed;
            }
            else
            {
                PremiumStatusInfoBar.IsOpen = false;
                BtnHowToActivate.Visibility = Visibility.Visible;
                BtnBuyInApp.Visibility = Visibility.Visible;
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
