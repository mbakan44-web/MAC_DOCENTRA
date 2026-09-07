using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PromtAiPdfPro.Services
{
    /// <summary>
    /// Kullanıcının en son açtığı PDF dosyalarını yönetir.
    /// Dosyalar AppData\Local\Docentra\recent_files.json içinde saklanır.
    /// </summary>
    public static class RecentFilesService
    {
        private const int MaxRecentFiles = 8;
        private static readonly string StoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Docentra",
            "recent_files.json"
        );

        /// <summary>
        /// Verilen dosya yolunu son kullanılanlar listesine ekler.
        /// Dosya zaten listede varsa en başa taşır.
        /// </summary>
        public static void AddFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try
            {
                var files = LoadFiles();

                // Aynı dosyayı listeden kaldır (varsa)
                files.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));

                // En başa ekle
                files.Insert(0, filePath);

                // Maksimum sayıyı aş
                if (files.Count > MaxRecentFiles)
                    files = files.Take(MaxRecentFiles).ToList();

                SaveFiles(files);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RecentFilesService.AddFile hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Son kullanılan dosyaların listesini döndürür.
        /// Var olmayan dosyaları otomatik olarak filtreler.
        /// </summary>
        public static List<RecentFileItem> GetRecentFiles()
        {
            try
            {
                var paths = LoadFiles();
                // Dosya sisteminde hâlâ var olanları al
                return paths
                    .Where(File.Exists)
                    .Select(p => new RecentFileItem
                    {
                        FullPath = p,
                        FileName = Path.GetFileName(p),
                        FileNameWithoutExt = Path.GetFileNameWithoutExtension(p),
                        Directory = Path.GetDirectoryName(p) ?? "",
                        LastModified = File.GetLastWriteTime(p)
                    })
                    .ToList();
            }
            catch
            {
                return new List<RecentFileItem>();
            }
        }

        /// <summary>
        /// Belirli bir dosyayı son kullanılanlar listesinden kaldırır.
        /// </summary>
        public static void RemoveFile(string filePath)
        {
            try
            {
                var files = LoadFiles();
                files.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));
                SaveFiles(files);
            }
            catch { }
        }

        /// <summary>
        /// Tüm son kullanılanlar listesini temizler.
        /// </summary>
        public static void ClearAll()
        {
            try
            {
                SaveFiles(new List<string>());
            }
            catch { }
        }

        // --- Private Helpers ---

        private static List<string> LoadFiles()
        {
            if (!File.Exists(StoragePath))
                return new List<string>();

            var json = File.ReadAllText(StoragePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        private static void SaveFiles(List<string> files)
        {
            var dir = Path.GetDirectoryName(StoragePath)!;
            Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StoragePath, json);
        }
    }

    /// <summary>
    /// Son kullanılan bir dosyayı temsil eden model.
    /// </summary>
    public class RecentFileItem
    {
        public string FullPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileNameWithoutExt { get; set; } = "";
        public string Directory { get; set; } = "";
        public DateTime LastModified { get; set; }
    }
}
