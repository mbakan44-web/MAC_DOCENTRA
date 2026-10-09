using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PromtAiPdfPro.Views
{
    public partial class CheckoutWindow : Window
    {
        private readonly string _hwid;
        private const string GUMROAD_URL = "https://docentra.gumroad.com/l/docentra";

        public bool IsPurchaseCompleted { get; private set; } = false;

        public CheckoutWindow(string hwid)
        {
            InitializeComponent();
            _hwid = hwid ?? string.Empty;
            TxtHwidBadge.Text = _hwid;
            _ = InitializeCheckoutAsync();
        }

        private async Task InitializeCheckoutAsync()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userDataFolder = Path.Combine(localAppData, "DocentraPDF", "WebView2_Checkout");

                if (!Directory.Exists(userDataFolder))
                    Directory.CreateDirectory(userDataFolder);

                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WvCheckout.EnsureCoreWebView2Async(env);

                WvCheckout.ZoomFactor = 0.95;

                // Dinamik olarak sipariş tamamlanma durumunu izle
                WvCheckout.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    if (args.Uri != null)
                    {
                        string lowerUri = args.Uri.ToLowerInvariant();
                        if (lowerUri.Contains("receipt") || lowerUri.Contains("thank_you") || lowerUri.Contains("success"))
                        {
                            IsPurchaseCompleted = true;
                        }
                    }
                };

                // Sayfayı yükle
                WvCheckout.Source = new Uri(GUMROAD_URL);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Checkout WebView2 Error: {ex.Message}");
                CheckoutLoading.Visibility = Visibility.Collapsed;
                MessageBox.Show("Ödeme penceresi yüklenirken bir sorun oluştu. Lütfen tarayıcıda aç butonunu deneyin.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void WvCheckout_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            CheckoutLoading.Visibility = Visibility.Collapsed;
            WvCheckout.Visibility = Visibility.Visible;

            if (e.IsSuccess && !string.IsNullOrEmpty(_hwid))
            {
                // Gumroad formunda "Hardware ID (HWID)" alanını otomatik doldur
                string js = $@"
                    (function() {{
                        var targetHwid = '{_hwid}';
                        function tryFill() {{
                            var inputs = document.querySelectorAll('input, textarea');
                            for (var i = 0; i < inputs.length; i++) {{
                                var el = inputs[i];
                                var ph = (el.getAttribute('placeholder') || '').toLowerCase();
                                var aria = (el.getAttribute('aria-label') || '').toLowerCase();
                                var name = (el.getAttribute('name') || '').toLowerCase();
                                var id = (el.id || '').toLowerCase();

                                if (ph.includes('hardware id') || ph.includes('hwid') ||
                                    aria.includes('hardware id') || aria.includes('hwid') ||
                                    name.includes('hwid') || id.includes('hwid')) {{
                                    if (!el.value || el.value !== targetHwid) {{
                                        el.value = targetHwid;
                                        el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                        el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                        console.log('HWID populated successfully:', targetHwid);
                                    }}
                                    return true;
                                }}
                            }}
                            return false;
                        }}

                        tryFill();
                        var count = 0;
                        var interval = setInterval(function() {{
                            count++;
                            if (tryFill() || count > 20) {{
                                clearInterval(interval);
                            }}
                        }}, 500);
                    }})();
                ";

                try
                {
                    await WvCheckout.ExecuteScriptAsync(js);
                }
                catch { }
            }
        }

        private void BtnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_hwid);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(GUMROAD_URL) { UseShellExecute = true });
            }
            catch { }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
