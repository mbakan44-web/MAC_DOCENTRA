using System.Windows;
using Wpf.Ui.Controls;

namespace PromtAiPdfPro.Views
{
    public partial class ActivationHelpWindow : Window
    {
        public ActivationHelpWindow()
        {
            InitializeComponent();
            ShowLocalLanguage();
        }

        private void ShowLocalLanguage()
        {
            // 1. Uygulama ayarlarından seçili dili al
            string selectedLang = PromtAiPdfPro.Services.SettingsService.Instance.Current.Language;

            // 2. Eğer "Auto" ise sistem diline bak
            if (string.IsNullOrEmpty(selectedLang) || selectedLang == "Auto")
            {
                selectedLang = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            }

            // 3. Dili temizle (tr-TR -> tr gibi)
            string langCode = selectedLang.Split('-')[0].ToLower();

            // Tüm kartları gizle
            Lang_TR.Visibility = Visibility.Collapsed;
            Lang_EN.Visibility = Visibility.Collapsed;
            Lang_DE.Visibility = Visibility.Collapsed;
            Lang_FR.Visibility = Visibility.Collapsed;
            Lang_ES.Visibility = Visibility.Collapsed;
            Lang_IT.Visibility = Visibility.Collapsed;
            Lang_RU.Visibility = Visibility.Collapsed;
            Lang_AR.Visibility = Visibility.Collapsed;
            Lang_JA.Visibility = Visibility.Collapsed;
            Lang_ZH.Visibility = Visibility.Collapsed;

            // Seçili dili göster
            switch (langCode)
            {
                case "tr": Lang_TR.Visibility = Visibility.Visible; break;
                case "de": Lang_DE.Visibility = Visibility.Visible; break;
                case "fr": Lang_FR.Visibility = Visibility.Visible; break;
                case "es": Lang_ES.Visibility = Visibility.Visible; break;
                case "it": Lang_IT.Visibility = Visibility.Visible; break;
                case "ru": Lang_RU.Visibility = Visibility.Visible; break;
                case "ar": Lang_AR.Visibility = Visibility.Visible; break;
                case "ja": Lang_JA.Visibility = Visibility.Visible; break;
                case "zh": Lang_ZH.Visibility = Visibility.Visible; break;
                default:   Lang_EN.Visibility = Visibility.Visible; break; 
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
