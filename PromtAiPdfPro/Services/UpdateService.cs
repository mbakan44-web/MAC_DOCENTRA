using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace PromtAiPdfPro.Services
{
    public class UpdateService
    {
        private const string VersionUrl = "https://docentrapdf.com/version.txt";
        private const string DownloadPageUrl = "https://docentrapdf.com/download";

        public string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.2";

        public async Task<(bool isAvailable, string newVersion, string downloadUrl)> CheckForUpdatesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetStringAsync(VersionUrl);
                    var onlineVersion = response.Trim();

                    // Basit versiyon karşılaştırması
                    if (Version.TryParse(onlineVersion, out var v1) && 
                        Version.TryParse(CurrentVersion, out var v2))
                    {
                        if (v1 > v2)
                        {
                            return (true, onlineVersion, DownloadPageUrl);
                        }
                    }
                }
            }
            catch
            {
                // Sessiz hata: İnternet yoksa veya sunucu kapalıysa kullanıcıyı rahatsız etme
            }
            return (false, CurrentVersion, null);
        }
    }
}
