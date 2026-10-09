using System;
using System.Reflection;

namespace PromtAiPdfPro.Helpers
{
    public static class AppInfo
    {
        private static string? _version;
        private static string? _versionTag;
        private static string? _versionSuffix;

        /// <summary>
        /// Uygulamanın .csproj veya Assembly seviyesindeki saf versiyon numarası (Örn: "1.0.4")
        /// </summary>
        public static string Version
        {
            get
            {
                if (_version == null)
                {
                    try
                    {
                        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                        if (assemblyVersion != null)
                        {
                            _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
                        }
                    }
                    catch { }

                    _version ??= "1.0.4";
                }
                return _version;
            }
        }

        /// <summary>
        /// Versiyon etiketi (Örn: "v1.0.4")
        /// </summary>
        public static string VersionTag => _versionTag ??= $"v{Version}";

        /// <summary>
        /// Başlıklar için parantezli versiyon eki (Örn: " (v1.0.4)")
        /// </summary>
        public static string VersionSuffix => _versionSuffix ??= $" ({VersionTag})";
    }
}
