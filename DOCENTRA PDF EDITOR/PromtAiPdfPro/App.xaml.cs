using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Wpf.Ui.Appearance;
using PromtAiPdfPro.Helpers;

namespace PromtAiPdfPro
{
    public partial class App : Application
    {
        private static Mutex? _mutex = null;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int dwProcessId);

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "DocentraPdfSuite_SingleInstance_v21";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // İşletim sistemine, arka plandaki asıl uygulamamızın öne gelmesine izin vermesini söylüyoruz.
                // -1 (ASFW_ANY) değeri, herhangi bir sürecin öne gelmesine izin verir.
                AllowSetForegroundWindow(-1);

                // Uygulama zaten açık. Dosya yolunu mevcut örneğe gönder.
                string message = e.Args.FirstOrDefault() ?? "";
                if (e.Args.Contains("-convert") && e.Args.Length > 1)
                {
                    string filePath = e.Args[Array.IndexOf(e.Args, "-convert") + 1];
                    message = "CONVERT|" + filePath;
                }
                else if (e.Args.Contains("-edit") && e.Args.Length > 1)
                {
                    string filePath = e.Args[Array.IndexOf(e.Args, "-edit") + 1];
                    message = "EDIT|" + filePath;
                }

                if (string.IsNullOrEmpty(message))
                {
                    message = "OPEN_MAIN_VIEW";
                }

