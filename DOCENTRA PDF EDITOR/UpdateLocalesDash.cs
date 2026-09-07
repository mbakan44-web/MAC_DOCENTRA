using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string localesDir = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\DOCENTRA PDF EDITOR\PromtAiPdfPro\Locales";
        
        var translations = new Dictionary<string, Dictionary<string, string>>
        {
            { "en-US", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Metadata Editor"}, {"Dash_MetadataSub", "View and edit PDF properties and metadata."}
            }},
            { "tr-TR", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Metadata Editörü"}, {"Dash_MetadataSub", "PDF özelliklerini ve metadatasını görüntüleyip düzenleyin."}
            }},
            { "de-DE", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Metadaten-Editor"}, {"Dash_MetadataSub", "PDF-Eigenschaften und Metadaten anzeigen und bearbeiten."}
            }},
            { "es-ES", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Editor de metadatos"}, {"Dash_MetadataSub", "Ver y editar propiedades y metadatos del PDF."}
            }},
            { "fr-FR", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Éditeur de métadonnées"}, {"Dash_MetadataSub", "Afficher et modifier les propriétés et les métadonnées du PDF."}
            }},
            { "it-IT", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Editor metadati"}, {"Dash_MetadataSub", "Visualizza e modifica le proprietà e i metadati del PDF."}
            }},
            { "pt-PT", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Editor de metadados"}, {"Dash_MetadataSub", "Ver e editar propriedades e metadados de PDF."}
            }},
            { "ru-RU", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "Редактор метаданных"}, {"Dash_MetadataSub", "Просмотр и редактирование свойств и метаданных PDF."}
            }},
            { "zh-CN", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "元数据编辑器"}, {"Dash_MetadataSub", "查看和编辑PDF属性和元数据。"}
            }},
            { "ja-JP", new Dictionary<string, string> {
                {"Dash_MetadataTitle", "メタデータエディター"}, {"Dash_MetadataSub", "PDFのプロパティとメタデータを表示および編集します。"}
            }}
        };

        foreach (var file in Directory.GetFiles(localesDir, "*.xaml"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (translations.ContainsKey(fileName))
            {
                var dict = translations[fileName];
                string content = File.ReadAllText(file);
                
                if (content.Contains("Dash_MetadataTitle")) continue;

                string newEntries = "\n    <!-- Dash Metadata -->\n";
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
