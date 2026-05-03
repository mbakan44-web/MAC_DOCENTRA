using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Docentra_Mac.Services
{
    public class UpdateService
    {
        private const string VersionUrl = "https://docentrapdf.com/version.txt";
        private const string CurrentVersion = "1.0.0";
        private readonly HttpClient _httpClient;

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public class UpdateInfo
        {
            public bool HasUpdate { get; set; }
            public string? NewVersion { get; set; }
            public string? DownloadUrl { get; set; }
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(VersionUrl);
                if (string.IsNullOrWhiteSpace(response)) return new UpdateInfo { HasUpdate = false };

                var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 1) return new UpdateInfo { HasUpdate = false };

                string remoteVersion = lines[0].Trim();
                string downloadUrl = lines.Length > 1 ? lines[1].Trim() : "https://docentrapdf.com/download";

                bool hasUpdate = IsNewerVersion(remoteVersion, CurrentVersion);

                return new UpdateInfo
                {
                    HasUpdate = hasUpdate,
                    NewVersion = remoteVersion,
                    DownloadUrl = downloadUrl
                };
            }
            catch
            {
                return new UpdateInfo { HasUpdate = false };
            }
        }

        private bool IsNewerVersion(string remote, string current)
        {
            try
            {
                Version vRemote = new Version(remote);
                Version vCurrent = new Version(current);
                return vRemote > vCurrent;
            }
            catch { return false; }
        }
    }
}