                if (!string.IsNullOrEmpty(message))
                {
                    // Mesajın gittiğinden emin olmak için senkron bekliyoruz
                    Task.Run(async () => await Services.IpcService.SendMessage(message)).Wait(1000);
                }
                Environment.Exit(0);
                return;
            }

            try {
                // IPC Sunucusunu Başlat
                Services.IpcService.StartServer((message) => {
                    this.Dispatcher.Invoke(() => {
                        if (message.StartsWith("CONVERT|"))
                        {
                            string path = message.Substring("CONVERT|".Length);
                            if (Application.Current.MainWindow is MainView mv)
                            {
                                Views.ConvertPage.PendingFilePath = path;
                                mv.RootNavigation.Navigate(typeof(Views.ConvertPage));
                                ForceForeground(mv);
                            }
                        }
                        else if (message.StartsWith("EDIT|"))
                        {
                            if (Application.Current.MainWindow is MainView mv)
                            {
                                mv.RootNavigation.Navigate(typeof(Views.ControlCenterPage));
                                ForceForeground(mv);
                            }
                        }
                        else if (message == "OPEN_MAIN_VIEW")
                        {
                            if (Application.Current.MainWindow is MainView mv)
                            {
                                ForceForeground(mv);
                            }
                            else
                            {
                                var newMv = new MainView();
                                Application.Current.MainWindow = newMv;
                                newMv.Show();
                                ForceForeground(newMv);
                            }
                        }
                        else
                        {
                            if (Views.PdfReaderWindow.Instance != null)
                            {
                                Views.PdfReaderWindow.Instance.AddNewTab(message);
                                ForceForeground(Views.PdfReaderWindow.Instance);
                            }
                            else
                            {
                                var readerWindow = new Views.PdfReaderWindow(message);
                                readerWindow.Show();
                                ForceForeground(readerWindow);
                            }
                        }
                    });
                });

                base.OnStartup(e);

                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                var settingsService = Services.SettingsService.Instance;
                var settings = settingsService.Current;

                string localeToLoad = GetBestLocale(settings.Language ?? "Auto");
                SetLanguage(localeToLoad);
                
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    ApplyTheme(settings.Theme ?? "Dark");
                });

                bool isConvert = e.Args.Contains("-convert");
                bool isEdit = e.Args.Contains("-edit");
                string? fileToOpen = null;
                
                if (isConvert && e.Args.Length > 1)
                    fileToOpen = e.Args[Array.IndexOf(e.Args, "-convert") + 1];
                else if (isEdit && e.Args.Length > 1)
                    fileToOpen = e.Args[Array.IndexOf(e.Args, "-edit") + 1];
                else
                    fileToOpen = e.Args.FirstOrDefault();

                if (!string.IsNullOrEmpty(fileToOpen))
                {
                    if (isConvert)
                    {
                        Views.ConvertPage.PendingFilePath = fileToOpen;
                        var mainView = new MainView();
                        mainView.Show();
                        mainView.RootNavigation.Navigate(typeof(Views.ConvertPage));
                    }
                    else if (isEdit)
                    {
                        var mainView = new MainView();
                        mainView.Show();
                        mainView.RootNavigation.Navigate(typeof(Views.ControlCenterPage));
                    }
                    else if (System.IO.Path.GetExtension(fileToOpen).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Views.PdfReaderWindow.Instance != null)
                        {
                            Views.PdfReaderWindow.Instance.AddNewTab(fileToOpen);
                            Views.PdfReaderWindow.Instance.Activate();
                        }
                        else
                        {
                            var readerWindow = new Views.PdfReaderWindow(fileToOpen);
                            readerWindow.Show();
                        }
                    }
                    else
                    {
                        var mainView = new MainView();
                        mainView.Show();
                    }
                }
                else
                {
                    var mainView = new MainView();
                    mainView.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup Error: " + ex.Message);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void ForceForeground(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Topmost = true;
            window.Show();
            window.Activate();
            window.Topmost = false;
            window.Focus();

            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            SetForegroundWindow(hwnd);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // IPC Sunucusunu durdur
                Services.IpcService.StopServer();

                // Tüm açık pencereleri zorla kapat
                foreach (Window window in Current.Windows.OfType<Window>().ToList())
                {
                    try { window.Close(); } catch { }
                }

                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch { }

            base.OnExit(e);
            
            // Agresif ve Kesin Çıkış: Bazı native DLL'ler (Pdfium, WebView2) süreci asılı bırakabilir.
            // İşletim sistemine tüm kaynakları temizlemesi için komut veriyoruz.
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private string GetBestLocale(string savedLanguage)
        {
            if (savedLanguage != "Auto") return savedLanguage;

            var systemLocale = System.Globalization.CultureInfo.CurrentUICulture.Name;
            if (systemLocale.StartsWith("tr", StringComparison.OrdinalIgnoreCase)) return "tr-TR";
            if (systemLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return "es-ES";
            if (systemLocale.StartsWith("de", StringComparison.OrdinalIgnoreCase)) return "de-DE";
            if (systemLocale.StartsWith("fr", StringComparison.OrdinalIgnoreCase)) return "fr-FR";
            if (systemLocale.StartsWith("it", StringComparison.OrdinalIgnoreCase)) return "it-IT";
            if (systemLocale.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) return "ru-RU";
            if (systemLocale.StartsWith("ar", StringComparison.OrdinalIgnoreCase)) return "ar-SA";
            if (systemLocale.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return "zh-CN";
            if (systemLocale.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return "ja-JP";
            return "en-US";
        }

        public void SetLanguage(string locale)
        {
            try
            {
                LanguageManager.CurrentLocale = locale;
                
                // Kültür ayarlarını güncelle (Sistem mesajları ve kaynak yönetimi için)
                var culture = new System.Globalization.CultureInfo(locale);
                System.Globalization.CultureInfo.CurrentCulture = culture;
                System.Globalization.CultureInfo.CurrentUICulture = culture;
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

                var newDict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Locales/{locale}.xaml", UriKind.Absolute) };
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                
                // Eski yerelleştirme dosyalarını temizle
                for (int i = mergedDicts.Count - 1; i >= 0; i--)
                {
                    var src = mergedDicts[i].Source;
                    if (src != null && src.OriginalString.ToLower().Contains("/locales/"))
                        mergedDicts.RemoveAt(i);
                }
                
                // En başa ekleyelim ki aramada öncelikli olsun
                mergedDicts.Insert(0, newDict);

                if (Application.Current.MainWindow is MainView mainView)
                    mainView.RefreshLocalization();
            }
            catch (Exception ex) { LogCrash(ex); }
        }

        public void ApplyTheme(string themeKey)
        {
            ThemeManager.CurrentTheme = themeKey;
            bool isPastel = themeKey is "PastelBlue" or "Lavender" or "Mint" or "Apricot";
            var baseTheme = (themeKey == "Light" || isPastel) ? ApplicationTheme.Light : ApplicationTheme.Dark;
            
            if (themeKey == "HighContrast")
                baseTheme = ApplicationTheme.HighContrast;

            ApplicationThemeManager.Apply(baseTheme);

            var res = Application.Current.Resources;
            switch (themeKey)
            {
                case "Dark":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0x1C, 0x1C, 0x1C));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x1C, 0x1C, 0x1C)) { Opacity = 0.6 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
                    break;
                case "Light":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1));
                    break;
                case "PastelBlue":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0xBB, 0xDE, 0xFB));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xBB, 0xDE, 0xFB)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
                    break;
                case "Lavender":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0xE1, 0xBE, 0xE7));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xE1, 0xBE, 0xE7)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0));
                    break;
                case "Mint":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0xC8, 0xE6, 0xC9));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xC8, 0xE6, 0xC9)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                    break;
                case "Apricot":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF1, 0xD7));
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF1, 0xD7)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.FromRgb(0xFB, 0x8C, 0x00));
                    break;
                case "HighContrast":
                    res["DocentraWindowBackground"] = new SolidColorBrush(Colors.Black);
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Colors.Black);
                    res["PremiumAccentBrush"] = new SolidColorBrush(Colors.White);
                    break;
            }

            // Update Window Backdrop for all active windows
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainView mainView)
                {
                    var backgroundBrush = (SolidColorBrush)res["DocentraWindowBackground"];
                    mainView.Background = backgroundBrush;
                    mainView.WindowBackdropType = (isPastel || themeKey == "HighContrast") 
                        ? Wpf.Ui.Controls.WindowBackdropType.None 
                        : Wpf.Ui.Controls.WindowBackdropType.Mica;
                }
                else if (window is Views.PdfReaderWindow readerWindow)
                {
                    var backgroundBrush = (SolidColorBrush)res["DocentraWindowBackground"];
                    readerWindow.Background = backgroundBrush;
                    readerWindow.WindowBackdropType = (isPastel || themeKey == "HighContrast") 
                        ? Wpf.Ui.Controls.WindowBackdropType.None 
                        : Wpf.Ui.Controls.WindowBackdropType.Mica;
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex) LogCrash(ex);
        }

        private void LogCrash(Exception ex)
        {
            try
            {
                string log = $"[{DateTime.Now}] {ex.Message}\n{ex.StackTrace}\n---\n";
                System.IO.File.AppendAllText("crash_log.txt", log);
            }
            catch { }
        }
    }
}
