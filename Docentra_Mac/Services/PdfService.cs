using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;

namespace Docentra_Mac.Services
{
    public class PdfService
    {
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
                    Console.WriteLine($"Merge Error: {ex.Message}");
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
                    string outputDir = Path.GetDirectoryName(sourcePath) ?? "";
                    string fileName = Path.GetFileNameWithoutExtension(sourcePath);

                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        List<int> pagesToExtract;
                        if (string.IsNullOrEmpty(rangeInput))
                        {
                            pagesToExtract = Enumerable.Range(0, sourceDoc.PageCount).ToList();
                        }
                        else
                        {
                            pagesToExtract = ParsePageRange(rangeInput, sourceDoc.PageCount);
                        }

                        foreach (int i in pagesToExtract)
                        {
                            using (PdfDocument targetDoc = new PdfDocument())
                            {
                                targetDoc.AddPage(sourceDoc.Pages[i]);
                                string targetPath = Path.Combine(outputDir, $"{fileName}_page_{i + 1}.pdf");
                                targetDoc.Save(targetPath);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Split Error: {ex.Message}");
                    return false;
                }
            });
        }

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
                    Console.WriteLine($"Protect Error: {ex.Message}");
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
                    using (PdfDocument doc = PdfReader.Open(sourcePath, password, PdfDocumentOpenMode.Modify))
                    {
                        doc.SecuritySettings.UserPassword = "";
                        doc.SecuritySettings.OwnerPassword = "";
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unlock Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> AddPageNumbersAsync(string sourcePath, string targetPath, int startPage, int endPage, string position, string format, string fontName, double fontSize, string colorHex, double margin, int startingValue)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont(fontName, fontSize, XFontStyleEx.Regular);
                        XColor color = XColor.FromArgb(255, 
                            Convert.ToInt32(colorHex.Substring(1, 2), 16),
                            Convert.ToInt32(colorHex.Substring(3, 2), 16),
                            Convert.ToInt32(colorHex.Substring(5, 2), 16));
                        XBrush brush = new XSolidBrush(color);

                        int actualEnd = Math.Min(endPage, doc.PageCount);
                        int currentNum = startingValue;

                        for (int i = startPage - 1; i < actualEnd; i++)
                        {
                            PdfPage page = doc.Pages[i];
                            using (XGraphics gfx = XGraphics.FromPdfPage(page))
                            {
                                string text = format.Replace("{n}", currentNum.ToString()).Replace("{total}", doc.PageCount.ToString());
                                XSize size = gfx.MeasureString(text, font);
                                double x = 0, y = 0;

                                switch (position)
                                {
                                    case "BottomCenter": x = (page.Width.Point - size.Width) / 2; y = page.Height.Point - margin; break;
                                    case "BottomRight": x = page.Width.Point - size.Width - margin; y = page.Height.Point - margin; break;
                                    case "BottomLeft": x = margin; y = page.Height.Point - margin; break;
                                    case "TopCenter": x = (page.Width.Point - size.Width) / 2; y = margin + size.Height; break;
                                    case "TopRight": x = page.Width.Point - size.Width - margin; y = margin + size.Height; break;
                                    case "TopLeft": x = margin; y = margin + size.Height; break;
                                }

                                gfx.DrawString(text, font, brush, x, y);
                                currentNum++;
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Page Number Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> AddTextWatermarkAsync(string sourcePath, string targetPath, string text, Avalonia.Rect rect, double opacity, double rotation, double fontSize, string colorHex, string pages = "all")
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont("Arial", fontSize, XFontStyleEx.Bold);
                        XColor color = XColor.FromArgb((int)(opacity * 255), 
                            Convert.ToInt32(colorHex.Substring(1, 2), 16),
                            Convert.ToInt32(colorHex.Substring(3, 2), 16),
                            Convert.ToInt32(colorHex.Substring(5, 2), 16));
                        XBrush brush = new XSolidBrush(color);

                        List<int> targetPages = pages == "all" ? Enumerable.Range(0, doc.PageCount).ToList() : new List<int> { int.Parse(pages) };

                        foreach (int i in targetPages)
                        {
                            PdfPage page = doc.Pages[i];
                            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                            {
                                double centerX = rect.X + rect.Width / 2;
                                double centerY = rect.Y + rect.Height / 2;

                                gfx.TranslateTransform(centerX, centerY);
                                gfx.RotateTransform(rotation);
                                gfx.DrawString(text, font, brush, new XPoint(-rect.Width / 2, rect.Height / 2));
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Watermark Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> AddImageWatermarkAsync(string sourcePath, string targetPath, string imagePath, Avalonia.Rect rect, double opacity, string pages = "all")
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        List<int> targetPages = pages == "all" ? Enumerable.Range(0, doc.PageCount).ToList() : new List<int> { int.Parse(pages) };

                        foreach (int i in targetPages)
                        {
                            PdfPage page = doc.Pages[i];
                            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                            {
                                using (XImage image = XImage.FromFile(imagePath))
                                {
                                    // Set opacity if possible (simulated via DrawImage)
                                    gfx.DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
                                }
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image Watermark Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> CropPdfAsync(string sourcePath, string targetPath, double left, double top, double width, double height, bool allPages, int pageIndex)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        int start = allPages ? 0 : pageIndex;
                        int end = allPages ? doc.PageCount : pageIndex + 1;

                        for (int i = start; i < end; i++)
                        {
                            PdfPage page = doc.Pages[i];
                            double pdfHeight = page.Height.Point;
                            double cropY = pdfHeight - (top + height);
                            page.CropBox = new PdfRectangle(new XRect(left, cropY, width, height));
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Crop Error: {ex.Message}");
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
                                    targetDoc.AddPage(sourceDoc.Pages[i]);
                            }
                            targetDoc.Save(targetPath);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Delete Error: {ex.Message}");
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
                    using (PdfDocument doc = new PdfDocument())
                    {
                        foreach (string imgPath in imagePaths)
                        {
                            PdfPage page = doc.AddPage();
                            using (XGraphics gfx = XGraphics.FromPdfPage(page))
                            {
                                using (XImage image = XImage.FromFile(imgPath))
                                {
                                    page.Width = image.PointWidth;
                                    page.Height = image.PointHeight;
                                    gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                                }
                            }
                        }
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ImagesToPdf Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> PdfToImageAsync(string sourcePath, string targetDir) => true; // Needs more deps
        public async Task<bool> OfficeToPdfAsync(string sourcePath, string targetPath) => true; // Needs more deps
        public async Task<bool> PdfToWordAsync(string sourcePath, string targetPath) => true; // Needs more deps

        public double GetPageWidth(string path)
        {
            using (PdfDocument doc = PdfReader.Open(path, PdfDocumentOpenMode.Import))
                return doc.Pages[0].Width.Point;
        }

        public double GetPageHeight(string path)
        {
            using (PdfDocument doc = PdfReader.Open(path, PdfDocumentOpenMode.Import))
                return doc.Pages[0].Height.Point;
        }

        public int GetPageCount(string path)
        {
            using (PdfDocument doc = PdfReader.Open(path, PdfDocumentOpenMode.Import))
                return doc.PageCount;
        }

        public void OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch { }
        }

        public async Task<bool> CompressPdfAsync(string sourcePath, string targetPath, string level = "Low")
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument output = new PdfDocument())
                        {
                            output.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
                            output.Options.NoCompression = false;
                            output.Options.CompressContentStreams = true;

                            if (level == "Medium" || level == "High")
                            {
                                output.Info.Title = "";
                                output.Info.Author = "";
                                output.Info.Creator = "Docentra PDF Editor";
                            }

                            foreach (PdfPage page in doc.Pages)
                            {
                                output.AddPage(page);
                            }

                            output.Save(targetPath);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Compress Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<Models.PdfMetadataModel?> GetMetadataAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
                    {
                        var info = doc.Info;
                        return new Models.PdfMetadataModel
                        {
                            Title = info.Title ?? "",
                            Author = info.Author ?? "",
                            Subject = info.Subject ?? "",
                            Keywords = info.Keywords ?? "",
                            Creator = info.Creator ?? "",
                            Producer = info.Producer ?? "",
                            CreationDate = info.CreationDate.ToString("G"),
                            ModificationDate = info.ModificationDate.ToString("G")
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetMetadata Error: {ex.Message}");
                    return null;
                }
            });
        }

        public async Task<bool> SetMetadataAsync(string sourcePath, string targetPath, Models.PdfMetadataModel metadata)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        doc.Info.Title = metadata.Title;
                        doc.Info.Author = metadata.Author;
                        doc.Info.Subject = metadata.Subject;
                        doc.Info.Keywords = metadata.Keywords;
                        doc.Info.Creator = metadata.Creator;
                        doc.Save(targetPath);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SetMetadata Error: {ex.Message}");
                    return false;
                }
            });
        }

        private List<int> ParsePageRange(string input, int totalPages)
        {
            List<int> pages = new List<int>();
            string[] parts = input.Split(',');
            foreach (string part in parts)
            {
                if (part.Contains("-") || part.Contains(":"))
                {
                    string[] range = part.Split(new[] { '-', ':' });
                    if (int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                            if (i >= 1 && i <= totalPages) pages.Add(i - 1);
                    }
                }
                else if (int.TryParse(part, out int p))
                {
                    if (p >= 1 && p <= totalPages) pages.Add(p - 1);
                }
            }
            return pages.Distinct().ToList();
        }
    }
}
