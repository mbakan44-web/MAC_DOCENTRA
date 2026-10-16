using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PromtAiPdfPro.Services
{
    public class SecurityService
    {
        private const string AppDataFolder = "PromtAiPdfPro";
        private const string LicenseFileName = "license.dat";
        private const int TrialDays = 7;
        private static readonly string EncryptionKey = "PromtAI-PDF-Professional-Secret-Key-2024"; // Gerçek projede daha güvenli saklanmalı

        public string GetHardwareID()
        {
            try
            {
                string cpuId = GetManagementInfo("Win32_Processor", "ProcessorId");
                string mbId = GetManagementInfo("Win32_BaseBoard", "SerialNumber");
                string diskId = GetManagementInfo("Win32_DiskDrive", "SerialNumber");

                string combined = $"{cpuId}-{mbId}-{diskId}";
                return ComputeHash(combined);
            }
            catch (Exception)
            {
                return "OFFLINE-STUB-ID-7788"; // Hata durumunda fallback
            }
        }

        private string GetManagementInfo(string className, string propertyName)
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}"))
            {
                foreach (var obj in searcher.Get())
                {
                    return obj[propertyName]?.ToString()?.Trim() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 4; i++) // Kısa bir ID oluşturması için ilk 4 segment
                {
                    builder.Append(bytes[i].ToString("X2"));
                    if (i < 3) builder.Append("-");
                }
                return $"PROMT-{builder.ToString()}";
            }
        }

        public TrialInfo GetTrialInfo()
        {
            string path = GetLicenseFilePath();
            if (!File.Exists(path))
            {
                var info = new TrialInfo { InstallDate = DateTime.Now, IsActivated = false };
                SaveTrialInfo(info);
                return info;
            }

            try
            {
                string encrypted = File.ReadAllText(path);
                string decrypted = Decrypt(encrypted);
                return JsonConvert.DeserializeObject<TrialInfo>(decrypted) ?? new TrialInfo { InstallDate = DateTime.Now, IsActivated = false, LicenseKey = string.Empty };
            }
            catch
            {
                return new TrialInfo { InstallDate = DateTime.Now.AddDays(-10), IsActivated = false }; // Hata durumunda kilitli
            }
        }

        public void SaveTrialInfo(TrialInfo info)
        {
            string path = GetLicenseFilePath();
            string json = JsonConvert.SerializeObject(info);
            string encrypted = Encrypt(json);
            File.WriteAllText(path, encrypted);
        }

        private string GetLicenseFilePath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, LicenseFileName);
        }

        private string Encrypt(string text)
        {
            byte[] salt = Encoding.ASCII.GetBytes("PROMT_SALT");
            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(EncryptionKey, salt, 1000, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(text);
                        cs.Write(data, 0, data.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            byte[] salt = Encoding.ASCII.GetBytes("PROMT_SALT");
            byte[] data = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(EncryptionKey, salt, 1000, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var ms = new MemoryStream(data))
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public class TrialInfo
    {
        public DateTime InstallDate { get; set; }
        public bool IsActivated { get; set; }
        public string LicenseKey { get; set; } = string.Empty;

        public bool IsExpired => !IsActivated && (DateTime.Now - InstallDate).TotalDays > 7;
        public int RemainingDays => Math.Max(0, 7 - (int)(DateTime.Now - InstallDate).TotalDays);
    }
}
