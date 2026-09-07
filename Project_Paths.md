# Proje Dosya Yolları ve Gerekli Araçlar

Bu belge, **DOCENTRA PDF & DOCUMENT EDITOR** projesinin geliştirilmesi, derlenmesi ve yayınlanması için gerekli olan dosya yollarını ve araçları içerir.

## 1. Temel Dosya Yolları

*   **Proje Ana Dizini:** `c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite`
*   **Windows Kaynak Kodları (WPF):** `.\PromtAiPdfPro`
*   **Mac Kaynak Kodları (Avalonia):** `.\Docentra_Mac`
*   **Web Sitesi Dosyaları:** `.\Website`
*   **Versiyon Takip Dosyası (Web):** `.\Website\version.txt`

## 2. Derleme ve Paketleme Araçları

*   **.NET SDK (Taşınabilir):** `.\dotnet_sdk\dotnet.exe`
*   **Inno Setup Derleyicisi (Setup Oluşturucu):** `C:\Users\mustafa.bakan\AppData\Local\Programs\Inno Setup 6\iscc.exe`
*   **Kurulum Senaryosu (.iss):** `.\Docentra_Installer.iss`
*   **Otomatik Derleme Scripti:** `.\Build_Docentra.bat`

## 3. Yardımcı Uygulamalar ve Gereksinimler

Projenin tam yönetimi için aşağıdaki araçların da kurulu/erişilebilir olması önerilir:

*   **Visual Studio 2022:** C# ve XAML geliştirme için temel IDE.
*   **Git:** Versiyon kontrolü ve kod yedekleme için (Proje klasöründe `.git` mevcut).
*   **Obfuscar:** Kodun tersine mühendisliğe karşı korunması için (Proje içinde yapılandırılmış durumda).
*   **FileZilla veya WinSCP:** Güncellenen `version.txt` ve yeni `.exe` dosyalarını sunucuya (docentrapdf.com) yüklemek için.
*   **Image Editor (Photoshop/GIMP):** İkon (`.ico`) ve uygulama görsellerini düzenlemek için.

---
*Not: Bu dosya otomatik olarak oluşturulmuştur ve proje gereksinimleri değiştikçe güncellenmelidir.*
