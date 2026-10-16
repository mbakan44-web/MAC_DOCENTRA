using System;

namespace PromtAiPdfPro.Helpers
{
    public static class LanguageManager
    {
        private static string _currentLocale = "tr-TR";
        public static event Action<string>? LanguageChanged;

        public static string CurrentLocale
        {
            get => _currentLocale;
            set
            {
                if (_currentLocale != value)
                {
                    _currentLocale = value;
                    LanguageChanged?.Invoke(value);
                }
            }
        }
    }
}
