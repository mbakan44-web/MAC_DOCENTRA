using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Docentra_Mac.Services;
using System;
using System.Linq;

namespace Docentra_Mac.Views.Pages
{
    public partial class SettingsPage : UserControl
    {
        private string _selectedTheme = "Dark";

        public SettingsPage()
        {
            InitializeComponent();
            // Defer until the control is fully loaded so resources are available
            this.Loaded += (s, e) => LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Set current language index (default English for now)
            LanguageList.SelectedIndex = 0;
            
            HighlightTheme("Dark");
        }

        private void Theme_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.Tag is string themeName)
            {
                _selectedTheme = themeName;
                HighlightTheme(themeName);
                ApplyTheme(themeName);
            }
        }

        private void HighlightTheme(string themeName)
        {
            if (ThemePanel == null) return;
            
            // Safe brush retrieval — avoids UnsetValueType cast exception
            IBrush accentBrush = Brushes.Indigo;
            try
            {
                var raw = Application.Current?.FindResource("PremiumAccentBrush");
                if (raw is IBrush b) accentBrush = b;
            }
            catch { }

            foreach (var child in ThemePanel.Children)
            {
                if (child is Border border)
                {
                    border.BorderBrush = (border.Tag as string) == themeName
                        ? accentBrush
                        : Brushes.Transparent;
                }
            }
        }

        private void ApplyTheme(string themeName)
        {
            if (Application.Current is App app)
            {
                app.SetTheme(themeName);
                StatusText.Text = this.FindResource("Sett_ThemeActive")?.ToString() ?? "Theme activated.";
            }
        }

        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            // 1. Language Logic
            if (LanguageList.SelectedItem is ComboBoxItem item && item.Tag is string cultureCode)
            {
                if (Application.Current is App app)
                {
                    app.SetLocale(cultureCode);
                }
            }

            // 2. Theme Logic (Already applied live)
            
            StatusText.Text = this.FindResource("Sett_Success")?.ToString() ?? "Settings saved successfully!";
        }
    }
}

