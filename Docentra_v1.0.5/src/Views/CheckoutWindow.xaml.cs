using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PromtAiPdfPro.Views
{
    public partial class CheckoutWindow : Window
    {
        private readonly string _hwid;
        private const string GUMROAD_URL = "https://docentra.gumroad.com/l/docentra?wanted=true";

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

                // Sertifika hatalarını tolere et
                WvCheckout.CoreWebView2.ServerCertificateErrorDetected += (s, args) =>
                {
                    args.Action = Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };

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

                // Sayfa ve her türlü DOM değişimi için otomatik doldurma scriptini ekle
                if (!string.IsNullOrEmpty(_hwid))
                {
                    await WvCheckout.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(GetAutoFillScript());
                }

                // Sayfayı doğrudan sepet/ödeme ekranına yönlendir
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
                try
                {
                    await WvCheckout.ExecuteScriptAsync(GetAutoFillScript());
                }
                catch { }
            }
        }

        private string GetAutoFillScript()
        {
            return $@"
                (function() {{
                    var targetHwid = '{_hwid}';
                    if (!targetHwid) return;

                    function setNativeInputValue(element, val) {{
                        var lastValue = element.value;
                        element.value = val;

                        // React 16+ input tracker uyumluluğu
                        var tracker = element._valueTracker;
                        if (tracker) {{
                            tracker.setValue(lastValue);
                        }}

                        // HTMLInputElement prototype setter üzerinden tetikle (React sentetik olaylarını zorla)
                        var descriptor = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value');
                        if (descriptor && descriptor.set) {{
                            descriptor.set.call(element, val);
                        }}

                        element.dispatchEvent(new Event('input', {{ bubbles: true, cancelable: true }}));
                        element.dispatchEvent(new Event('change', {{ bubbles: true, cancelable: true }}));
                    }}

                    function tryFillHwid() {{
                        var inputs = document.querySelectorAll('input:not([type=""hidden""]):not([type=""checkbox""]):not([type=""radio""])');
                        for (var i = 0; i < inputs.length; i++) {{
                            var el = inputs[i];
                            var ph = (el.getAttribute('placeholder') || '').toLowerCase();
                            var aria = (el.getAttribute('aria-label') || '').toLowerCase();
                            var name = (el.getAttribute('name') || '').toLowerCase();
                            var id = (el.id || '').toLowerCase();

                            // E-posta, kart numarası gibi alakasız alanları kesinlikle atla
                            if (ph.includes('email') || name.includes('email') || id.includes('email') ||
                                ph.includes('card') || name.includes('card') || id.includes('card') ||
                                ph.includes('cvc') || ph.includes('cvv') || ph.includes('discount') || ph.includes('coupon')) {{
                                continue;
                            }}

                            var isHwidField = false;

                            if (ph.includes('hardware id') || ph.includes('hwid') ||
                                aria.includes('hardware id') || aria.includes('hwid') ||
                                name.includes('hwid') || id.includes('hwid')) {{
                                isHwidField = true;
                            }} else {{
                                var container = el.closest('div, fieldset, label, section');
                                if (container && container.textContent && container.textContent.toLowerCase().includes('hardware id')) {{
                                    isHwidField = true;
                                }}
                            }}

                            if (isHwidField) {{
                                if (el.value !== targetHwid) {{
                                    setNativeInputValue(el, targetHwid);
                                    console.log('[Docentra] HWID başarıyla dolduruldu:', targetHwid);
                                }}
                            }}
                        }}
                    }}

                    // 1. Sürekli periyodik kontrol (React re-render veya SPA geçişleri için)
                    setInterval(tryFillHwid, 300);

                    // 2. DOM Mutation Observer
                    if (document.body) {{
                        var obs = new MutationObserver(tryFillHwid);
                        obs.observe(document.body, {{ childList: true, subtree: true }});
                    }} else {{
                        document.addEventListener('DOMContentLoaded', function() {{
                            var obs = new MutationObserver(tryFillHwid);
                            obs.observe(document.body, {{ childList: true, subtree: true }});
                        }});
                    }}

                    // 3. Kullanıcı tıklar veya odaklarsa anında doldur
                    document.addEventListener('focusin', function(e) {{
                        if (e.target && e.target.tagName === 'INPUT') {{
                            tryFillHwid();
                        }}
                    }});
                }})();
            ";
        }

        private void BtnCopyHwid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_hwid);
                MessageBox.Show($"Cihaz Kimliğiniz (HWID) panoya kopyalandı:\n{_hwid}", "Kopyalandı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch { }
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
