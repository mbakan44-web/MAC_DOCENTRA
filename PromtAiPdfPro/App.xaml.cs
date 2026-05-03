using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using PromtAiPdfPro.Helpers;

namespace PromtAiPdfPro
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try {
                base.OnStartup(e);

                // Küresel Hata Yakalama
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // 1. Ayarları Yükle
                var settingsService = Services.SettingsService.Instance;
                var settings = settingsService.Current;

                // 2. Dil Belirleme
                string savedLanguage = settings.Language ?? "Auto";
                string localeToLoad = savedLanguage;

                if (savedLanguage == "Auto")
                {
                    var systemLocale = System.Globalization.CultureInfo.CurrentUICulture.Name;
                    
                    if (systemLocale.StartsWith("tr", StringComparison.OrdinalIgnoreCase)) localeToLoad = "tr-TR";
                    else if (systemLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase)) localeToLoad = "es-ES";
                    else if (systemLocale.StartsWith("de", StringComparison.OrdinalIgnoreCase)) localeToLoad = "de-DE";
                    else if (systemLocale.StartsWith("fr", StringComparison.OrdinalIgnoreCase)) localeToLoad = "fr-FR";
                    else if (systemLocale.StartsWith("it", StringComparison.OrdinalIgnoreCase)) localeToLoad = "it-IT";
                    else if (systemLocale.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) localeToLoad = "ru-RU";
                    else if (systemLocale.StartsWith("ar", StringComparison.OrdinalIgnoreCase)) localeToLoad = "ar-SA";
                    else if (systemLocale.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) localeToLoad = "zh-CN";
                    else if (systemLocale.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) localeToLoad = "ja-JP";
                    else localeToLoad = "en-US";
                }

                // Dili Uygula
                SetLanguage(localeToLoad);

                // 3. Tema Belirleme
                string savedTheme = settings.Theme ?? "Dark";
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    ApplyTheme(savedTheme);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup Error: " + ex.Message + "\n" + ex.StackTrace);
                LogCrash(ex);
            }
        }


        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash(e.Exception);
            e.Handled = true;
            System.Windows.MessageBox.Show($"Bir hata oluştu: {e.Exception.Message}\n\nDetaylar log dosyasına kaydedildi.", "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogCrash(ex);
        }

        private void LogCrash(Exception ex)
        {
            try
            {
                string log = $"[{System.DateTime.Now}] CRASH: {ex.Message}\n{ex.StackTrace}\n\n";
                if (ex.InnerException != null)
                    log += $"Inner: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n\n";
                System.IO.File.AppendAllText("crash_log.txt", log);
            }
            catch { }
        }

        public void SetLanguage(string locale)
        {
            try
            {
                LanguageManager.CurrentLocale = locale;

                // ResourceDictionary oluştur ve Source'u ayarla
                var newDict = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/Locales/{locale}.xaml", UriKind.Absolute)
                };

                // Temizlik yaparken sadece bizim Locales klasöründen gelenleri kaldır
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                for (int i = mergedDicts.Count - 1; i >= 0; i--)
                {
                    var src = mergedDicts[i].Source;
                    if (src != null)
                    {
                        string uriStr = src.OriginalString.ToLower();
                        if (uriStr.Contains("/locales/") || uriStr.Contains("locales/"))
                        {
                            mergedDicts.RemoveAt(i);
                        }
                    }
                }

                // Yeni sözlüğü ekle
                mergedDicts.Add(newDict);

                // Refresh UI texts
                if (Application.Current.MainWindow is MainView mainView)
                    mainView.RefreshLocalization();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SetLanguage Error: " + ex.Message);
                LogCrash(ex);
            }
        }

        public void ApplyTheme(string themeKey)
        {
            ThemeManager.CurrentTheme = themeKey;

            // 1. WPF-UI temel tema: pastel temalar Light tabanını kullanır
            bool isPastel = themeKey is "PastelBlue" or "Lavender" or "Peach" or "Mint" or "Apricot";
            var baseTheme = (themeKey == "Light" || isPastel) ? ApplicationTheme.Light : ApplicationTheme.Dark;
            ApplicationThemeManager.Apply(baseTheme);

            // 2. Accent ve arka plan renklerini tema'ya göre ayarla
            var res = Application.Current.Resources;

            switch (themeKey)
            {
                case "Dark":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x1C, 0x1C, 0x1C)) { Opacity = 0.6 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
                    break;

                case "Light":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3)) { Opacity = 0.8 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1));
                    break;

                case "PastelBlue":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xDF, 0xEE, 0xF8)) { Opacity = 0.9 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0x15, 0x7A, 0xBE));
                    OverrideWindowBackground(Color.FromRgb(0xE8, 0xF4, 0xFD));
                    break;

                case "Lavender":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xED, 0xE7, 0xF6)) { Opacity = 0.9 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2));
                    OverrideWindowBackground(Color.FromRgb(0xF3, 0xEE, 0xFB));
                    break;

                case "Peach":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFD, 0xED, 0xE7)) { Opacity = 0.9 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0xD3, 0x4F, 0x1E));
                    OverrideWindowBackground(Color.FromRgb(0xFD, 0xF0, 0xEB));
                    break;

                case "Mint":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xE3, 0xF5, 0xED)) { Opacity = 0.9 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0x1B, 0x7A, 0x52));
                    OverrideWindowBackground(Color.FromRgb(0xEA, 0xF7, 0xF1));
                    break;

                case "Apricot":
                    res["GlassBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0xFD, 0xF2, 0xDA)) { Opacity = 0.9 };
                    res["PremiumAccentBrush"]   = new SolidColorBrush(Color.FromRgb(0xC6, 0x7C, 0x00));
                    OverrideWindowBackground(Color.FromRgb(0xFD, 0xF5, 0xE4));
                    break;
            }
        }

        private void OverrideWindowBackground(Color color)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Background = new SolidColorBrush(color);
        }

    }
}
