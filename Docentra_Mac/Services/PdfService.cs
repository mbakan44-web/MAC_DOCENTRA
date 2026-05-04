using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace Docentra_Mac.Services
{
    public class PdfService
    {
        public int GetPageCount(string filePath)
        {
            try
            {
                using (var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
                {
                    return inputDocument.PageCount;
                }
            }
            catch { return 0; }
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
                    Console.WriteLine($"Merge Error: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> SplitPagesAsync(string sourcePath, string? rangeInput = null)
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
                            for (int i = 0; i < sourceDoc.PageCount; i++)
                            {
                                string outPath = Path.Combine(dir, $"{name}_Page_{i + 1}.pdf");
                                using (PdfDocument targetDoc = new PdfDocument())
                                {
                                    targetDoc.AddPage(sourceDoc.Pages[i]);
                                    targetDoc.Save(outPath);
                                }
                            }
                        }
                        else
                        {
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
                    Console.WriteLine($"Split Error: {ex.Message}");
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

            var parts = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains("-") || trimmed.Contains(":"))
                {
                    var rangeSeparator = trimmed.Contains("-") ? '-' : ':';
                    var range = trimmed.Split(rangeSeparator);
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

        public async Task<bool> AddTextWatermarkAsync(string sourcePath, string targetPath, string watermarkText, double opacity, double rotation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont("Helvetica", 40, XFontStyleEx.Bold);
                        int alpha = (int)(opacity * 255);
                        XBrush brush = new XSolidBrush(XColor.FromArgb(alpha, 128, 128, 128));

                        foreach (PdfPage page in doc.Pages)
                        {
                            using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                            {
                                var size = gfx.MeasureString(watermarkText, font);
                                
                                gfx.TranslateTransform(page.Width.Point / 2, page.Height.Point / 2);
                                gfx.RotateTransform(rotation);
                                gfx.DrawString(watermarkText, font, brush, new XPoint(-size.Width / 2, size.Height / 2));
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
        public async Task<bool> DeletePagesAsync(string sourcePath, string targetPath, string rangeInput)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        var pagesToDelete = ParsePageRange(rangeInput, sourceDoc.PageCount);
                        if (pagesToDelete.Count == 0) return false;

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
                    Console.WriteLine($"Delete Error: {ex.Message}");
                    return false;
                }
            });
        }
        public async Task<bool> ProtectPdfAsync(string sourcePath, string targetPath, string userPassword, string ownerPassword)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument sourceDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument targetDoc = new PdfDocument())
                        {
                            foreach (PdfPage page in sourceDoc.Pages)
                            {
                                targetDoc.AddPage(page);
                            }

                            targetDoc.SecuritySettings.UserPassword = userPassword;
                            targetDoc.SecuritySettings.OwnerPassword = ownerPassword;
                            
                            // Set basic permissions
                            targetDoc.SecuritySettings.PermitAccessibilityExtractContent = false;
                            targetDoc.SecuritySettings.PermitAnnotations = false;
                            targetDoc.SecuritySettings.PermitAssembleDocument = false;
                            targetDoc.SecuritySettings.PermitExtractContent = false;
                            targetDoc.SecuritySettings.PermitFormsFill = false;
                            targetDoc.SecuritySettings.PermitFullQualityPrint = false;
                            targetDoc.SecuritySettings.PermitModifyDocument = false;
                            targetDoc.SecuritySettings.PermitPrint = false;

                            targetDoc.Save(targetPath);
                        }
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
        public async Task<bool> AddPageNumbersAsync(string sourcePath, string targetPath, string position)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument doc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify))
                    {
                        XFont font = new XFont("Helvetica", 10, XFontStyleEx.Regular);
                        XBrush brush = XBrushes.Black;

                        for (int i = 0; i < doc.PageCount; i++)
                        {
                            PdfPage page = doc.Pages[i];
                            using (XGraphics gfx = XGraphics.FromPdfPage(page))
                            {
                                string text = $"Page {i + 1} of {doc.PageCount}";
                                XSize size = gfx.MeasureString(text, font);
                                double x = 0, y = 0;

                                switch (position)
                                {
                                    case "BottomCenter":
                                        x = (page.Width.Point - size.Width) / 2;
                                        y = page.Height.Point - 30;
                                        break;
                                    case "BottomRight":
                                        x = page.Width.Point - size.Width - 30;
                                        y = page.Height.Point - 30;
                                        break;
                                    case "TopCenter":
                                        x = (page.Width.Point - size.Width) / 2;
                                        y = 40;
                                        break;
                                }

                                gfx.DrawString(text, font, brush, x, y);
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
    }
}
