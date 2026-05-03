using System;
using System.IO;
using System.Text.Json;

namespace PromtAiPdfPro.Services
{
    public class AppSettings
    {
        public string Language { get; set; } = "Auto"; // "Auto", "tr-TR", "en-US"
        public string Theme { get; set; } = "Dark";    // "Dark", "Light", "PastelBlue", "Lavender", "Peach", "Mint", "Apricot"
        public string DefaultOutputPath { get; set; } = ""; // Boş ise kullanıcıya sorulur
    }

    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsFilePath;
        private AppSettings _currentSettings = null!;

        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "PromtAiPdfPro");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
            LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _currentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _currentSettings = new AppSettings();
                }
            }
            else
            {
                _currentSettings = new AppSettings();
                SaveSettings();
            }
            return _currentSettings;
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to save settings: " + ex.Message);
            }
        }

        public AppSettings Current => _currentSettings;
    }
}
