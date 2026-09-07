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
        private readonly List<Border> _themeCards = new();
        private string _selectedTheme = ThemeManager.CurrentTheme;
        private string _defaultOutputPath = "";
        private bool _isInitializing = false;

        private readonly Dictionary<string, string> _cardToTheme = new()
        {
            { "ThemeCard_Dark",         "Dark"         },
            { "ThemeCard_Light",        "Light"        },
            { "ThemeCard_PastelBlue",   "PastelBlue"   },
            { "ThemeCard_Lavender",     "Lavender"     },
            { "ThemeCard_Mint",         "Mint"         },
            { "ThemeCard_Apricot",      "Apricot"      },
            { "ThemeCard_HighContrast", "HighContrast" },
        };

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else if (Application.Current.MainWindow is MainView mainWindow)
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
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

                _defaultOutputPath = settings.DefaultOutputPath ?? "";
                if (!string.IsNullOrEmpty(_defaultOutputPath))
                    TxtDefaultPath.Text = _defaultOutputPath;

                _themeCards.Clear();
                foreach (var (name, _) in _cardToTheme)
                {
                    if (FindName(name) is Border card)
                    {
                        _themeCards.Add(card);
                        card.MouseLeftButtonUp += ThemeCard_Click;
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
                HighlightSelectedCard(_selectedTheme);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SettingsPage Load Error: " + ex.Message);
            }
        }

        private void ThemeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border card) return;
            var themeKey = card.Tag?.ToString();
            if (string.IsNullOrEmpty(themeKey)) return;

            _selectedTheme = themeKey;
            HighlightSelectedCard(themeKey);
            ((App)Application.Current).ApplyTheme(themeKey);
        }

        private void HighlightSelectedCard(string themeKey)
        {
            var accentColor = Application.Current.Resources["PremiumAccentBrush"] as SolidColorBrush;

            foreach (var card in _themeCards)
            {
                if (card.Tag?.ToString() == themeKey)
                {
                    card.BorderBrush = accentColor ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
                    card.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Black, BlurRadius = 12, ShadowDepth = 0, Opacity = 0.35 };
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
                    if (systemLocale.StartsWith("tr", StringComparison.OrdinalIgnoreCase)) localeToApply = "tr-TR";
                    else if (systemLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase)) localeToApply = "es-ES";
                    else if (systemLocale.StartsWith("de", StringComparison.OrdinalIgnoreCase)) localeToApply = "de-DE";
                    else if (systemLocale.StartsWith("fr", StringComparison.OrdinalIgnoreCase)) localeToApply = "fr-FR";
                    else if (systemLocale.StartsWith("it", StringComparison.OrdinalIgnoreCase)) localeToApply = "it-IT";
                    else if (systemLocale.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) localeToApply = "ru-RU";
                    else if (systemLocale.StartsWith("ar", StringComparison.OrdinalIgnoreCase)) localeToApply = "ar-SA";
                    else if (systemLocale.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) localeToApply = "zh-CN";
                    else if (systemLocale.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) localeToApply = "ja-JP";
                    else localeToApply = "en-US";
                }

                SettingsService.Instance.Current.Language = locale;
                ((App)Application.Current).SetLanguage(localeToApply);
            }
        }

        private void BtnClean_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("crash_log.txt")) File.WriteAllText("crash_log.txt", "");
                MessageBox.Show(GetLocalizedString("Msg_SystemCleanSuccess", "Cleanup completed!"), 
                                GetLocalizedString("Msg_Success", "Success"), 
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
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

                MessageBox.Show(GetLocalizedString("Msg_SystemCleanSuccess", "Cleanup completed!"), 
                                GetLocalizedString("Msg_Success", "Success"), 
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnRegisterDefault_Click(object sender, RoutedEventArgs e)
        {
            if (Helpers.FileAssociationHelper.RegisterPdfAssociation())
                MessageBox.Show(GetLocalizedString("Msg_Success", "Success!"), "Docentra", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(GetLocalizedString("Msg_Error", "Error!"), "Docentra", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var settingsService = SettingsService.Instance;

            if (CboLanguage.SelectedItem is ComboBoxItem langItem && langItem.Tag != null)
                settingsService.Current.Language = langItem.Tag.ToString()!;

            settingsService.Current.Theme = _selectedTheme;
            settingsService.Current.DefaultOutputPath = _defaultOutputPath;
            settingsService.SaveSettings();

            ((App)Application.Current).ApplyTheme(_selectedTheme);

            // Garantici Mesaj Gösterimi:
            // Doğrudan o anki dilden metni çekiyoruz
            string msg = GetLocalizedString("Msg_SettingsSaved", "Settings saved successfully.");
            string title = GetLocalizedString("Msg_Success", "Success");

            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetLocalizedString(string key, string fallback)
        {
            try
            {
                // 1. Önce doğrudan uygulamanın MergedDictionaries listesinde en üsttekine bakalım
                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.Contains(key)) return dict[key] as string ?? fallback;
                }

                // 2. Bulamazsak genel kaynaklarda ara
                var res = Application.Current.TryFindResource(key) as string;
                if (!string.IsNullOrEmpty(res)) return res;
            }
            catch { }
            return fallback;
        }

        private void BtnWebsite_Click(object sender, MouseButtonEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "https://www.docentrapdf.com", UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}
