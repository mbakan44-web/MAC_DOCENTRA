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
                {"Nav_Metadata", "Metadata"}, {"Metadata_Title", "Title"}, {"Metadata_Author", "Author"}, {"Metadata_Subject", "Subject"},
                {"Metadata_Keywords", "Keywords"}, {"Metadata_Creator", "Creator"}, {"Metadata_Producer", "Producer"}, 
                {"Metadata_CreationDate", "Creation Date:"}, {"Metadata_ModificationDate", "Modification Date:"},
                {"Metadata_SaveBtn", "Save Metadata"}, {"Metadata_SuccessMsg", "Metadata saved successfully."}
            }},
            { "tr-TR", new Dictionary<string, string> {
                {"Nav_Metadata", "Metadata"}, {"Metadata_Title", "Başlık"}, {"Metadata_Author", "Yazar"}, {"Metadata_Subject", "Konu"},
                {"Metadata_Keywords", "Anahtar Kelimeler"}, {"Metadata_Creator", "Oluşturan"}, {"Metadata_Producer", "Üretici"}, 
                {"Metadata_CreationDate", "Oluşturulma Tarihi:"}, {"Metadata_ModificationDate", "Değiştirilme Tarihi:"},
                {"Metadata_SaveBtn", "Metadatayı Kaydet"}, {"Metadata_SuccessMsg", "Metadata başarıyla kaydedildi."}
            }},
            { "de-DE", new Dictionary<string, string> {
                {"Nav_Metadata", "Metadaten"}, {"Metadata_Title", "Titel"}, {"Metadata_Author", "Autor"}, {"Metadata_Subject", "Betreff"},
                {"Metadata_Keywords", "Schlüsselwörter"}, {"Metadata_Creator", "Ersteller"}, {"Metadata_Producer", "Produzent"}, 
                {"Metadata_CreationDate", "Erstellungsdatum:"}, {"Metadata_ModificationDate", "Änderungsdatum:"},
                {"Metadata_SaveBtn", "Metadaten speichern"}, {"Metadata_SuccessMsg", "Metadaten erfolgreich gespeichert."}
            }},
            { "es-ES", new Dictionary<string, string> {
                {"Nav_Metadata", "Metadatos"}, {"Metadata_Title", "Título"}, {"Metadata_Author", "Autor"}, {"Metadata_Subject", "Asunto"},
                {"Metadata_Keywords", "Palabras clave"}, {"Metadata_Creator", "Creador"}, {"Metadata_Producer", "Productor"}, 
                {"Metadata_CreationDate", "Fecha de creación:"}, {"Metadata_ModificationDate", "Fecha de modificación:"},
                {"Metadata_SaveBtn", "Guardar metadatos"}, {"Metadata_SuccessMsg", "Metadatos guardados con éxito."}
            }},
            { "fr-FR", new Dictionary<string, string> {
                {"Nav_Metadata", "Métadonnées"}, {"Metadata_Title", "Titre"}, {"Metadata_Author", "Auteur"}, {"Metadata_Subject", "Sujet"},
                {"Metadata_Keywords", "Mots-clés"}, {"Metadata_Creator", "Créateur"}, {"Metadata_Producer", "Producteur"}, 
                {"Metadata_CreationDate", "Date de création:"}, {"Metadata_ModificationDate", "Date de modification:"},
                {"Metadata_SaveBtn", "Enregistrer les métadonnées"}, {"Metadata_SuccessMsg", "Métadonnées enregistrées avec succès."}
            }},
            { "it-IT", new Dictionary<string, string> {
                {"Nav_Metadata", "Metadati"}, {"Metadata_Title", "Titolo"}, {"Metadata_Author", "Autore"}, {"Metadata_Subject", "Soggetto"},
                {"Metadata_Keywords", "Parole chiave"}, {"Metadata_Creator", "Creatore"}, {"Metadata_Producer", "Produttore"}, 
                {"Metadata_CreationDate", "Data di creazione:"}, {"Metadata_ModificationDate", "Data di modifica:"},
                {"Metadata_SaveBtn", "Salva metadati"}, {"Metadata_SuccessMsg", "Metadati salvati con successo."}
            }},
            { "pt-PT", new Dictionary<string, string> {
                {"Nav_Metadata", "Metadados"}, {"Metadata_Title", "Título"}, {"Metadata_Author", "Autor"}, {"Metadata_Subject", "Assunto"},
                {"Metadata_Keywords", "Palavras-chave"}, {"Metadata_Creator", "Criador"}, {"Metadata_Producer", "Produtor"}, 
                {"Metadata_CreationDate", "Data de criação:"}, {"Metadata_ModificationDate", "Data de modificação:"},
                {"Metadata_SaveBtn", "Salvar Metadados"}, {"Metadata_SuccessMsg", "Metadados salvos com sucesso."}
            }},
            { "ru-RU", new Dictionary<string, string> {
                {"Nav_Metadata", "Метаданные"}, {"Metadata_Title", "Заголовок"}, {"Metadata_Author", "Автор"}, {"Metadata_Subject", "Тема"},
                {"Metadata_Keywords", "Ключевые слова"}, {"Metadata_Creator", "Создатель"}, {"Metadata_Producer", "Производитель"}, 
                {"Metadata_CreationDate", "Дата создания:"}, {"Metadata_ModificationDate", "Дата изменения:"},
                {"Metadata_SaveBtn", "Сохранить метаданные"}, {"Metadata_SuccessMsg", "Метаданные успешно сохранены."}
            }},
            { "zh-CN", new Dictionary<string, string> {
                {"Nav_Metadata", "元数据"}, {"Metadata_Title", "标题"}, {"Metadata_Author", "作者"}, {"Metadata_Subject", "主题"},
                {"Metadata_Keywords", "关键字"}, {"Metadata_Creator", "创建者"}, {"Metadata_Producer", "生产者"}, 
                {"Metadata_CreationDate", "创建日期:"}, {"Metadata_ModificationDate", "修改日期:"},
                {"Metadata_SaveBtn", "保存元数据"}, {"Metadata_SuccessMsg", "元数据已成功保存。"}
            }},
            { "ja-JP", new Dictionary<string, string> {
                {"Nav_Metadata", "メタデータ"}, {"Metadata_Title", "タイトル"}, {"Metadata_Author", "作成者"}, {"Metadata_Subject", "件名"},
                {"Metadata_Keywords", "キーワード"}, {"Metadata_Creator", "クリエイター"}, {"Metadata_Producer", "プロデューサー"}, 
                {"Metadata_CreationDate", "作成日:"}, {"Metadata_ModificationDate", "変更日:"},
                {"Metadata_SaveBtn", "メタデータを保存"}, {"Metadata_SuccessMsg", "メタデータが正常に保存されました。"}
            }}
        };

        foreach (var file in Directory.GetFiles(localesDir, "*.xaml"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (translations.ContainsKey(fileName))
            {
                var dict = translations[fileName];
                string content = File.ReadAllText(file);
                
                if (content.Contains("Nav_Metadata")) continue;

                string newEntries = "\n    <!-- Metadata -->\n";
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
