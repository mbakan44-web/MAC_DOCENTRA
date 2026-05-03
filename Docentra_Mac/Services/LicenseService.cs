using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Docentra_Mac.Services
{
    public class LicenseService
    {
        private static LicenseStatus? _currentStatus = null;

        public class LicenseStatus
        {
            public bool IsPremium { get; set; }
            public string DeviceId { get; set; }
            public string StatusMessage { get; set; }
        }

        private string GetLicenseFilePath()
        {
            // Mac'te Home dizini altında gizli klasör kullanırız
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string folder = Path.Combine(home, ".docentra");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, "license.key");
        }

        public string GetHardwareId()
        {
            try
            {
                // Mac için Seri Numarası veya UUID alma (IOPlatformSerialNumber)
                string uuid = ExecuteMacCommand("ioreg -rd1 -c IOPlatformExpertDevice | grep -E 'IOPlatformUUID'");
                
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(uuid + "MAC-SALT-2026"));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < 5; i++)
                    {
                        builder.Append(bytes[i].ToString("X2"));
                    }
                    return builder.ToString();
                }
            }
            catch
            {
                return "MAC-ERR-ID";
            }
        }

        private string ExecuteMacCommand(string command)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "WIN-TEST-DEVICE-ID"; // Windows'ta test ederken dönecek ID
            }

            try
            {
                var escapedArgs = command.Replace("\"", "\\\"");
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result.Trim();
            }
            catch { return "MAC-UNKNOWN-DEVICE"; }
        }

        public async Task<LicenseStatus> CheckLicenseAsync()
        {
            string hwid = GetHardwareId();
            string savedKey = GetSavedLicenseKey();
            bool isActivated = ValidateKey(savedKey, hwid);

            var status = new LicenseStatus
            {
                IsPremium = isActivated,
                DeviceId = hwid,
                StatusMessage = isActivated ? "Premium Active" : "Trial Mode"
            };

            _currentStatus = status;
            return status;
        }

        public bool ValidateKey(string key, string hwid)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(hwid)) return false;
            string expectedKey = GenerateKeyFromHwid(hwid);
            return key.Equals(expectedKey, StringComparison.OrdinalIgnoreCase);
        }

        public string GenerateKeyFromHwid(string hwid)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Token obfuscation (Windows ile aynı tuzu kullanıyoruz ki jeneratör ortak çalışsın)
                string t = "DOCENTRA-SECURE-2026"; 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(hwid + t));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 8; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                    if (i % 2 == 1 && i < 7) builder.Append("-");
                }
                return builder.ToString();
            }
        }

        private string GetSavedLicenseKey()
        {
            try
            {
                string path = GetLicenseFilePath();
                if (File.Exists(path)) return File.ReadAllText(path).Trim();
            }
            catch { }
            return "";
        }

        public void SaveLicenseKey(string key)
        {
            try
            {
                File.WriteAllText(GetLicenseFilePath(), key);
            }
            catch { }
        }
    }
}
