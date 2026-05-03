using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using PromtAiPdfPro.Helpers;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class SettingsPage : Page
    {
        // Tüm tema kartlarını tutacak liste (XAML'dan isimle alınacak)
        private readonly List<Border> _themeCards = new();
        private string _selectedTheme = ThemeManager.CurrentTheme;
        private string _defaultOutputPath = "";
        private bool _isInitializing = false;

        // Her kart adını theme key'e map et
        private readonly Dictionary<string, string> _cardToTheme = new()
        {
            { "ThemeCard_Dark",       "Dark"       },
            { "ThemeCard_Light",      "Light"      },
            { "ThemeCard_PastelBlue", "PastelBlue" },
            { "ThemeCard_Lavender",   "Lavender"   },
            { "ThemeCard_Peach",      "Peach"      },
            { "ThemeCard_Mint",       "Mint"       },
            { "ThemeCard_Apricot",    "Apricot"    },
        };

        public SettingsPage()
        {
            InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
            }
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // ── Dil ComboBox ──────────────────────────────────────────
            _isInitializing = true;
            var settingsService = SettingsService.Instance;
            var settings = settingsService.Current;
            string savedLang = settings.Language ?? "Auto";

            foreach (ComboBoxItem item in CboLanguage.Items)
            {
                if (item.Tag?.ToString() == savedLang)
                {
                    CboLanguage.SelectedItem = item;
                    break;
                }
            }
            _isInitializing = false;

            // ── Varsayılan Yol ────────────────────────────────────────
            _defaultOutputPath = settings.DefaultOutputPath ?? "";
            if (!string.IsNullOrEmpty(_defaultOutputPath))
            {
                TxtDefaultPath.Text = _defaultOutputPath;
            }

            // ── Tema Kartları ─────────────────────────────────────────
            _themeCards.Clear();
            foreach (var (name, _) in _cardToTheme)
            {
                if (FindName(name) is Border card)
                {
                    _themeCards.Add(card);
                    card.MouseLeftButtonUp += ThemeCard_Click;
                    // Hover efekti
                    card.MouseEnter += (s, _) =>
                    {
                        if (s is Border b && b.Tag?.ToString() != _selectedTheme)
                            b.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 100, 100));
                    };
                    card.MouseLeave += (s, _) =>
                    {
                        if (s is Border b && b.Tag?.ToString() != _selectedTheme)
                            b.BorderBrush = Brushes.Transparent;
                    };
                }
            }

            // Mevcut temayı seçili göster
            HighlightSelectedCard(_selectedTheme);
        }

        private void ThemeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border card) return;
            var themeKey = card.Tag?.ToString();
            if (string.IsNullOrEmpty(themeKey)) return;

            _selectedTheme = themeKey;
            HighlightSelectedCard(themeKey);

            // Anında önizleme (kaydetmeden)
            ((App)Application.Current).ApplyTheme(themeKey);
        }

        private void HighlightSelectedCard(string themeKey)
        {
            var accentColor = (SolidColorBrush)Application.Current.Resources["PremiumAccentBrush"];

            foreach (var card in _themeCards)
            {
                if (card.Tag?.ToString() == themeKey)
                {
                    card.BorderBrush = accentColor ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
                    card.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Black,
                        BlurRadius = 12,
                        ShadowDepth = 0,
                        Opacity = 0.35
                    };
                }
                else
                {
                    card.BorderBrush = Brushes.Transparent;
                    card.Effect = null;
                }
            }
        }

        private void CboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboLanguage == null || !this.IsLoaded || _isInitializing) return;
            if (CboLanguage.SelectedItem is ComboBoxItem langItem && langItem.Tag != null)
            {
                string locale = langItem.Tag.ToString()!;
                string localeToApply = locale;

                if (locale == "Auto")
                {
                    var systemLocale = System.Globalization.CultureInfo.CurrentUICulture.Name;
                    if (systemLocale.StartsWith("tr", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "tr-TR";
                    else if (systemLocale.StartsWith("es", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "es-ES";
                    else if (systemLocale.StartsWith("de", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "de-DE";
                    else if (systemLocale.StartsWith("fr", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "fr-FR";
                    else if (systemLocale.StartsWith("it", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "it-IT";
                    else if (systemLocale.StartsWith("ru", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "ru-RU";
                    else if (systemLocale.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "ar-SA";
                    else if (systemLocale.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "zh-CN";
                    else if (systemLocale.StartsWith("ja", System.StringComparison.OrdinalIgnoreCase)) localeToApply = "ja-JP";
                    else localeToApply = "en-US";
                }

                // Merkezi ayarları anında güncelle (Sayfa reload olursa kaybolmasın)
                SettingsService.Instance.Current.Language = locale;
                
                ((App)Application.Current).SetLanguage(localeToApply);
            }
        }

        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _defaultOutputPath = dialog.SelectedPath;
                    TxtDefaultPath.Text = _defaultOutputPath;
                }
            }
        }

        private void BtnCleanLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("crash_log.txt")) File.Delete("crash_log.txt");
                
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var tempFolder = Path.Combine(appData, "PromtAiPdfPro", "Temp");
                if (Directory.Exists(tempFolder))
                {
                    var files = Directory.GetFiles(tempFolder);
                    foreach (var f in files) try { File.Delete(f); } catch { }
                }

                MessageBox.Show((string)Application.Current.FindResource("Msg_SystemCleanSuccess"), 
                                (string)Application.Current.FindResource("Msg_Success"), 
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            var settingsService = SettingsService.Instance;

            // 1. Dil Kaydet
            if (CboLanguage.SelectedItem is ComboBoxItem langItem && langItem.Tag != null)
            {
                string locale = langItem.Tag.ToString()!;
                settingsService.Current.Language = locale;
            }

            // 2. Tema Kaydet (settings.json'a)
            settingsService.Current.Theme = _selectedTheme;
            
            // 3. Yol Kaydet
            settingsService.Current.DefaultOutputPath = _defaultOutputPath;
            
            settingsService.SaveSettings();

            // 3. Temayı Uygula (SetLanguage WPF-UI'ı reset edebilir)
            app.ApplyTheme(_selectedTheme);

            // 4. Bildirim
            var msg   = (string)Application.Current.FindResource("Msg_SettingsSaved");
            var title = (string)Application.Current.FindResource("Msg_Success");
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnWebsite_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.docentrapdf.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open website: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
