using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PromtAiPdfPro.Services
{
    public class LicenseService
    {
        private static LicenseStatus _currentStatus = null;

        public class LicenseStatus
        {
            public bool IsPremium { get; set; }
            public AppStatus Status { get; set; }
            public int DaysRemaining { get; set; }
            public int DayOfUsage { get; set; }
            public string DeviceId { get; set; }
            public int DiscountPercent { get; set; }
            public string StatusMessage { get; set; }
            public string LockReason { get; set; }
        }

        public string GetHardwareId()
        {
            try
            {
                // 1. İşlemci ve Anakart (Mevcut)
                string cpu = GetWmiProperty("Win32_Processor", "ProcessorId");
                string baseboard = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
                
                // 2. Disk Seri No (Daha spesifik)
                string disk = GetWmiProperty("Win32_DiskDrive", "SerialNumber");

                // 3. Windows MachineGuid (Kayıt Defterinden - Çok Güçlüdür)
                string machineGuid = GetMachineGuid();

                // 4. Sanal Makine Denetimi (Güvenlik Soruşturması)
                bool isVM = DetectVirtualMachine();

                // Tüm verileri birleştir (Internal Identity)
                string rawIdentity = $"{cpu}-{baseboard}-{disk}-{machineGuid}-{isVM}";
                
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawIdentity));
                    StringBuilder builder = new StringBuilder();
                    
                    // Kullanıcıya sadece ilk 10 karakteri göster (Şık durur)
                    for (int i = 0; i < 5; i++)
                    {
                        builder.Append(bytes[i].ToString("X2"));
                    }
                    
                    return builder.ToString(); // Örn: 4F2A1B9C5E
                }
            }
            catch
            {
                return "ERR-SEC-HASH";
            }
        }

        private string GetWmiProperty(string @class, string property)
        {
            try
            {
                using (var mbs = new ManagementObjectSearcher($"Select {property} From {@class}"))
                using (var mbsList = mbs.Get())
                {
                    foreach (var mo in mbsList)
                    {
                        return mo[property]?.ToString() ?? "";
                    }
                }
            } catch { }
            return "UNKNOWN";
        }

        private string GetMachineGuid()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return subKey?.GetValue("MachineGuid")?.ToString() ?? "NO-GUID";
                }
            } catch { }
            return "DEFAULT-GUID";
        }

        private bool DetectVirtualMachine()
        {
            try
            {
                string model = GetWmiProperty("Win32_ComputerSystem", "Model").ToLower();
                string manufacturer = GetWmiProperty("Win32_ComputerSystem", "Manufacturer").ToLower();

                if (model.Contains("virtual") || model.Contains("vmware") || 
                    manufacturer.Contains("vmware") || manufacturer.Contains("virtualbox"))
                {
                    return true;
                }
            } catch { }
            return false;
        }

        public enum AppStatus
        {
            FullTrial,        // 0-7 Gün: Her şey açık, %80 indirim
            DiscountTrial,    // 8-14 Gün: Bazı sayfalar kapalı, %40 indirim
            RestrictedFree,   // 15+ Gün: Sayfalar kapalı + 5 sayfa sınırı
            Premium           // Sınırsız
        }

        // Hacker'ların işini zorlaştırmak için basit bool yerine BitFlags veya Karmaşık Enum kullanalım
        [Flags]
        public enum SecurityToken
        {
            None = 0,
            TrialActive = 1,
            DiscountEligible = 2,
            AccessDenied = 4,
            PremiumVerified = 8,
            HardLimitActive = 16
        }

        private static SecurityToken _currentToken = SecurityToken.None;

        private static string GetInternalToken()
        {
            // OBFUSCATED: "DOCENTRA-SECURE-2026" is hidden as XORed bytes
            byte[] data = { 0x6E, 0x65, 0x69, 0x6F, 0x64, 0x78, 0x76, 0x6B, 0x07, 0x79, 0x6F, 0x69, 0x7F, 0x78, 0x6F, 0x07, 0x18, 0x1A, 0x18, 0x1C };
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) result[i] = (byte)(data[i] ^ 0x2A);
            return Encoding.UTF8.GetString(result);
        }

        private const string RegistryPath = @"SOFTWARE\DocentraApp";
        private const string RegistryKeyName = "LicenseKey";
        private const string EntropyKeyName = "SystemEntropy"; // Hidden InstallDate

        private int GetDynamicUsageDay()
        {
            try
            {
                using (var subKey = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    string raw = subKey?.GetValue(EntropyKeyName)?.ToString() ?? "";
                    long ticks;

                    if (string.IsNullOrEmpty(raw))
                    {
                        // First run: Save current time encrypted
                        ticks = DateTime.Now.Ticks;
                        long encrypted = ticks ^ 0x55AA55AA; // Obfuscation XOR
                        subKey?.SetValue(EntropyKeyName, encrypted.ToString());
                        return 1;
                    }

                    if (long.TryParse(raw, out long storedEncrypted))
                    {
                        ticks = storedEncrypted ^ 0x55AA55AA;
                        DateTime installDate = new DateTime(ticks);
                        int days = (DateTime.Now - installDate).Days + 1;
                        
                        // Anti-Cheat: If user rolls back PC clock, detect it
                        if (days < 0) return 99; 
                        return days;
                    }
                }
            }
            catch { }
            return 99; // Safety fallback to restricted mode
        }

        public async Task<LicenseStatus> CheckLicenseAsync()
        {
            string hwid = GetHardwareId();
            string savedKey = GetSavedLicenseKey();
            bool isActivated = ValidateKey(savedKey, hwid);

            int dayOfUsage = GetDynamicUsageDay();
            
            var status = new LicenseStatus
            {
                IsPremium = isActivated,
                DeviceId = hwid,
                DayOfUsage = dayOfUsage
            };

            if (isActivated)
            {
                _currentToken = SecurityToken.PremiumVerified;
                status.Status = AppStatus.Premium;
                status.StatusMessage = "Premium Active"; // Localization key should be used but for now simple string
            }
            else
            {
                // Mevcut Trial mantığı (dayOfUsage'a göre)
                if (dayOfUsage <= 7)
                {
                    _currentToken = SecurityToken.TrialActive | SecurityToken.DiscountEligible;
                    status.Status = AppStatus.FullTrial;
                    status.DaysRemaining = 7 - dayOfUsage;
                    status.DiscountPercent = 80;
                    status.StatusMessage = SafeGetResource("Prem_Status_Full", "Free Trial - Full Access");
                }
                else if (dayOfUsage <= 14)
                {
                    _currentToken = SecurityToken.TrialActive | SecurityToken.DiscountEligible | SecurityToken.AccessDenied;
                    status.Status = AppStatus.DiscountTrial;
                    status.DaysRemaining = 14 - dayOfUsage;
                    status.DiscountPercent = 40;
                    status.StatusMessage = SafeGetResource("Prem_Status_Discount", "Trial - Limited Access");
                    status.LockReason = SafeGetResource("Prem_LockReason_Discount", "Some features are locked. Upgrade to Premium.");
                }
                else
                {
                    _currentToken = SecurityToken.HardLimitActive | SecurityToken.AccessDenied;
                    status.Status = AppStatus.RestrictedFree;
                    status.DaysRemaining = 0;
                    status.StatusMessage = SafeGetResource("Prem_Status_Restricted", "Trial Expired");
                    status.LockReason = SafeGetResource("Prem_LockReason_Restricted", "Please purchase a license to continue.");
                }
            }

            _currentStatus = status;
            return _currentStatus;
        }

        public bool ValidateKey(string key, string hwid)
        {
            // NOTE: License validation is HWID-based and independent of App Version.
            // This ensures Premium status persists across updates (as long as Seed remains same).
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(hwid)) return false;

            try
            {
                // Algoritma: Hash(HWID + SecretSeed)
                string expectedKey = GenerateKeyFromHwid(hwid);
                return key.Equals(expectedKey, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        public string GenerateKeyFromHwid(string hwid)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Logic Scattering: Combine HWID and Seed in a non-obvious way
                string t = GetInternalToken();
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(hwid + t));
                StringBuilder builder = new StringBuilder();
                // 16 karakterlik bir key üretelim (Bloklar halinde: XXXX-XXXX-XXXX-XXXX)
                for (int i = 0; i < 8; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                    if (i % 2 == 1 && i < 7) builder.Append("-");
                }
                return builder.ToString();
            }
        }

        public bool ActivatePremium(string key)
        {
            string hwid = GetHardwareId();
            if (ValidateKey(key, hwid))
            {
                SaveLicenseKey(key);
                _currentStatus = null; // Önbelleği temizle ki CheckLicenseAsync tekrar çalışsın
                return true;
            }
            return false;
        }

        private void SaveLicenseKey(string key)
        {
            try
            {
                using (var subKey = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    subKey?.SetValue(RegistryKeyName, key);
                }
            } catch { }
        }

        private string GetSavedLicenseKey()
        {
            try
            {
                using (var subKey = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    return subKey?.GetValue(RegistryKeyName)?.ToString() ?? "";
                }
            } catch { return ""; }
        }

        private static string SafeGetResource(string key, string fallback)
        {
            try
            {
                var val = System.Windows.Application.Current?.TryFindResource(key);
                if (val is string s && !string.IsNullOrEmpty(s)) return s;
            }
            catch { }
            return fallback;
        }

        public bool ValidateAccess(string featureId)
        {
            // Premium ise her zaman izin ver
            if (_currentToken.HasFlag(SecurityToken.PremiumVerified)) return true;

            // Eğer erişim engeli token'ı varsa ve özellik kilitli gruptaysa
            if (_currentToken.HasFlag(SecurityToken.AccessDenied))
            {
                string[] restrictedFeatures = { "OcrPage", "OfficeConvertPage", "CropPage", "AddPageNumbersPage", "SignPage", "DeletePagesPage" };
                if (System.Array.Exists(restrictedFeatures, f => f == featureId))
                    return false;
            }

            return true;
        }

        public bool ValidateOperation(int count)
        {
            if (_currentToken.HasFlag(SecurityToken.PremiumVerified)) return true;

            // 15+ gün kısıtlaması (Hard Limit)
            if (_currentToken.HasFlag(SecurityToken.HardLimitActive))
            {
                return count <= 5;
            }

            return true;
        }
    }
}
