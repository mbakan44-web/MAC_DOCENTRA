$localesDir = "c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\DOCENTRA PDF EDITOR\PromtAiPdfPro\Locales"
$files = Get-ChildItem -Path $localesDir -Filter "*.xaml"

$translations = @{
    "ar-SA" = @{ Nav="ضغط PDF"; DashTitle="ضغط PDF"; DashSub="تقليل حجم ملفات PDF الكبيرة دون فقدان الجودة."; CompTitle="ضغط PDF"; CompDesc="تحسين ملفات PDF الكبيرة لتقليل حجمها."; CompSelect="اختر ملف PDF للضغط"; CompBtn="ضغط وحفظ"; CompSuccess="تم ضغط ملف PDF بنجاح!"; CompSuccessWithOpen="تمت عملية الضغط بنجاح. هل تريد فتح الملف؟" }
    "de-DE" = @{ Nav="PDF komprimieren"; DashTitle="PDF komprimieren"; DashSub="Reduzieren Sie die Größe großer PDF-Dateien ohne Qualitätsverlust."; CompTitle="PDF-Komprimierung"; CompDesc="Optimieren Sie Ihre großen PDF-Dateien, um deren Größe zu reduzieren."; CompSelect="Zu komprimierende PDF auswählen"; CompBtn="Komprimieren und Speichern"; CompSuccess="PDF erfolgreich komprimiert!"; CompSuccessWithOpen="Komprimierung erfolgreich. Möchten Sie die Datei öffnen?" }
    "es-ES" = @{ Nav="Comprimir PDF"; DashTitle="Comprimir PDF"; DashSub="Reduzca el tamaño de archivos PDF grandes sin perder calidad."; CompTitle="Compresión de PDF"; CompDesc="Optimice sus archivos PDF grandes para reducir su tamaño."; CompSelect="Seleccionar PDF para comprimir"; CompBtn="Comprimir y guardar"; CompSuccess="¡PDF comprimido con éxito!"; CompSuccessWithOpen="Compresión exitosa. ¿Desea abrir el archivo?" }
    "fr-FR" = @{ Nav="Compresser le PDF"; DashTitle="Compresser le PDF"; DashSub="Réduisez la taille des fichiers PDF volumineux sans perte de qualité."; CompTitle="Compression de PDF"; CompDesc="Optimisez vos fichiers PDF volumineux pour réduire leur taille."; CompSelect="Sélectionner le PDF à compresser"; CompBtn="Compresser et enregistrer"; CompSuccess="PDF compressé avec succès !"; CompSuccessWithOpen="Compression réussie. Voulez-vous ouvrir le fichier ?" }
    "it-IT" = @{ Nav="Comprimi PDF"; DashTitle="Comprimi PDF"; DashSub="Riduci le dimensioni dei file PDF di grandi dimensioni senza perdere qualità."; CompTitle="Compressione PDF"; CompDesc="Ottimizza i tuoi file PDF di grandi dimensioni per ridurne le dimensioni."; CompSelect="Seleziona PDF da comprimere"; CompBtn="Comprimi e salva"; CompSuccess="PDF compresso con successo!"; CompSuccessWithOpen="Compressione completata. Vuoi aprire il file?" }
    "ja-JP" = @{ Nav="PDFを圧縮"; DashTitle="PDFを圧縮"; DashSub="品質を落とさずに大きなPDFファイルのサイズを縮小します."; CompTitle="PDF圧縮"; CompDesc="大きなPDFファイルを最適化してサイズを縮小します。"; CompSelect="圧縮するPDFを選択"; CompBtn="圧縮して保存"; CompSuccess="PDFが正常に圧縮されました！"; CompSuccessWithOpen="圧縮が完了しました。ファイルを開きますか？" }
    "ru-RU" = @{ Nav="Сжать PDF"; DashTitle="Сжать PDF"; DashSub="Уменьшите размер больших PDF-файлов без потери качества."; CompTitle="Сжатие PDF"; CompDesc="Оптимизируйте большие PDF-файлы, чтобы уменьшить их размер."; CompSelect="Выберите PDF для сжатия"; CompBtn="Сжать и сохранить"; CompSuccess="PDF успешно сжат!"; CompSuccessWithOpen="Сжатие выполнено. Хотите открыть файл?" }
    "zh-CN" = @{ Nav="压缩 PDF"; DashTitle="压缩 PDF"; DashSub="在不损失质量的情况下减小大 PDF 文件的大小。"; CompTitle="PDF 压缩"; CompDesc="优化大 PDF 文件以减小其大小。"; CompSelect="选择要压缩的 PDF"; CompBtn="压缩并保存"; CompSuccess="PDF 压缩成功！"; CompSuccessWithOpen="压缩成功。是否要打开文件？" }
}

foreach ($file in $files) {
    $lang = $file.BaseName
    if (-not $translations.ContainsKey($lang)) { continue }
    $t = $translations[$lang]

    $content = Get-Content $file.FullName -Raw -Encoding UTF8

    # Clean existing
    $content = $content -replace '(?s)\s*<system:String x:Key="Nav_Compress">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Dash_CompressTitle">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Dash_CompressSub">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<!-- Compress Tool -->.*?(?=\r?\n\s*</ResourceDictionary>)', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_Title">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_Desc">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_SelectFile">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_ProcessBtn">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_Success">.*?</system:String>', ''
    $content = $content -replace '(?s)\s*<system:String x:Key="Compress_SuccessWithOpen">.*?</system:String>', ''

    # Add Nav
    $content = $content -replace '(<system:String x:Key="Nav_Sign">.*?</system:String>)', ('$1' + "`r`n    <system:String x:Key=""Nav_Compress"">" + $t.Nav + "</system:String>")
    
    # Add Dash
    $content = $content -replace '(<system:String x:Key="Dash_SignSub">.*?</system:String>)', ('$1' + "`r`n    <system:String x:Key=""Dash_CompressTitle"">" + $t.DashTitle + "</system:String>`r`n    <system:String x:Key=""Dash_CompressSub"">" + $t.DashSub + "</system:String>")

    # Add End
    $newSection = "`r`n`r`n    <!-- Compress Tool -->`r`n" +
                  "    <system:String x:Key=""Compress_Title"">" + $t.CompTitle + "</system:String>`r`n" +
                  "    <system:String x:Key=""Compress_Desc"">" + $t.CompDesc + "</system:String>`r`n" +
                  "    <system:String x:Key=""Compress_SelectFile"">" + $t.CompSelect + "</system:String>`r`n" +
                  "    <system:String x:Key=""Compress_ProcessBtn"">" + $t.CompBtn + "</system:String>`r`n" +
                  "    <system:String x:Key=""Compress_Success"">" + $t.CompSuccess + "</system:String>`r`n" +
                  "    <system:String x:Key=""Compress_SuccessWithOpen"">" + $t.CompSuccessWithOpen + "</system:String>`r`n"
    
    $content = $content -replace '</ResourceDictionary>', ($newSection + "</ResourceDictionary>")

    [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
    Write-Host "Updated $lang"
}
