import os
import base64
import subprocess

workspace_dir = r"d:\All-in-One PDF Suite"
html_output_path = os.path.join(workspace_dir, "Docentra_Pitch_Deck.html")
pdf_output_path = os.path.join(workspace_dir, "Docentra_Pitch_Deck_Workup.pdf")

def to_base64(filepath):
    if os.path.exists(filepath):
        with open(filepath, "rb") as f:
            encoded = base64.b64encode(f.read()).decode("utf-8")
            ext = os.path.splitext(filepath)[1].lower().replace(".", "")
            if ext == "jpg": ext = "jpeg"
            return f"data:image/{ext};base64,{encoded}"
    return ""

img_logo = to_base64(r"d:\All-in-One PDF Suite\Docentra_v1.0.4\src\Assets\app_icon.png")
img_hero = to_base64(r"d:\All-in-One PDF Suite\Website\hero-mockup.png")
img_dashboard = to_base64(r"d:\All-in-One PDF Suite\Website\assets\screenshots\dashboard_v2.png")
img_ocr = to_base64(r"d:\All-in-One PDF Suite\Website\assets\screenshots\ocr_screen.png")
img_metadata = to_base64(r"d:\All-in-One PDF Suite\Website\assets\screenshots\metadata_cleaner.png")
img_watermark = to_base64(r"d:\All-in-One PDF Suite\Website\assets\screenshots\watermark_screen.png")
img_reader = to_base64(r"d:\All-in-One PDF Suite\Website\assets\screenshots\pdf_reader.png")

