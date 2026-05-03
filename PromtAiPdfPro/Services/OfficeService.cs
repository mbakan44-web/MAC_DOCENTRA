using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PromtAiPdfPro.Services
{
    public class OfficeService
    {
        private static readonly object _logLock = new object();

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
                    File.AppendAllText("crash_log.txt", $"[{DateTime.Now}] OfficeService: {message}\n");
                }
            }
            catch { }
        }

        /// <summary>
        /// Word belgesindeki içerikleri (tablolar, metinler, biçimlendirmeler) Office panosunu kullanarak Excel'e aktarır.
        /// </summary>
        public async Task<bool> WordToExcelAsync(string wordPath, string excelPath)
        {
            return await RunInSTAAsync(() =>
            {
                dynamic wordApp = null;
                dynamic wordDocs = null;
                dynamic wordDoc = null;
                dynamic excelApp = null;
                dynamic excelWorkbooks = null;
                dynamic excelWorkbook = null;
                dynamic excelSheet = null;

                try
                {
                    Type wordType = Type.GetTypeFromProgID("Word.Application");
                    Type excelType = Type.GetTypeFromProgID("Excel.Application");

                    if (wordType == null || excelType == null)
                    {
                        LogMessage("Word veya Excel yüklü değil.");
                        return false;
                    }

                    wordApp = Activator.CreateInstance(wordType);
                    excelApp = Activator.CreateInstance(excelType);

                    wordApp.Visible = false;
                    excelApp.Visible = false;
                    excelApp.DisplayAlerts = false;

                    wordDocs = wordApp.Documents;
                    wordDoc = wordDocs.Open(wordPath, ReadOnly: true);

                    excelWorkbooks = excelApp.Workbooks;
                    excelWorkbook = excelWorkbooks.Add();
                    excelSheet = excelWorkbook.Sheets[1];

                    // Word belgesinin tamamını kopyala
                    wordDoc.Content.Copy();
                    
                    // Excel'e yapıştır (Tüm tablo ve biçimlendirmeler korunur)
                    excelSheet.Paste();

                    // Sütunları otomatik genişlet
                    excelSheet.Columns.AutoFit();

                    excelWorkbook.SaveAs(excelPath, 51); // 51 = xlOpenXMLWorkbook (.xlsx)
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"WordToExcel Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    if (wordDoc != null) { wordDoc.Close(false); Marshal.ReleaseComObject(wordDoc); }
                    if (wordDocs != null) Marshal.ReleaseComObject(wordDocs);
                    if (wordApp != null) { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); }

                    if (excelWorkbook != null) { excelWorkbook.Close(false); Marshal.ReleaseComObject(excelWorkbook); }
                    if (excelWorkbooks != null) Marshal.ReleaseComObject(excelWorkbooks);
                    if (excelApp != null) { excelApp.Quit(); Marshal.ReleaseComObject(excelApp); }
                }
            });
        }

        /// <summary>
        /// Excel'deki dolu alanı (UsedRange) Office panosu ile Word'e gerçek bir tablo olarak aktarır.
        /// </summary>
        public async Task<bool> ExcelToWordAsync(string excelPath, string wordPath)
        {
            return await RunInSTAAsync(() =>
            {
                dynamic excelApp = null;
                dynamic excelWorkbooks = null;
                dynamic excelWorkbook = null;
                dynamic excelSheet = null;
                dynamic wordApp = null;
                dynamic wordDocs = null;
                dynamic wordDoc = null;

                try
                {
                    Type excelType = Type.GetTypeFromProgID("Excel.Application");
                    Type wordType = Type.GetTypeFromProgID("Word.Application");

                    if (excelType == null || wordType == null)
                    {
                        LogMessage("Excel veya Word yüklü değil.");
                        return false;
                    }

                    excelApp = Activator.CreateInstance(excelType);
                    wordApp = Activator.CreateInstance(wordType);

                    excelApp.Visible = false;
                    excelApp.DisplayAlerts = false;
                    wordApp.Visible = false;

                    excelWorkbooks = excelApp.Workbooks;
                    excelWorkbook = excelWorkbooks.Open(excelPath, ReadOnly: true);
                    excelSheet = excelWorkbook.Sheets[1];

                    wordDocs = wordApp.Documents;
                    wordDoc = wordDocs.Add();
                    dynamic selection = wordApp.Selection;

                    // Excel'deki dolu alanı kopyala
                    excelSheet.UsedRange.Copy();

                    // Word sayfa yönünü Dikey (Portrait) yap (0 = wdOrientPortrait)
                    wordDoc.PageSetup.Orientation = 0;

                    // Excel verisini tablo olarak değil, sadece düz metin olarak (2 = wdPasteText) yapıştır.
                    // Bu sayede 1 sayfalık veri onlarca sayfaya bölünmez, sıkıştırılmış metin olarak aktarılır.
                    selection.PasteSpecial(DataType: 2);

                    // 12 = wdFormatXMLDocument (.docx)
                    wordDoc.SaveAs2(wordPath, 12);
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"ExcelToWord Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    if (excelSheet != null) Marshal.ReleaseComObject(excelSheet);
                    if (excelWorkbook != null) { excelWorkbook.Close(false); Marshal.ReleaseComObject(excelWorkbook); }
                    if (excelWorkbooks != null) Marshal.ReleaseComObject(excelWorkbooks);
                    if (excelApp != null) { excelApp.Quit(); Marshal.ReleaseComObject(excelApp); }

                    if (wordDoc != null) { wordDoc.Close(false); Marshal.ReleaseComObject(wordDoc); }
                    if (wordDocs != null) Marshal.ReleaseComObject(wordDocs);
                    if (wordApp != null) { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); }
                }
            });
        }

        /// <summary>
        /// PowerPoint sunumundaki metinleri okuyup Word belgesine aktarır.
        /// </summary>
        public async Task<bool> PowerPointToWordAsync(string pptPath, string wordPath)
        {
            return await RunInSTAAsync(() =>
            {
                dynamic pptApp = null;
                dynamic pptPres = null;
                dynamic wordApp = null;
                dynamic wordDocs = null;
                dynamic wordDoc = null;

                try
                {
                    Type pptType = Type.GetTypeFromProgID("PowerPoint.Application");
                    Type wordType = Type.GetTypeFromProgID("Word.Application");

                    if (pptType == null || wordType == null)
                    {
                        LogMessage("PowerPoint veya Word yüklü değil.");
                        return false;
                    }

                    pptApp = Activator.CreateInstance(pptType);
                    wordApp = Activator.CreateInstance(wordType);

                    wordApp.Visible = false;
                    // PowerPoint often requires Visible = msoTrue to manipulate objects reliably via COM
                    // Let's hide the window or minimize it if possible, but for reliability we just open it.
                    // We'll use withWindow: false in Open.
                    
                    pptPres = pptApp.Presentations.Open(pptPath, ReadOnly: true, WithWindow: false);

                    wordDocs = wordApp.Documents;
                    wordDoc = wordDocs.Add();
                    dynamic selection = wordApp.Selection;

                    int slideCount = pptPres.Slides.Count;
                    for (int i = 1; i <= slideCount; i++)
                    {
                        dynamic slide = pptPres.Slides[i];
                        
                        selection.Font.Bold = 1;
                        selection.TypeText($"--- Slayt {i} ---\n");
                        selection.Font.Bold = 0;

                        foreach (dynamic shape in slide.Shapes)
                        {
                            try
                            {
                                // msoTrue is -1, msoFalse is 0
                                if (shape.HasTextFrame != 0)
                                {
                                    string text = shape.TextFrame.TextRange.Text;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        selection.TypeText(text.TrimEnd() + "\n\n");
                                    }
                                }
                            }
                            catch { }
                        }
                        selection.TypeText("\n");
                    }

                    // 12 = wdFormatXMLDocument (.docx)
                    wordDoc.SaveAs2(wordPath, 12);
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"PowerPointToWord Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    if (pptPres != null) { pptPres.Close(); Marshal.ReleaseComObject(pptPres); }
                    if (pptApp != null) { pptApp.Quit(); Marshal.ReleaseComObject(pptApp); }

                    if (wordDoc != null) { wordDoc.Close(false); Marshal.ReleaseComObject(wordDoc); }
                    if (wordDocs != null) Marshal.ReleaseComObject(wordDocs);
                    if (wordApp != null) { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); }
                }
            });
        }

        /// <summary>
        /// PowerPoint sunumundaki metinleri okuyup Excel sayfasına aktarır.
        /// </summary>
        public async Task<bool> PowerPointToExcelAsync(string pptPath, string excelPath)
        {
            return await RunInSTAAsync(() =>
            {
                dynamic pptApp = null;
                dynamic pptPres = null;
                dynamic excelApp = null;
                dynamic excelWorkbooks = null;
                dynamic excelWorkbook = null;
                dynamic excelSheet = null;

                try
                {
                    Type pptType = Type.GetTypeFromProgID("PowerPoint.Application");
                    Type excelType = Type.GetTypeFromProgID("Excel.Application");

                    if (pptType == null || excelType == null)
                    {
                        LogMessage("PowerPoint veya Excel yüklü değil.");
                        return false;
                    }

                    pptApp = Activator.CreateInstance(pptType);
                    excelApp = Activator.CreateInstance(excelType);

                    if (excelApp != null)
                    {
                        excelApp.Visible = false;
                        excelApp.DisplayAlerts = false;
                    }

                    if (excelApp is null || pptApp is null) return false;

                    if (pptApp != null)
                    {
                        pptPres = pptApp.Presentations.Open(pptPath, ReadOnly: true, WithWindow: false);
                    }

                    if (pptPres == null) return false;

#pragma warning disable CS8602
                    excelWorkbooks = excelApp.Workbooks;
                    excelWorkbook = excelWorkbooks.Add();
                    excelSheet = excelWorkbook.Sheets[1];

                    int row = 1;
                    int slideCount = pptPres.Slides.Count;
#pragma warning restore CS8602
                    
                    for (int i = 1; i <= slideCount; i++)
                    {
                        dynamic slide = pptPres.Slides[i];
                        
                        // Slayt başlığı (Kalın)
                        excelSheet.Cells[row, 1].Value = $"--- Slayt {i} ---";
                        excelSheet.Cells[row, 1].Font.Bold = true;
                        row++;

                        foreach (dynamic shape in slide.Shapes)
                        {
                            try
                            {
                                if (shape.HasTextFrame != 0)
                                {
                                    string text = shape.TextFrame.TextRange.Text;
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        excelSheet.Cells[row, 1].Value = text.TrimEnd();
                                        row++;
                                    }
                                }
                            }
                            catch { }
                        }
                        row++; // Boş bir satır bırak
                    }

                    excelSheet.Columns.AutoFit();

                    // 51 = xlOpenXMLWorkbook (.xlsx)
                    excelWorkbook.SaveAs(excelPath, 51);
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"PowerPointToExcel Error: {ex.Message}");
                    return false;
                }
                finally
                {
                    if (pptPres != null) { pptPres.Close(); Marshal.ReleaseComObject(pptPres); }
                    if (pptApp != null) { pptApp.Quit(); Marshal.ReleaseComObject(pptApp); }

                    if (excelSheet != null) Marshal.ReleaseComObject(excelSheet);
                    if (excelWorkbook != null) { excelWorkbook.Close(false); Marshal.ReleaseComObject(excelWorkbook); }
                    if (excelWorkbooks != null) Marshal.ReleaseComObject(excelWorkbooks);
                    if (excelApp != null) { excelApp.Quit(); Marshal.ReleaseComObject(excelApp); }
                }
            });
        }
        /// <summary>
        /// Tüm açık Word, Excel ve PowerPoint süreçlerini sonlandırır.
        /// </summary>
        public void KillOfficeProcesses()
        {
            string[] processes = { "WINWORD", "EXCEL", "POWERPNT" };
            foreach (var processName in processes)
            {
                var runningProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (var process in runningProcesses)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }
            }
        }
    }
}
