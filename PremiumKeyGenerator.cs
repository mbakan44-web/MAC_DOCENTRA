 vfgt565thnjv cccccccc<musing System;
using System.Security.Cryptography;
using System.Text;

namespace DocentraKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DOCENTRA PDF - Premium Key Generator v2.0";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("   DOCENTRA PDF & DOCUMENT EDITOR - KEY GENERATOR   ");
            Console.WriteLine("====================================================");
            Console.ResetColor();

            while (true)
            {
                Console.Write("\nLütfen Müşterinin HWID kodunu girin (Örn: 4F2A1B9C5E): ");
                string hwid = Console.ReadLine()?.Trim().ToUpper();

                if (string.IsNullOrEmpty(hwid))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Hata: HWID boş olamaz!");
                    Console.ResetColor();
                    continue;
                }

                string key = GeneratePremiumKey(hwid);

                Console.WriteLine("\n----------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PREMIUM LİSANS ANAHTARI OLUŞTURULDU:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n   {key}\n");
                Console.ResetColor();
                Console.WriteLine("----------------------------------------------------");
                
                Console.WriteLine("\nYeni bir anahtar üretmek için bir tuşa basın (Çıkış için Ctrl+C)...");
                Console.ReadKey();
            }
        }

        private static string GeneratePremiumKey(string hwid)
        {
            // Uygulama içindeki gizli salt (Token)
            string secretToken = GetInternalToken();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Uygulama ile birebir aynı algoritma: Hash(HWID + Token)
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(hwid + secretToken));
                
                StringBuilder builder = new StringBuilder();
                // 16 karakterlik (XXXX-XXXX-XXXX-XXXX) formatında anahtar üretimi
                for (int i = 0; i < 8; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                    if (i % 2 == 1 && i < 7) builder.Append("-");
                }
                return builder.ToString();
            }
        }

        private static string GetInternalToken()
        {
            // LicenseService.cs içindeki obfuscated token ile birebir aynıdır
            byte[] data = { 0x6E, 0x65, 0x69, 0x6F, 0x64, 0x78, 0x76, 0x6B, 0x07, 0x79, 0x6F, 0x69, 0x7F, 0x78, 0x6F, 0x07, 0x18, 0x1A, 0x18, 0x1C };
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) result[i] = (byte)(data[i] ^ 0x2A);
            return Encoding.UTF8.GetString(result);
        }
    }
}
