using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Docentra_Mac.Views;
using System;
using System.Linq;

namespace Docentra_Mac
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void SetLocale(string cultureCode)
        {
            try
            {
                var locales = this.Resources.MergedDictionaries;
                var newLocale = new Avalonia.Markup.Xaml.Styling.ResourceInclude(new System.Uri("avares://Docentra_Mac/App.axaml"))
                {
                    Source = new System.Uri($"avares://Docentra_Mac/Locales/{cultureCode}.axaml")
                };

                locales.Clear();
                locales.Add(newLocale);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetLocale Error: {ex.Message}");
            }
        }

        public void SetTheme(string themeName)
        {
            var res = this.Resources;
            
            // Set Base Theme Variant
            RequestedThemeVariant = themeName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

            switch (themeName)
            {
                case "Light":
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.Parse("#4F46E5"));
                    res["MainBackground"] = new SolidColorBrush(Color.Parse("#F8FAFC"));
                    res["SidebarBackground"] = new SolidColorBrush(Color.Parse("#F1F5F9"));
                    res["TextForeground"] = new SolidColorBrush(Color.Parse("#0F172A"));
                    res["SecondaryTextForeground"] = new SolidColorBrush(Color.Parse("#64748B"));
                    res["ToolCardBackground"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    break;
                case "Dark":
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.Parse("#4F46E5"));
                    res["MainBackground"] = new SolidColorBrush(Color.Parse("#020617"));
                    res["SidebarBackground"] = new SolidColorBrush(Color.Parse("#0F172A"));
                    res["TextForeground"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    res["SecondaryTextForeground"] = new SolidColorBrush(Color.Parse("#94A3B8"));
                    res["ToolCardBackground"] = new SolidColorBrush(Color.Parse("#0F172A"));
                    break;
                case "Lavender":
                    RequestedThemeVariant = ThemeVariant.Dark;
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.Parse("#7B1FA2"));
                    res["MainBackground"] = new SolidColorBrush(Color.Parse("#1A0F2E"));
                    res["TextForeground"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    break;
                case "Peach":
                    RequestedThemeVariant = ThemeVariant.Dark;
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.Parse("#D34F1E"));
                    res["MainBackground"] = new SolidColorBrush(Color.Parse("#1F0D05"));
                    res["TextForeground"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    break;
                case "Mint":
                    RequestedThemeVariant = ThemeVariant.Dark;
                    res["PremiumAccentBrush"] = new SolidColorBrush(Color.Parse("#1B7A52"));
                    res["MainBackground"] = new SolidColorBrush(Color.Parse("#051F14"));
                    res["TextForeground"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    break;
            }
        }
    }
}
