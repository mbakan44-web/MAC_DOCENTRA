using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PromtAiPdfPro.Services
{
    public class PdfService
    {
        private static readonly object _logLock = new object();
        // STA Thread Runner Helper for Office Interop
        private static Task<T> RunInSTAAsync<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(action());
                }
                catch (Exception ex)
                {
                    LogMessage($"STA Thread Error: {ex.Message}");
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return tcs.Task;
        }

        private static void LogMessage(string message)
        {
            try
            {
                lock (_logLock)
                {
                    File.AppendAllText("crash_log.txt", $"[{DateTime.Now}] {message}\n");
                }
            }
            catch { }
        }

        public static void KillOfficeProcesses()
        {
            string[] processes = { "WINWORD", "EXCEL", "POWERPNT" };
            foreach (var name in processes)
            {
                try
                {
                    var found = System.Diagnostics.Process.GetProcessesByName(name);
                    foreach (var p in found)
                    {
                        try { p.Kill(); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error killing process {name}: {ex.Message}");
                }
            }
        }

        public int GetPageCount(string filePath)
        {
            using (var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
            {
                return inputDocument.PageCount;
            }
        }

        public async Task<bool> MergeFilesAsync(List<string> sourceFiles, string targetPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument targetDoc = new PdfDocument())
                    {
                        foreach (string file in sourceFiles)
                        {
                            using (PdfDocument sourceDoc = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                            {
                                foreach (PdfPage page in sourceDoc.Pages)
                                {
                                    targetDoc.AddPage(page);
                                }
                            }
                        }
                        targetDoc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Merge Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> SplitPagesAsync(string sourcePath, string rangeInput = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string dir = Path.GetDirectoryName(sourcePath) ?? "";
                    string name = Path.GetFileNameWithoutExtension(sourcePath);

                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        if (string.IsNullOrWhiteSpace(rangeInput))
                        {
                            // Klasik: Her sayfayı ayrı PDF yap
                            for (int i = 0; i < sourceDoc.PageCount; i++)
                            {
                                string outPath = Path.Combine(dir, $"{name}_Sayfa_{i + 1}.pdf");
                                using (PdfDocument targetDoc = new PdfDocument())
                                {
                                    targetDoc.AddPage(sourceDoc.Pages[i]);
                                    targetDoc.Save(outPath);
                                }
                            }
                        }
                        else
                        {
                            // Hibrit: Belirli sayfaları tek bir PDF'de topla veya parçala
                            var targetPages = ParsePageRange(rangeInput, sourceDoc.PageCount);
                            if (targetPages.Count == 0) return false;

                            string outPath = Path.Combine(dir, $"{name}_Split.pdf");
                            using (PdfDocument targetDoc = new PdfDocument())
                            {
                                foreach (int i in targetPages.OrderBy(x => x))
                                {
                                    targetDoc.AddPage(sourceDoc.Pages[i]);
                                }
                                targetDoc.Save(outPath);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Split Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> DeletePagesAsync(string sourcePath, string targetPath, string rangeInput)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        var pagesToDelete = ParsePageRange(rangeInput, sourceDoc.PageCount);
                        using (PdfDocument targetDoc = new PdfDocument())
                        {
                            for (int i = 0; i < sourceDoc.PageCount; i++)
                            {
                                if (!pagesToDelete.Contains(i))
                                {
                                    targetDoc.AddPage(sourceDoc.Pages[i]);
                                }
                            }
                            targetDoc.Save(targetPath);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"DeletePages Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> ImagesToPdfAsync(List<string> imagePaths, string targetPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument targetDoc = new PdfDocument())
                    {
                        foreach (string imgPath in imagePaths)
                        {
                            PdfPage page = targetDoc.AddPage();
                            using (XImage image = XImage.FromFile(imgPath))
                            {
                                page.Width = XUnit.FromPoint(image.PointWidth);
                                page.Height = XUnit.FromPoint(image.PointHeight);

                                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                {
                                    gfx.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
                                }
                            }
                        }
                        targetDoc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"ImagesToPdf Error: {ex.Message}");
                    return false;
                }
            });
        }

        // --- GÜVENLİK VE FİLİGRAN METOTLARI ---

        public async Task<bool> ProtectPdfAsync(string sourcePath, string targetPath, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        doc.SecuritySettings.UserPassword = password;
                        doc.SecuritySettings.OwnerPassword = Guid.NewGuid().ToString();
                        doc.SecuritySettings.PermitPrint = true;
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Protect Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> UnlockPdfAsync(string sourcePath, string targetPath, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Şifreli dosyayı 'Import' modunda ve kullanıcı şifresiyle açıyoruz
                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, password, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument targetDoc = new PdfDocument())
                        {
                            foreach (PdfPage page in sourceDoc.Pages)
                            {
                                targetDoc.AddPage(page);
                            }
                            targetDoc.Save(targetPath);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Unlock Error: {ex.Message}");
                    return false;
                }
            });
        }

        private HashSet<int> ParsePageRange(string input, int totalPages)
        {
            var pages = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(input)) return pages;

            string normalized = input.ToLower().Trim();

            if (normalized == "all")
            {
                for (int i = 0; i < totalPages; i++) pages.Add(i);
                return pages;
            }

            if (normalized == "first")
            {
                pages.Add(0);
                return pages;
            }

            var parts = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains(":"))
                {
                    var range = trimmed.Split(':');
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        for (int i = Math.Max(1, start); i <= Math.Min(totalPages, end); i++)
                        {
                            pages.Add(i - 1);
                        }
                    }
                }
                else if (int.TryParse(trimmed, out int page))
                {
                    if (page >= 1 && page <= totalPages)
                    {
                        pages.Add(page - 1);
                    }
                }
            }
            return pages;
        }

        public async Task<bool> AddTextWatermarkAsync(string sourcePath, string targetPath, string watermarkText, XRect rect, double fontSize, double opacity, double rotation, string rangeInput)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont("Arial", fontSize, XFontStyleEx.Bold);
                        int alpha = (int)(opacity * 255);
                        // Using black with alpha for better contrast (0, 0, 0)
                        XBrush brush = new XSolidBrush(XColor.FromArgb(alpha, 0, 0, 0));

                        var targetPages = ParsePageRange(rangeInput, doc.PageCount);
                        foreach (int i in targetPages)
                        {
                            var page = doc.Pages[i];
                            using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                            {
                                var state = gfx.Save();
                                
                                double centerX = rect.X + rect.Width / 2;
                                double centerY = rect.Y + rect.Height / 2;
                                
                                // Rotate around center: Translate -> Rotate -> Translate
                                gfx.TranslateTransform(centerX, centerY);
                                gfx.RotateTransform(rotation);
                                gfx.TranslateTransform(-centerX, -centerY);
                                
                                gfx.DrawString(watermarkText, font, brush, rect, XStringFormats.TopLeft);
                                
                                gfx.Restore(state);
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"TextWatermark Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> AddImageWatermarkAsync(string sourcePath, string targetPath, string imagePath, XRect rect, double opacity, string rangeInput, bool isRelative = false)
        {
            try
            {
                // 1. Resmi bellekte şeffaflaştır (WPF Görüntü İşleme - STA Gerekir)
                byte[] transparentBytes = await RunInSTAAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    var dv = new DrawingVisual();
                    using (var dc = dv.RenderOpen())
                    {
                        dc.PushOpacity(opacity);
                        dc.DrawImage(bitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    }

                    var rtb = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(dv);

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        return ms.ToArray();
                    }
                });

                // 2. Şeffaf resmi PDF'e işle
                return await Task.Run(() =>
                {
                    try
                    {
                        using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                        {
                            var imageStream = new MemoryStream();
                            imageStream.Write(transparentBytes, 0, transparentBytes.Length);
                            imageStream.Position = 0;

                            using (imageStream)
                            {
                                using (XImage image = XImage.FromStream(imageStream))
                                {
                                    var targetPages = ParsePageRange(rangeInput, doc.PageCount);
                                    foreach (int i in targetPages)
                                    {
                                        var page = doc.Pages[i];
                                        using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                                        {
                                            XRect targetRect = rect;
                                            if (isRelative)
                                            {
                                                // Convert relative (0-1) to PDF points
                                                targetRect = new XRect(
                                                    rect.X * page.Width.Point,
                                                    rect.Y * page.Height.Point,
                                                    rect.Width * page.Width.Point,
                                                    rect.Height * page.Height.Point
                                                );
                                            }
                                            gfx.DrawImage(image, targetRect);
                                        }
                                    }
                                }
                            }
                            doc.Save(targetPath);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"ImageWatermark Process Error: {ex.Message}");
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                LogMessage($"ImageWatermark Global Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CropPdfAsync(string sourcePath, string targetPath, double x, double y, double width, double height, bool applyToAll, int currentPageIndex, Dictionary<int, Rect> pageSelections = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        for (int i = 0; i < doc.Pages.Count; i++)
                        {
                            PdfPage page = doc.Pages[i];
                            double pdfHeight = page.Height.Point;
                            Rect cropRect;

                            if (applyToAll)
                            {
                                cropRect = new Rect(x, y, width, height);
                            }
                            else if (pageSelections != null && pageSelections.ContainsKey(i))
                            {
                                cropRect = pageSelections[i];
                            }
                            else if (i == currentPageIndex)
                            {
                                cropRect = new Rect(x, y, width, height);
                            }
                            else
                            {
                                // Skip pages with no selection in non-apply-to-all mode if you want, 
                                // but usually we should at least keep them as is.
                                continue; 
                            }

                            double cropY = pdfHeight - (cropRect.Y + cropRect.Height);
                            page.CropBox = new PdfRectangle(new XRect(cropRect.X, cropY, cropRect.Width, cropRect.Height));
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Crop Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> AddPageNumbersAsync(string sourcePath, string targetPath, int startNum, int endNum, string position, string format, string fontName, double fontSize, string colorHex, double margin, int startingNumber)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont(fontName, fontSize, XFontStyleEx.Regular);
                        
                        // Parse hex color manually if FromHtml is not available
                        XColor xColor = XColors.Black;
                        try {
                            var colorObj = new System.Drawing.ColorConverter().ConvertFromString(colorHex);
                            if (colorObj is System.Drawing.Color color)
                            {
                                xColor = XColor.FromArgb(color.A, color.R, color.G, color.B);
                            }
                        } catch { }
                        XBrush brush = new XSolidBrush(xColor);
                        
                        int totalPages = doc.PageCount;
                        int s = Math.Max(1, startNum);
                        int e = Math.Min(totalPages, endNum);

                        for (int i = s - 1; i < e; i++)
                        {
                            PdfPage page = doc.Pages[i];
                            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                            {
                                int currentNum = (i - (s - 1)) + startingNumber;
                                string text = format.Replace("{n}", currentNum.ToString()).Replace("{total}", totalPages.ToString());
                                XSize size = gfx.MeasureString(text, font);
                                
                                double x = 0, y = 0;
                                switch (position.ToLower())
                                {
                                    case "bottomcenter":
                                        x = (page.Width.Point - size.Width) / 2;
                                        y = page.Height.Point - margin - size.Height;
                                        break;
                                    case "bottomright":
                                        x = page.Width.Point - margin - size.Width;
                                        y = page.Height.Point - margin - size.Height;
                                        break;
                                    case "bottomleft":
                                        x = margin;
                                        y = page.Height.Point - margin - size.Height;
                                        break;
                                    case "topcenter":
                                        x = (page.Width.Point - size.Width) / 2;
                                        y = margin;
                                        break;
                                    case "topright":
                                        x = page.Width.Point - margin - size.Width;
                                        y = margin;
                                        break;
                                    case "topleft":
                                        x = margin;
                                        y = margin;
                                        break;
                                }
                                gfx.DrawString(text, font, brush, x, y + size.Height); // PdfSharp draws from baseline
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"AddPageNumbers Error: {ex.Message}");
                    return false;
                }
            });
        }
        public async Task<bool> WordToPdfAsync(string wordPath, string pdfPath)
        {
            return await RunInSTAAsync(() => {
                dynamic? wordApp = null; dynamic? documents = null; dynamic? doc = null;
                try {
                    Type? wordType = Type.GetTypeFromProgID("Word.Application");
                    if (wordType == null) { LogMessage("Word not found (ProgID null)"); return false; }
                    try {
                        wordApp = Activator.CreateInstance(wordType);
                    } catch (Exception ex) when (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) {
                        LogMessage("Word server failure detected, attempting self-healing...");
                        PdfService.KillOfficeProcesses();
                        System.Threading.Thread.Sleep(1500);
                        wordApp = Activator.CreateInstance(wordType);
                    }

                    if (wordApp == null) { LogMessage("Word instance creation failed"); return false; }
                    wordApp.Visible = false;
                    documents = wordApp.Documents;
                    doc = documents.Open(wordPath);
                    // 17 = wdExportFormatPDF
                    doc.ExportAsFixedFormat(pdfPath, 17);
                    return true;
                } catch (Exception ex) { 
                    string errorMsg = ex.Message;
                    if (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) 
                    {
                        errorMsg = "Word startup failed (CO_E_SERVER_EXEC_FAILURE). A hung process might be blocking this. Try cleaning office processes.";
                    }
                    LogMessage($"WordToPdf Error: {errorMsg} (HResult: 0x{ex.HResult:X8})");
                    return false; 
                } finally {
                    if (doc != null) { doc.Close(0); Marshal.ReleaseComObject(doc); }
                    if (documents != null) Marshal.ReleaseComObject(documents);
                    if (wordApp != null) { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); }
                }
            });
        }

        public async Task<bool> PowerPointToPdfAsync(string pptPath, string pdfPath)
        {
            return await RunInSTAAsync(() => {
                dynamic? pptApp = null; dynamic? presentations = null; dynamic? pres = null;
                try {
                    Type? pptType = Type.GetTypeFromProgID("PowerPoint.Application");
                    if (pptType == null) { LogMessage("PowerPoint not found (ProgID null)"); return false; }
                    try {
                        pptApp = Activator.CreateInstance(pptType);
                    } catch (Exception ex) when (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) {
                        LogMessage("PowerPoint server failure detected, attempting self-healing...");
                        PdfService.KillOfficeProcesses();
                        System.Threading.Thread.Sleep(1500);
                        pptApp = Activator.CreateInstance(pptType);
                    }

                    if (pptApp == null) { LogMessage("PowerPoint instance creation failed"); return false; }
                    
                    presentations = pptApp.Presentations;
                    // msoFalse = 0, msoTrue = -1
                    // Open(filename, ReadOnly, Untitled, WithWindow)
                    pres = presentations.Open(pptPath, -1, -1, 0); 
                    // 32 = ppFixedFormatTypePDF
                    pres.ExportAsFixedFormat(pdfPath, 32);
                    return true;
                } catch (Exception ex) {
                    string errorMsg = ex.Message;
                    if (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) 
                    {
                        errorMsg = "PowerPoint startup failed (CO_E_SERVER_EXEC_FAILURE). A hung process might be blocking this. Try cleaning office processes.";
                    }
                    LogMessage($"PowerPointToPdf Error: {errorMsg} (HResult: 0x{ex.HResult:X8})");
                    return false;
                } finally {
                    if (pres != null) { pres.Close(); Marshal.ReleaseComObject(pres); }
                    if (presentations != null) Marshal.ReleaseComObject(presentations);
                    if (pptApp != null) { pptApp.Quit(); Marshal.ReleaseComObject(pptApp); }
                }
            });
        }

        public async Task<bool> PdfToWordAsync(string pdfPath, string wordPath)
        {
            return await RunInSTAAsync(() => {
                dynamic? wordApp = null; dynamic? documents = null; dynamic? doc = null;
                try {
                    Type? wordType = Type.GetTypeFromProgID("Word.Application");
                    if (wordType == null) return false;
                    wordApp = Activator.CreateInstance(wordType);
                    if (wordApp == null) return false;
                    wordApp.Visible = false;
                    documents = wordApp.Documents;
                    doc = documents.Open(pdfPath);
                    doc.SaveAs2(wordPath, 12);
                    return true;
                } catch { return false; }
                finally {
                    if (doc != null) { doc.Close(0); Marshal.ReleaseComObject(doc); }
                    if (documents != null) Marshal.ReleaseComObject(documents);
                    if (wordApp != null) { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); }
                }
            });
        }

        public async Task<bool> OfficeToPdfAsync(string sourcePath, string pdfPath)
        {
            string ext = Path.GetExtension(sourcePath).ToLower();
            if (ext == ".docx" || ext == ".doc")
                return await WordToPdfAsync(sourcePath, pdfPath);
            else if (ext == ".xlsx" || ext == ".xls")
                return await ExcelToPdfAsync(sourcePath, pdfPath);
            else if (ext == ".pptx" || ext == ".ppt")
                return await PowerPointToPdfAsync(sourcePath, pdfPath);
            
            return false;
        }

        public async Task<bool> ExcelToPdfAsync(string excelPath, string pdfPath)
        {
            return await RunInSTAAsync(() => {
                dynamic? excelApp = null; dynamic? workbooks = null; dynamic? workbook = null;
                try {
                    Type? excelType = Type.GetTypeFromProgID("Excel.Application");
                    if (excelType == null) { LogMessage("Excel not found (ProgID null)"); return false; }
                    try {
                        excelApp = Activator.CreateInstance(excelType);
                    } catch (Exception ex) when (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) {
                        LogMessage("Excel server failure detected, attempting self-healing...");
                        PdfService.KillOfficeProcesses();
                        System.Threading.Thread.Sleep(1500);
                        excelApp = Activator.CreateInstance(excelType);
                    }

                    if (excelApp == null) { LogMessage("Excel instance creation failed"); return false; }
                    excelApp.Visible = false;
                    excelApp.DisplayAlerts = false;
                    workbooks = excelApp.Workbooks;
                    workbook = workbooks.Open(excelPath);
                    // 0 = xlTypePDF
                    workbook.ExportAsFixedFormat(0, pdfPath);
                    return true;
                } catch (Exception ex) { 
                    string errorMsg = ex.Message;
                    if (ex.HResult == -2147221003 || ex.Message.Contains("80080005")) 
                    {
                        errorMsg = "Excel startup failed (CO_E_SERVER_EXEC_FAILURE). A hung process might be blocking this. Try cleaning office processes.";
                    }
                    LogMessage($"ExcelToPdf Error: {errorMsg} (HResult: 0x{ex.HResult:X8})");
                    return false; 
                } finally {
                    if (workbook != null) { workbook.Close(false); Marshal.ReleaseComObject(workbook); }
                    if (workbooks != null) Marshal.ReleaseComObject(workbooks);
                    if (excelApp != null) { excelApp.Quit(); Marshal.ReleaseComObject(excelApp); }
                }
            });
        }

        public async Task<bool> PdfToImageAsync(string pdfPath, string outputFolder)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(pdfPath);
                Windows.Data.Pdf.PdfDocument pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(outputFolder);

                for (uint i = 0; i < pdfDoc.PageCount; i++)
                {
                    using (Windows.Data.Pdf.PdfPage page = pdfDoc.GetPage(i))
                    {
                        string fileName = $"Sayfa_{i + 1}.png";
                        StorageFile imgFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                        
                        using (IRandomAccessStream stream = await imgFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            Windows.Data.Pdf.PdfPageRenderOptions options = new Windows.Data.Pdf.PdfPageRenderOptions();
                            options.DestinationWidth = (uint)(page.Size.Width * 3); // Yüksek çözünürlük (3x)
                            await page.RenderToStreamAsync(stream, options);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"PdfToImage Error: {ex.Message}");
                return false;
            }
        }

        public async Task<BitmapSource?> GetPdfThumbnailAsync(string pdfPath)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(pdfPath);
                Windows.Data.Pdf.PdfDocument pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

                if (pdfDoc.PageCount == 0) return null;

                using (Windows.Data.Pdf.PdfPage page = pdfDoc.GetPage(0))
                {
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await page.RenderToStreamAsync(stream);
                        
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream.AsStream();
                        bitmap.EndInit();
                        bitmap.Freeze(); 
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"GetPdfThumbnail Error: {ex.Message}");
                return null;
            }
        }

        public async Task<BitmapSource?> GetPdfPageImageAsync(string pdfPath, uint pageIndex)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(pdfPath);
                Windows.Data.Pdf.PdfDocument pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

                if (pageIndex >= pdfDoc.PageCount) return null;

                using (Windows.Data.Pdf.PdfPage page = pdfDoc.GetPage(pageIndex))
                {
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        Windows.Data.Pdf.PdfPageRenderOptions options = new Windows.Data.Pdf.PdfPageRenderOptions();
                        options.DestinationWidth = (uint)(page.Size.Width * 1.5); // Preview quality
                        await page.RenderToStreamAsync(stream, options);
                        
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream.AsStream();
                        bitmap.EndInit();
                        bitmap.Freeze(); 
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"GetPdfPageImage Error: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetPageCountAsync(string pdfPath)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(pdfPath);
                Windows.Data.Pdf.PdfDocument pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
                return (int)pdfDoc.PageCount;
            }
            catch (Exception ex)
            {
                LogMessage($"GetPageCount Error: {ex.Message}");
                return 0;
            }
        }
    }
}