html_content = f"""<!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="UTF-8">
<title>Docentra Pitch Deck - Workup 2026</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=Space+Grotesk:wght@600;700&display=swap');

  @page {{
    size: 16in 9in;
    margin: 0;
  }}

  * {{
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }}

  body {{
    font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, sans-serif;
    background-color: #080c14;
    color: #e2e8f0;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }}

  .slide {{
    width: 16in;
    height: 9in;
    page-break-after: always;
    position: relative;
    overflow: hidden;
    background: radial-gradient(circle at 10% 10%, #172554 0%, #0a0f1d 50%, #040711 100%);
    display: flex;
    flex-direction: column;
    padding: 2.8rem 3.5rem;
  }}

  /* Background decorative glow */
  .slide::before {{
    content: '';
    position: absolute;
    top: -20%;
    right: -10%;
    width: 600px;
    height: 600px;
    background: radial-gradient(circle, rgba(59, 130, 246, 0.15) 0%, rgba(99, 102, 241, 0.05) 50%, transparent 70%);
    pointer-events: none;
    z-index: 1;
  }}

  .slide::after {{
    content: '';
    position: absolute;
    bottom: -15%;
    left: -10%;
    width: 500px;
    height: 500px;
    background: radial-gradient(circle, rgba(16, 185, 129, 0.1) 0%, transparent 70%);
    pointer-events: none;
    z-index: 1;
  }}

  .content-layer {{
    position: relative;
    z-index: 2;
    height: 100%;
    display: flex;
    flex-direction: column;
  }}

  /* Header Bar */
  .header {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
  }}

  .brand {{
    display: flex;
    align-items: center;
    gap: 1rem;
  }}

  .brand img {{
    width: 48px;
    height: 48px;
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(59, 130, 246, 0.4);
  }}

  .brand-name {{
    font-family: 'Space Grotesk', sans-serif;
    font-size: 1.8rem;
    font-weight: 700;
    letter-spacing: -0.5px;
    color: #ffffff;
  }}

  .brand-tag {{
    background: rgba(59, 130, 246, 0.15);
    border: 1px solid rgba(59, 130, 246, 0.4);
    color: #60a5fa;
    padding: 0.35rem 0.9rem;
    border-radius: 20px;
    font-size: 0.85rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }}

  .slide-category {{
    color: #94a3b8;
    font-size: 1rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 1px;
  }}

  .slide-title-group {{
    margin-bottom: 2rem;
  }}

  .slide-title {{
    font-family: 'Space Grotesk', sans-serif;
    font-size: 2.8rem;
    font-weight: 700;
    line-height: 1.15;
    color: #ffffff;
    margin-bottom: 0.5rem;
  }}

  .highlight-blue {{
    background: linear-gradient(135deg, #60a5fa 0%, #a855f7 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }}

  .highlight-green {{
    background: linear-gradient(135deg, #34d399 0%, #3b82f6 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }}

  .slide-subtitle {{
    font-size: 1.25rem;
    color: #94a3b8;
    font-weight: 400;
  }}

  /* Grid Layouts */
  .grid-2 {{
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 2.5rem;
    align-items: center;
    flex: 1;
  }}

  .grid-3 {{
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
    flex: 1;
  }}

  .grid-4 {{
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 1.5rem;
    flex: 1;
  }}

  /* Cards */
  .card {{
    background: rgba(17, 24, 39, 0.7);
    backdrop-filter: blur(12px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 20px;
    padding: 2rem;
    display: flex;
    flex-direction: column;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  }}

  .card-highlight {{
    background: linear-gradient(145deg, rgba(30, 58, 138, 0.4) 0%, rgba(17, 24, 39, 0.8) 100%);
    border: 1px solid rgba(96, 165, 250, 0.3);
  }}

  .card-icon {{
    width: 54px;
    height: 54px;
    border-radius: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.8rem;
    margin-bottom: 1.25rem;
  }}

  .icon-blue {{ background: rgba(59, 130, 246, 0.2); color: #60a5fa; border: 1px solid rgba(59, 130, 246, 0.3); }}
  .icon-red {{ background: rgba(239, 68, 68, 0.2); color: #f87171; border: 1px solid rgba(239, 68, 68, 0.3); }}
  .icon-green {{ background: rgba(16, 185, 129, 0.2); color: #34d399; border: 1px solid rgba(16, 185, 129, 0.3); }}
  .icon-purple {{ background: rgba(168, 85, 247, 0.2); color: #c084fc; border: 1px solid rgba(168, 85, 247, 0.3); }}
  .icon-amber {{ background: rgba(245, 158, 11, 0.2); color: #fbbf24; border: 1px solid rgba(245, 158, 11, 0.3); }}

  .card h3 {{
    font-size: 1.4rem;
    font-weight: 700;
    color: #ffffff;
    margin-bottom: 0.75rem;
  }}

  .card p {{
    font-size: 1.05rem;
    line-height: 1.6;
    color: #94a3b8;
  }}

  /* Cover Slide */
  .cover {{
    justify-content: center;
    align-items: center;
    text-align: center;
    padding: 0 5rem;
  }}

  .cover-logo {{
    width: 110px;
    height: 110px;
    margin-bottom: 2rem;
    border-radius: 28px;
    box-shadow: 0 15px 40px rgba(59, 130, 246, 0.5);
  }}

  .cover-badge {{
    display: inline-block;
    padding: 0.5rem 1.4rem;
    border-radius: 30px;
    background: rgba(59, 130, 246, 0.15);
    border: 1px solid rgba(59, 130, 246, 0.4);
    color: #60a5fa;
    font-weight: 700;
    font-size: 1rem;
    letter-spacing: 1px;
    text-transform: uppercase;
    margin-bottom: 1.5rem;
  }}

  .cover-title {{
    font-family: 'Space Grotesk', sans-serif;
    font-size: 4.8rem;
    font-weight: 800;
    letter-spacing: -1.5px;
    line-height: 1.05;
    color: #ffffff;
    margin-bottom: 1.5rem;
  }}

  .cover-desc {{
    font-size: 1.6rem;
    max-width: 950px;
    line-height: 1.5;
    color: #cbd5e1;
    margin-bottom: 3rem;
  }}

  .cover-footer {{
    display: flex;
    gap: 3rem;
    padding-top: 2rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }}

  .meta-item {{
    text-align: left;
  }}

  .meta-label {{
    font-size: 0.85rem;
    color: #64748b;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin-bottom: 0.25rem;
  }}

  .meta-value {{
    font-size: 1.15rem;
    font-weight: 700;
    color: #f1f5f9;
  }}

  /* Images */
  .img-frame {{
    width: 100%;
    border-radius: 18px;
    overflow: hidden;
    border: 1px solid rgba(255, 255, 255, 0.12);
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.6);
  }}

  .img-frame img {{
    width: 100%;
    height: auto;
    display: block;
  }}

  /* Table */
  .comp-table {{
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
    border-radius: 16px;
    overflow: hidden;
    background: rgba(17, 24, 39, 0.7);
    border: 1px solid rgba(255, 255, 255, 0.1);
  }}

  .comp-table th {{
    background: rgba(30, 41, 59, 0.9);
    padding: 1.2rem 1.5rem;
    text-align: left;
    font-size: 1.1rem;
    font-weight: 700;
    color: #ffffff;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }}

  .comp-table th.highlight-col {{
    background: rgba(37, 99, 235, 0.3);
    color: #60a5fa;
    border-left: 2px solid #3b82f6;
    border-right: 2px solid #3b82f6;
  }}

  .comp-table td {{
    padding: 1.1rem 1.5rem;
    font-size: 1.05rem;
    color: #cbd5e1;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  }}

  .comp-table td.highlight-col {{
    background: rgba(37, 99, 235, 0.1);
    font-weight: 700;
    color: #ffffff;
    border-left: 2px solid #3b82f6;
    border-right: 2px solid #3b82f6;
  }}

  .check-yes {{ color: #34d399; font-weight: 700; }}
  .check-no {{ color: #f87171; font-weight: 700; }}

  /* Footer on all slides */
  .slide-footer {{
    margin-top: auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-top: 1rem;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
    font-size: 0.9rem;
    color: #64748b;
  }}

  /* Flags */
  .lang-grid {{
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 1.25rem;
    margin-top: 1rem;
  }}

  .lang-card {{
    background: rgba(30, 41, 59, 0.5);
    border: 1px solid rgba(255, 255, 255, 0.08);
    padding: 1.25rem;
    border-radius: 14px;
    display: flex;
    align-items: center;
    gap: 1rem;
  }}

  .lang-flag {{
    font-size: 2.2rem;
  }}

  .lang-name {{
    font-size: 1.1rem;
    font-weight: 700;
    color: #ffffff;
  }}

  .lang-code {{
    font-size: 0.8rem;
    color: #94a3b8;
  }}

  .stat-number {{
    font-family: 'Space Grotesk', sans-serif;
    font-size: 3.5rem;
    font-weight: 800;
    color: #60a5fa;
    line-height: 1;
    margin-bottom: 0.5rem;
  }}
</style>
</head>
<body>

<!-- ==================== SLIDE 1: KAPAK ==================== -->
<div class="slide cover">
  <div class="content-layer" style="align-items: center;">
    <img src="{img_logo}" class="cover-logo" alt="Docentra Logo">
    <div class="cover-badge">Workup Girişimcilik Programı 2026 Başvuru Sunumu</div>
    <h1 class="cover-title">DOCENTRA</h1>
    <p class="cover-desc">
      Hassas kurumsal verileri buluta yükleme riskini bitiren, <span class="highlight-blue">%100 çevrimdışı</span> çalışan, yerel OCR destekli hepsi bir arada doküman ve PDF yönetim platformu.
    </p>

    <div class="cover-footer">
      <div class="meta-item">
        <div class="meta-label">Girişim Dikeyi</div>
        <div class="meta-value">B2B SaaS / Kurumsal Yazılım</div>
      </div>
      <div class="meta-item">
        <div class="meta-label">Aşama</div>
        <div class="meta-value">Pre-seed (Canlı Ürün v1.0.4)</div>
      </div>
      <div class="meta-item">
        <div class="meta-label">Kurucu</div>
        <div class="meta-value">Mustafa Bakan (Founder & Lead Dev)</div>
      </div>
      <div class="meta-item">
        <div class="meta-label">Web Sitesi</div>
        <div class="meta-value">docentrapdf.com</div>
      </div>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 2: PROBLEM ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Problem Analizi</span>
      </div>
      <span class="slide-category">02 / PROBLEM</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Mevcut PDF ve Doküman Çözümleri <span class="highlight-blue">Yetersiz ve Riskli</span></h2>
      <p class="slide-subtitle">Kurumlar ve profesyoneller günlük evrak işlerini yürütürken 3 devasa engelle karşılaşıyor.</p>
    </div>

    <div class="grid-3">
      <div class="card">
        <div class="card-icon icon-red">⚠️</div>
        <h3>1. Buluta Yükleme & KVKK / Gizlilik Riski</h3>
        <p>Online PDF araçları (Smallpdf, iLovePDF vb.) şirketlerin finansal tablolarını, sözleşmelerini ve kimlik fotokopilerini yabancı sunuculara yükletiyor. Bankacılık, hukuk ve sağlıkta ciddi veri sızıntısı ve KVKK/GDPR ihlali doğuruyor.</p>
      </div>

      <div class="card">
        <div class="card-icon icon-red">💸</div>
        <h3>2. Fahiş Abonelik Fiyatları (SaaS Fatigue)</h3>
        <p>Adobe Acrobat gibi pazar liderleri kullanıcı başına aylık 20-30 USD gibi zorunlu ve tekrarlayan faturalar kesiyor. KOBİ'ler ve ekipler her yıl binlerce doları sadece temel PDF işlemleri için harcamak zorunda kalıyor.</p>
      </div>

      <div class="card">
        <div class="card-icon icon-red">🐢</div>
        <h3>3. Şişkin, Yavaş ve Ağır Yazılımlar (Bloatware)</h3>
        <p>Geleneksel masaüstü alternatifler GB'larca disk kaplıyor, arka planda onlarca telemetri ve güncelleme servisi çalıştırarak kurumsal bilgisayarları kilitliyor ve anında işlem yapmayı engelliyor.</p>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 3: ÇÖZÜM ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Çözüm Önerimiz</span>
      </div>
      <span class="slide-category">03 / ÇÖZÜM</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Sıfır Bulut Bağımlılığı, <span class="highlight-green">Maksimum Performans & Gizlilik</span></h2>
      <p class="slide-subtitle">Docentra, doküman yönetimini tamamen kullanıcının kendi cihazı üzerine taşır.</p>
    </div>

    <div class="grid-2">
      <div style="display: flex; flex-direction: column; gap: 1.5rem;">
        <div class="card card-highlight">
          <div class="card-icon icon-green">🛡️</div>
          <h3>%100 Yerel ve Çevrimdışı (Zero-Cloud Architecture)</h3>
          <p>Tüm dönüştürme, OCR, şifreleme ve birleştirme işlemleri internet bağlantısı gerekmeden doğrudan donanım üzerinde gerçekleşir. Hiçbir bayt harici sunucuya gitmez.</p>
        </div>

        <div class="card">
          <div class="card-icon icon-blue">⚡</div>
          <h3>60 MB Süper Hafif & Anında Kurulum</h3>
          <p>Şişkinlik yok, arka plan takipçisi yok. Saniyeler içinde kurulur, anında açılır ve kurumsal bilgisayarları hiç yormaz.</p>
        </div>

        <div class="card">
          <div class="card-icon icon-purple">💎</div>
          <h3>Şeffaf & Erişilebilir Lisanslama</h3>
          <p>Aylık abonelik tuzağı yerine ömür boyu sahip olma (Lifetime) ve şirketler için uygun maliyetli kurumsal lisanslama modeli.</p>
        </div>
      </div>

      <div>
        <div class="img-frame">
          <img src="{img_hero}" alt="Docentra Hero Mockup">
        </div>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 4: ÜRÜN & ÖZELLİKLER ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Çalışan Ürün (v1.0.4)</span>
      </div>
      <span class="slide-category">04 / ÜRÜN KAPSAMI</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Bir Profesyonelin İhtiyaç Duyduğu <span class="highlight-blue">14+ Güçlü Araç</span></h2>
      <p class="slide-subtitle">Tek bir hafif uygulama içerisinde entegre profesyonel doküman araç seti.</p>
    </div>

    <div class="grid-2">
      <div>
        <div class="img-frame">
          <img src="{img_dashboard}" alt="Docentra Dashboard">
        </div>
      </div>

      <div style="display: flex; flex-direction: column; gap: 1rem;">
        <div class="card" style="padding: 1.25rem 1.75rem;">
          <h3 style="font-size: 1.15rem; margin-bottom: 0.25rem;">📑 Çoklu Sekmeli PDF Görüntüleyici</h3>
          <p style="font-size: 0.95rem;">Aynı anda birden fazla PDF belgesini sekmeler halinde hızlıca inceleme ve yönetme.</p>
        </div>
        <div class="card" style="padding: 1.25rem 1.75rem;">
          <h3 style="font-size: 1.15rem; margin-bottom: 0.25rem;">🔀 PDF Birleştirme & Ayırma (Merge & Split)</h3>
          <p style="font-size: 0.95rem;">Yüzlerce sayfayı tek tıkla sıralama, istenilen aralıklara bölme ve sayfa çıkarma.</p>
        </div>
        <div class="card" style="padding: 1.25rem 1.75rem;">
          <h3 style="font-size: 1.15rem; margin-bottom: 0.25rem;">📄 Office & Görsel Dönüştürücü</h3>
          <p style="font-size: 0.95rem;">Word, Excel ve görselleri yüksek çözünürlükte yerel PDF formatına dönüştürme.</p>
        </div>
        <div class="card" style="padding: 1.25rem 1.75rem;">
          <h3 style="font-size: 1.15rem; margin-bottom: 0.25rem;">✂️ Sayfa Numaralandırma, Kırpma & Düzenleme</h3>
          <p style="font-size: 0.95rem;">Özel pozisyonlarda sayfa numarası ekleme, kenar boşluklarını kırpma ve rotate etme.</p>
        </div>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 5: OCR & GÜVENLİK ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Teknoloji & Güvenlik</span>
      </div>
      <span class="slide-category">05 / DERİN TEKNOLOJİ</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Yerel OCR ve <span class="highlight-green">Askeri Düzey Güvenlik</span></h2>
      <p class="slide-subtitle">Taranmış kağıt evrakları cihaz üzerinde aranabilir metne çevirir, gizli dijital izleri temizler.</p>
    </div>

    <div class="grid-3">
      <div class="card">
        <div class="img-frame" style="margin-bottom: 1.25rem;">
          <img src="{img_ocr}" alt="Docentra OCR">
        </div>
        <h3>Yerel (Local) OCR Motoru</h3>
        <p>Taranmış PDF ve fotoğraflardaki yazıları buluta ihtiyaç duymadan cihaz üzerinde yüksek doğrulukla tanır. Metin kopyalama ve aranabilir PDF üretimi sağlar.</p>
      </div>

      <div class="card">
        <div class="img-frame" style="margin-bottom: 1.25rem;">
          <img src="{img_metadata}" alt="Metadata Cleaner">
        </div>
        <h3>Dijital İz Temizleyici (Metadata)</h3>
        <p>PDF'lerin arka planında gizlenen yazar ismi, revizyon tarihi, kurum bilgisi ve GPS koordinatlarını tek tıkla sıfırlayarak dışarıya bilgi sızmasını önler.</p>
      </div>

      <div class="card">
        <div class="img-frame" style="margin-bottom: 1.25rem;">
          <img src="{img_watermark}" alt="Watermark Screen">
        </div>
        <h3>Filigran, Şifreleme & İmza</h3>
        <p>AES-256 askeri şifreleme ile parola koruması, yetkisiz kopyalamayı engelleyen özel metin/logo filigranı ve yerel dijital imza araçları.</p>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 6: ÇOKLU DİL DESTEĞİ ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Global Pazara Hazır</span>
      </div>
      <span class="slide-category">06 / ÇOKLU DİL DESTEĞİ</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">İlk Günden Global: <span class="highlight-blue">10 Farklı Dil Desteği</span></h2>
      <p class="slide-subtitle">Docentra, sadece Türkiye pazarı için değil; global SaaS standartlarında 10 dünya dilini tam destekler.</p>
    </div>

    <div style="flex: 1; display: flex; flex-direction: column; justify-content: center;">
      <div class="lang-grid">
        <div class="lang-card">
          <div class="lang-flag">🇹🇷</div>
          <div>
            <div class="lang-name">Türkçe</div>
            <div class="lang-code">Yerel Tam Destek (tr-TR)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇬🇧</div>
          <div>
            <div class="lang-name">English</div>
            <div class="lang-code">Global Default (en-US)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇩🇪</div>
          <div>
            <div class="lang-name">Deutsch</div>
            <div class="lang-code">Avrupa Pazarı (de-DE)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇪🇸</div>
          <div>
            <div class="lang-name">Español</div>
            <div class="lang-code">Latin & İspanya (es-ES)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇫🇷</div>
          <div>
            <div class="lang-name">Français</div>
            <div class="lang-code">Fransa & Frankofon (fr-FR)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇮🇹</div>
          <div>
            <div class="lang-name">Italiano</div>
            <div class="lang-code">İtalya Pazarı (it-IT)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇯🇵</div>
          <div>
            <div class="lang-name">日本語</div>
            <div class="lang-code">Japonya Pazarı (ja-JP)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇷🇺</div>
          <div>
            <div class="lang-name">Русский</div>
            <div class="lang-code">BDT & Rusça (ru-RU)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇨🇳</div>
          <div>
            <div class="lang-name">简体中文</div>
            <div class="lang-code">Çin & Asya (zh-CN)</div>
          </div>
        </div>
        <div class="lang-card">
          <div class="lang-flag">🇸🇦</div>
          <div>
            <div class="lang-name">العربية</div>
            <div class="lang-code">Orta Doğu & RTL (ar-SA)</div>
          </div>
        </div>
      </div>

      <div class="card" style="margin-top: 2rem; background: rgba(30, 58, 138, 0.25); border: 1px solid rgba(59, 130, 246, 0.4); flex-direction: row; align-items: center; justify-content: space-between;">
        <div>
          <h3 style="font-size: 1.25rem; margin-bottom: 0.25rem;">🌐 Dünya Nüfusunun %70+'sinin Ana Diline Tam Erişim</h3>
          <p style="font-size: 1rem; color: #cbd5e1;">Arapça gibi sağdan sola yazılan (RTL) diller dahil tüm arayüz dinamik WPF yerelleştirme mimarisiyle sıfırdan tasarlandı.</p>
        </div>
        <div style="font-size: 2.5rem;">🌍</div>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 7: İŞ MODELİ ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Gelir Modeli</span>
      </div>
      <span class="slide-category">07 / İŞ MODELİ</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Fiyatlandırma & Gelir Modeli: <span class="highlight-green">$19.90 / PC</span></h2>
      <p class="slide-subtitle">Abonelik tuzağı yok. Tekil alımlarda net $19.90, şirketler ve çoklu alımlarda ise %30'a varan indirim avantajı.</p>
    </div>

    <div class="grid-3">
      <div class="card card-highlight">
        <div class="card-icon icon-green">💻</div>
        <h3>Standart Lisans</h3>
        <div class="stat-number" style="color: #34d399;">$19.90</div>
        <p style="font-weight: 600; color: #cbd5e1; margin-bottom: 0.5rem;">Tek Seferlik / PC Başına Ömür Boyu</p>
        <p>Bireysel kullanıcılar ve tekil bilgisayarlar için bir defalık ödeme. Aylık/yıllık yenileme veya gizli ücret yok; tüm güncellemeler ömür boyu dahil.</p>
      </div>

      <div class="card">
        <div class="card-icon icon-blue">🏢</div>
        <h3>Toplu Alım Avantajı</h3>
        <div class="stat-number" style="color: #60a5fa;">%30'a Varan</div>
        <p style="font-weight: 600; color: #cbd5e1; margin-bottom: 0.5rem;">Şirketler & Ekipler İçin Hacim İndirimi</p>
        <p>Hukuk büroları, muhasebe ofisleri ve kurumsal ekipler için çoklu PC lisansı alımlarında %30'a varan kademeli indirim. Tek anahtarla tüm ofiste anında aktivasyon.</p>
      </div>

      <div class="card">
        <div class="card-icon icon-purple">📈</div>
        <h3>Sıfır Sunucu Maliyeti</h3>
        <div class="stat-number" style="color: #c084fc;">%90+</div>
        <p style="font-weight: 600; color: #cbd5e1; margin-bottom: 0.5rem;">Yüksek Brüt Kâr Marjı</p>
        <p>Tüm işlemler kullanıcının kendi bilgisayarında yerel çalıştığı için sunucu/veri tabanı masrafımız sıfırdır. İndirimli toplu satışlarda bile doğrudan net nakit akışı sağlar.</p>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 8: REKABET ANALİZİ ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Pazar Konumu</span>
      </div>
      <span class="slide-category">08 / REKABET</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Dev Rakiplere Karşı <span class="highlight-blue">Ezber Bozan Üstünlük</span></h2>
      <p class="slide-subtitle">Gizlilik, performans ve maliyet ekseninde pazarın en dengeli çözümü.</p>
    </div>

    <table class="comp-table">
      <thead>
        <tr>
          <th>Özellik & Kriter</th>
          <th class="highlight-col">DOCENTRA</th>
          <th>Adobe Acrobat Pro</th>
          <th>Smallpdf / iLovePDF</th>
          <th>PDF24 Creator</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td><strong>İşlem Mimarisi</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ %100 Çevrimdışı (Yerel)</span></td>
          <td>Karma (Bulut Zorunlu)</td>
          <td><span class="check-no">✗ Bulut Sunucusu Şart</span></td>
          <td>Yerel</td>
        </tr>
        <tr>
          <td><strong>Veri Gizliliği (KVKK/GDPR)</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ Sıfır Sızıntı Garantisi</span></td>
          <td>Bulutta İşlenir</td>
          <td><span class="check-no">✗ Yüksek Sızıntı Riski</span></td>
          <td>Yerel</td>
        </tr>
        <tr>
          <td><strong>Yerel (Cihaz İçi) OCR</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ Dahili ve Hızlı</span></td>
          <td>Var (Ağır)</td>
          <td>Sadece Bulutta</td>
          <td>Temel Seviye</td>
        </tr>
        <tr>
          <td><strong>Kurulum Boyutu</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ &lt; 60 MB</span></td>
          <td><span class="check-no">✗ 1.5 GB+ (Bloatware)</span></td>
          <td>Web Tarayıcı</td>
          <td>350 MB+</td>
        </tr>
        <tr>
          <td><strong>Maliyet Modeli</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ $19.90 Tek Seferlik</span></td>
          <td><span class="check-no">✗ $240 / Yıl (Abonelik)</span></td>
          <td><span class="check-no">✗ $108 / Yıl (Abonelik)</span></td>
          <td>Ücretsiz (Monetizasyon yok)</td>
        </tr>
        <tr>
          <td><strong>Çoklu Dil Desteği</strong></td>
          <td class="highlight-col"><span class="check-yes">✓ 10 Dil (Arapça RTL dahil)</span></td>
          <td>Geniş</td>
          <td>Temel</td>
          <td>Almanca / İngilizce odaklı</td>
        </tr>
      </tbody>
    </table>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 9: TRACTION & ROADMAP ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Gelişmeler & Hedefler</span>
      </div>
      <span class="slide-category">09 / TRACTION & YOL HARİTASI</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Mevcut Çekiş Gücü & <span class="highlight-green">Büyüme Planı</span></h2>
      <p class="slide-subtitle">Çalışan ürünümüz hazır, global kanallarda lansman yapıldı.</p>
    </div>

    <div class="grid-2">
      <div class="card">
        <h3 style="color: #60a5fa; margin-bottom: 1.25rem;">🚀 Mevcut Durum (Traction)</h3>
        <ul style="list-style: none; display: flex; flex-direction: column; gap: 1rem; color: #cbd5e1; font-size: 1.05rem;">
          <li>✅ <strong>Canlı ve Çalışan Ürün:</strong> Docentra v1.0.4 tüm 14 modülüyle hazır ve lisanslama altyapısı aktif.</li>
          <li>✅ <strong>Product Hunt Lansmanı:</strong> Küresel lansman gerçekleştirildi, global topluluk geri bildirimi alındı.</li>
          <li>✅ <strong>AlternativeTo Listelemesi:</strong> Adobe Acrobat alternatifi olarak organik aramalardan trafik çekiliyor.</li>
          <li>✅ <strong>Global Web Sitesi:</strong> docentrapdf.com yayında, 10 dilli SEO sayfaları ve blog altyapısı kuruldu.</li>
        </ul>
      </div>

      <div class="card card-highlight">
        <h3 style="color: #34d399; margin-bottom: 1.25rem;">🎯 Workup Dönemi Hedefleri (Roadmap)</h3>
        <ul style="list-style: none; display: flex; flex-direction: column; gap: 1rem; color: #cbd5e1; font-size: 1.05rem;">
          <li>📍 <strong>1. Ay:</strong> İş Bankası ve iştiraklerinde 50-100 kişilik B2B pilot kullanım başlatma (POC).</li>
          <li>📍 <strong>3. Ay:</strong> Kurumsal toplu lisanslama ve merkezi dağıtım (MSI Installer / Active Directory) desteği.</li>
          <li>📍 <strong>5. Ay:</strong> Yerel AI Modülü (Cihaz içi doküman özetleme & chat).</li>
          <li>📍 <strong>6. Ay:</strong> Workup Demo Day ile tohum yatırım turunu kapatma ve global pazarda 10.000 aktif lisansa ulaşma.</li>
        </ul>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı</span>
    </div>
  </div>
</div>

<!-- ==================== SLIDE 10: KURUCU & WORKUP VİZYONU ==================== -->
<div class="slide">
  <div class="content-layer">
    <div class="header">
      <div class="brand">
        <img src="{img_logo}">
        <span class="brand-name">DOCENTRA</span>
        <span class="brand-tag">Ekip & Vizyon</span>
      </div>
      <span class="slide-category">10 / KURUCU & İLETİŞİM</span>
    </div>

    <div class="slide-title-group">
      <h2 class="slide-title">Neden Workup? <span class="highlight-blue">Birlikte Büyüyelim</span></h2>
      <p class="slide-subtitle">Kurumsal gücü ve vizyonu birleştirerek global bir PDF markası inşa ediyoruz.</p>
    </div>

    <div class="grid-2">
      <div class="card" style="justify-content: center;">
        <div style="display: flex; align-items: center; gap: 1.5rem; margin-bottom: 1.5rem;">
          <div style="width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, #3b82f6, #8b5cf6); display: flex; align-items: center; justify-content: center; font-size: 2.2rem; font-weight: 700; color: white;">
            MB
          </div>
          <div>
            <h3 style="font-size: 1.6rem; margin-bottom: 0.25rem;">Mustafa Bakan</h3>
            <p style="color: #60a5fa; font-weight: 600; font-size: 1.1rem;">Kurucu (Founder & Lead Developer)</p>
            <p style="color: #94a3b8; font-size: 0.95rem;">Kahramanmaraş, Türkiye</p>
          </div>
        </div>
        <p style="font-size: 1.05rem; line-height: 1.6;">
          Docentra'nın tüm yazılım mimarisini, yerel OCR entegrasyonunu ve global pazarlama kanallarını uçtan uca tek başına geliştiren çevik ve sonuç odaklı bir girişimci. Workup'ın mentorluk ve B2B kurumsal gücüyle girişimi hızlandırmayı hedefliyor.
        </p>
      </div>

      <div class="card card-highlight" style="justify-content: center;">
        <h3 style="color: #ffffff; font-size: 1.5rem; margin-bottom: 1rem;">🤝 Workup'tan Ne Bekliyoruz?</h3>
        <p style="font-size: 1.05rem; line-height: 1.6; margin-bottom: 1.5rem;">
          • İş Bankası ve grup şirketleriyle ilk büyük kurumsal <strong>B2B Pilot ve POC anlaşması</strong>.<br>
          • Kurumsal satış ve B2B fiyatlandırma stratejilerinde alanında uzman mentor desteği.<br>
          • Demo Day'de Türkiye'nin lider melek yatırımcıları ve fonlarıyla tohum yatırım turu.
        </p>

        <div style="padding-top: 1rem; border-top: 1px solid rgba(255,255,255,0.1); display: flex; flex-direction: column; gap: 0.5rem; font-size: 1rem;">
          <div>📧 <strong>E-Posta:</strong> support@docentrapdf.com</div>
          <div>🌐 <strong>Web:</strong> https://docentrapdf.com</div>
          <div>🚀 <strong>Product Hunt:</strong> producthunt.com/products/docentra-pdf-viewer-converter</div>
        </div>
      </div>
    </div>

    <div class="slide-footer">
      <span>Docentra Pitch Deck</span>
      <span>Workup Girişimcilik Programı 2026</span>
    </div>
  </div>
</div>

</body>
</html>
"""

with open(html_output_path, "w", encoding="utf-8") as f:
    f.write(html_content)

print(f"HTML presentation written to: {html_output_path}")

# Run headless Edge or Chrome to print to PDF
chrome_path = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
browser_path = chrome_path if os.path.exists(chrome_path) else edge_path

cmd = [
    browser_path,
    "--headless",
    "--disable-gpu",
    "--no-pdf-header-footer",
    f"--print-to-pdf={pdf_output_path}",
    html_output_path
]

print("Rendering PDF with:", browser_path)
res = subprocess.run(cmd, capture_output=True, text=True)
print("Return code:", res.returncode)
if os.path.exists(pdf_output_path):
    print("SUCCESS! PDF size:", os.path.getsize(pdf_output_path), "bytes")
else:
    print("PDF generation failed:", res.stderr)
