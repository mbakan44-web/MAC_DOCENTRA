using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string localesDir = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\DOCENTRA PDF EDITOR\PromtAiPdfPro\Locales";
        
        var translations = new Dictionary<string, Dictionary<string, string>>
        {
            { "en-US", new Dictionary<string, string> {
                {"Tool_SelectFile", "Select File"}, {"Tool_SelectFileDesc", "Click or drag & drop a PDF file here."}, {"Tool_Browse", "Browse"}
            }},
            { "tr-TR", new Dictionary<string, string> {
                {"Tool_SelectFile", "Dosya Seç"}, {"Tool_SelectFileDesc", "PDF dosyasını buraya sürükleyin veya tıklayarak seçin."}, {"Tool_Browse", "Gözat"}
            }},
            { "de-DE", new Dictionary<string, string> {
                {"Tool_SelectFile", "Datei auswählen"}, {"Tool_SelectFileDesc", "Klicken oder ziehen Sie eine PDF-Datei hierher."}, {"Tool_Browse", "Durchsuchen"}
            }},
            { "es-ES", new Dictionary<string, string> {
                {"Tool_SelectFile", "Seleccionar archivo"}, {"Tool_SelectFileDesc", "Haga clic o arrastre y suelte un archivo PDF aquí."}, {"Tool_Browse", "Navegar"}
            }},
            { "fr-FR", new Dictionary<string, string> {
                {"Tool_SelectFile", "Sélectionner un fichier"}, {"Tool_SelectFileDesc", "Cliquez ou glissez-déposez un fichier PDF ici."}, {"Tool_Browse", "Parcourir"}
            }},
            { "it-IT", new Dictionary<string, string> {
                {"Tool_SelectFile", "Seleziona file"}, {"Tool_SelectFileDesc", "Fai clic o trascina e rilascia qui un file PDF."}, {"Tool_Browse", "Sfoglia"}
            }},
            { "pt-PT", new Dictionary<string, string> {
                {"Tool_SelectFile", "Selecionar ficheiro"}, {"Tool_SelectFileDesc", "Clique ou arraste e largue um ficheiro PDF aqui."}, {"Tool_Browse", "Navegar"}
            }},
            { "ru-RU", new Dictionary<string, string> {
                {"Tool_SelectFile", "Выбрать файл"}, {"Tool_SelectFileDesc", "Нажмите или перетащите PDF-файл сюда."}, {"Tool_Browse", "Обзор"}
            }},
            { "zh-CN", new Dictionary<string, string> {
                {"Tool_SelectFile", "选择文件"}, {"Tool_SelectFileDesc", "单击或将PDF文件拖放到此处。"}, {"Tool_Browse", "浏览"}
            }},
            { "ja-JP", new Dictionary<string, string> {
                {"Tool_SelectFile", "ファイルを選択"}, {"Tool_SelectFileDesc", "クリックするか、PDFファイルをここにドラッグ＆ドロップしてください。"}, {"Tool_Browse", "参照"}
            }},
            { "ar-SA", new Dictionary<string, string> {
                {"Tool_SelectFile", "اختر ملفًا"}, {"Tool_SelectFileDesc", "انقر أو اسحب وأفلت ملف PDF هنا."}, {"Tool_Browse", "تصفح"}
            }}
        };

        foreach (var file in Directory.GetFiles(localesDir, "*.xaml"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (translations.ContainsKey(fileName))
            {
                var dict = translations[fileName];
                string content = File.ReadAllText(file);
                
                if (content.Contains("Tool_SelectFile")) continue;

                string newEntries = "\n    <!-- File Selection -->\n";
                foreach (var kvp in dict)
                {
                    newEntries += $"    <system:String x:Key=\"{kvp.Key}\">{kvp.Value}</system:String>\n";
                }

                content = content.Replace("</ResourceDictionary>", newEntries + "</ResourceDictionary>");
                File.WriteAllText(file, content);
                Console.WriteLine($"Updated {fileName}");
            }
        }
    }
}
