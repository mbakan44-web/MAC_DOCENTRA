using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace PromtAiPdfPro.Helpers
{
    public static class FileAssociationHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0x0000;

        public static bool RegisterPdfAssociation()
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string exeName = Path.GetFileName(exePath);
                string progId = "Docentra.PDF";
                
                // Ensure icon is extracted to a persistent location (important for portable EXE)
                string iconPath = PrepareIconFile();

                // 1. Create ProgId
                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
                {
                    key.SetValue("", "Docentra PDF Document");
                    key.SetValue("FriendlyTypeName", "Docentra PDF Document");
                    key.SetValue("AppUserModelID", "Docentra.PDF.Editor");

                    using (var icon = key.CreateSubKey("DefaultIcon"))
                    {
                        // Use quoted path with index
                        icon.SetValue("", $"\"{iconPath}\",0");
                    }

                    using (var shell = key.CreateSubKey(@"shell\open\command"))
                    {
                        shell.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }

                // 2. Associate .pdf with ProgId
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf"))
                {
                    key.SetValue("", progId);
                }

                // 2b. Add to OpenWithProgids (Crucial for Win 10/11)
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf\OpenWithProgids"))
                {
                    key.SetValue(progId, "");
                }

                // 3. Register Application for "Open With" menu
                using (var appKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\Applications\{exeName}"))
                {
                    appKey.SetValue("FriendlyAppName", "DOCENTRA");
                    using (var shell = appKey.CreateSubKey(@"shell\open\command"))
                    {
                        shell.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                    
                    using (var supportedTypes = appKey.CreateSubKey("SupportedTypes"))
                    {
                        supportedTypes.SetValue(".pdf", "");
                    }

                    using (var icon = appKey.CreateSubKey("DefaultIcon"))
                    {
                        icon.SetValue("", $"\"{iconPath}\",0");
                    }
                }

                // 4. Register Capabilities (The official way for Windows 10/11)
                string capsKey = $@"Software\Docentra\Capabilities";
                using (var key = Registry.CurrentUser.CreateSubKey(capsKey))
                {
                    key.SetValue("ApplicationDescription", "Professional PDF Editor");
                    key.SetValue("ApplicationName", "Docentra");
                    using (var assoc = key.CreateSubKey("FileAssociations"))
                    {
                        assoc.SetValue(".pdf", progId);
                    }
                }
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications"))
                {
                    key.SetValue("Docentra", capsKey);
                }

                // 5. Register Context Menu for Office and Images
                RegisterContextMenu();

                // 6. Notify Windows that associations have changed
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Association Error: " + ex.Message);
                return false;
            }
        }

        public static void RegisterContextMenu()
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string[] officeImgExts = { ".doc", ".docx", ".rtf", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
                
                // 1. Office and Images: "Convert to PDF"
                string convText = "PDF'e Dönüştür (Docentra)";
                try
                {
                    var currentLocale = LanguageManager.CurrentLocale;
                    if (currentLocale == "en-US") convText = "Convert to PDF (Docentra)";
                    else if (currentLocale == "de-DE") convText = "In PDF konvertieren (Docentra)";
                    else if (currentLocale == "fr-FR") convText = "Convertir en PDF (Docentra)";
                    else if (currentLocale == "es-ES") convText = "Convertir a PDF (Docentra)";
                }
                catch { }

                foreach (string ext in officeImgExts)
                {
                    RegisterVerb(Registry.CurrentUser, $@"Software\Classes\{ext}\shell\Docentra.Convert", convText, exePath, $"-convert \"%1\"");
                    RegisterVerb(Registry.CurrentUser, $@"Software\Classes\SystemFileAssociations\{ext}\shell\Docentra.Convert", convText, exePath, $"-convert \"%1\"");
                }

                // 2. PDF Files: "Edit PDF"
                string editText = "PDF Düzenle (Docentra)";
                try
                {
                    var currentLocale = LanguageManager.CurrentLocale;
                    if (currentLocale == "en-US") editText = "Edit PDF (Docentra)";
                    else if (currentLocale == "de-DE") editText = "PDF bearbeiten (Docentra)";
                    else if (currentLocale == "fr-FR") editText = "Modifier le PDF (Docentra)";
                    else if (currentLocale == "es-ES") editText = "Editar PDF (Docentra)";
                }
                catch { }

                // Remove OLD entry if exists (from previous implementation)
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.pdf\shell\Docentra.Convert", false); } catch { }
                try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\SystemFileAssociations\.pdf\shell\Docentra.Convert", false); } catch { }

                RegisterVerb(Registry.CurrentUser, @"Software\Classes\.pdf\shell\Docentra.Edit", editText, exePath, $"-edit \"%1\"");
                RegisterVerb(Registry.CurrentUser, @"Software\Classes\SystemFileAssociations\.pdf\shell\Docentra.Edit", editText, exePath, $"-edit \"%1\"");
                
                // Try to find the actual PDF ProgID and cleanup/register
                try
                {
                    string pdfProgId = Registry.ClassesRoot.OpenSubKey(".pdf")?.GetValue("")?.ToString();
                    if (!string.IsNullOrEmpty(pdfProgId))
                    {
                        try { Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{pdfProgId}\shell\Docentra.Convert", false); } catch { }
                        RegisterVerb(Registry.CurrentUser, $@"Software\Classes\{pdfProgId}\shell\Docentra.Edit", editText, exePath, $"-edit \"%1\"");
                    }
                }
                catch { }
            }
            catch { }
        }

        private static void RegisterVerb(RegistryKey baseKey, string keyPath, string menuText, string exePath, string arguments)
        {
            try
            {
                using (var key = baseKey.CreateSubKey(keyPath))
                {
                    key.SetValue("", menuText);
                    key.SetValue("Icon", exePath);
                    using (var command = key.CreateSubKey("command"))
                    {
                        command.SetValue("", $"\"{exePath}\" {arguments}");
                    }
                }
            }
            catch { }
        }

        private static string PrepareIconFile()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string docentraPath = Path.Combine(appData, "Docentra");
            string iconPath = Path.Combine(docentraPath, "pdf_file.ico");
            
            try
            {
                if (!Directory.Exists(docentraPath)) Directory.CreateDirectory(docentraPath);

                // Try multiple Pack URI variants
                string[] uris = {
                    "pack://application:,,,/Docentra;component/assets/pdf_file.ico",
                    "pack://application:,,,/PromtAiPdfPro;component/assets/pdf_file.ico",
                    "pack://application:,,,/Docentra;component/Assets/pdf_file.ico",
                    "pack://application:,,,/PromtAiPdfPro;component/Assets/pdf_file.ico",
                    "pack://application:,,,/Assets/pdf_file.ico"
                };

                foreach (var uriStr in uris)
                {
                    try
                    {
                        var resourceUri = new Uri(uriStr);
                        var streamResourceInfo = System.Windows.Application.GetResourceStream(resourceUri);
                        if (streamResourceInfo != null)
                        {
                            using (var fs = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                            {
                                streamResourceInfo.Stream.CopyTo(fs);
                            }
                            return iconPath;
                        }
                    }
                    catch { }
                }

                // Fallback: Check local Assets (next to EXE)
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pdf_file.ico");
                if (File.Exists(localPath))
                {
                    try
                    {
                        File.Copy(localPath, iconPath, true);
                        return iconPath;
                    }
                    catch { }
                }
            }
            catch { }

            return iconPath; 
        }
    }
}
