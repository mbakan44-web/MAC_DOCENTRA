using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// Gerçek çok-boyutlu ICO dosyası oluşturucu
// image.png -> 256, 128, 64, 48, 32, 16 px katmanlı ICO
class Program
{
    static void Main()
    {
        string src = @"C:\Users\mustafa.bakan\.gemini\antigravity\brain\1537746e-9d02-4d65-9266-041d60e06475\premium_docentra_pdf_squircle_icon_1778502319746.png";
        string dst = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\DOCENTRA PDF EDITOR\PromtAiPdfPro\Assets\pdf_file.ico";
        string dstPng = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\DOCENTRA PDF EDITOR\PromtAiPdfPro\Assets\pdf_file.png";

        if (!File.Exists(src)) { Console.WriteLine("Source not found!"); return; }

        using var original = new Bitmap(src);
        
        // 1. Remove Checkerboard Background
        using var cleaned = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                Color c = original.GetPixel(x, y);
                // Checkerboard colors are typically pure white or very light grey
                bool isChecker = (c.R > 190 && c.G > 190 && c.B > 190 && Math.Abs(c.R - c.G) < 10 && Math.Abs(c.G - c.B) < 10);
                
                if (isChecker)
                    cleaned.SetPixel(x, y, Color.Transparent);
                else
                    cleaned.SetPixel(x, y, c);
            }
        }

        // 2. Crop to Content (Bounding Box) - Ignore faint noise pixels
        int minX = cleaned.Width, minY = cleaned.Height, maxX = 0, maxY = 0;
        bool found = false;
        for (int y = 0; y < cleaned.Height; y++)
        {
            for (int x = 0; x < cleaned.Width; x++)
            {
                // Use a threshold (Alpha > 50) to ignore semi-transparent specks
                if (cleaned.GetPixel(x, y).A > 50)
                {
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                    found = true;
                }
            }
        }

        if (!found) { Console.WriteLine("No content found after cleaning!"); return; }
        
        // Add a tiny 1px margin just for anti-aliasing
        minX = Math.Max(0, minX - 1); minY = Math.Max(0, minY - 1);
        maxX = Math.Min(cleaned.Width - 1, maxX + 1); maxY = Math.Min(cleaned.Height - 1, maxY + 1);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        using var cropped = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cropped))
        {
            g.DrawImage(cleaned, new Rectangle(0, 0, width, height), new Rectangle(minX, minY, width, height), GraphicsUnit.Pixel);
        }

        // 3. Save as clean PNG for reference
        cropped.Save(dstPng, ImageFormat.Png);

        // 4. Create Multi-Size ICO
        int[] sizes = { 256, 128, 64, 48, 32, 16 };
        var pngs = new byte[sizes.Length][];

        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            using var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Calculate best fit (aspect ratio preserved but filling)
            float ratio = Math.Min((float)s / cropped.Width, (float)s / cropped.Height);
            int drawW = (int)(cropped.Width * ratio);
            int drawH = (int)(cropped.Height * ratio);
            int posX = (s - drawW) / 2;
            int posY = (s - drawH) / 2;

            g.DrawImage(cropped, posX, posY, drawW, drawH);

            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            pngs[i] = ms.ToArray();
        }

        using var fs = new FileStream(dst, FileMode.Create);
        using var w  = new BinaryWriter(fs);
        w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
        int offset = 6 + 16 * sizes.Length;
        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            w.Write((byte)(s >= 256 ? 0 : s)); w.Write((byte)(s >= 256 ? 0 : s));
            w.Write((byte)0); w.Write((byte)0); w.Write((short)1); w.Write((short)32);
            w.Write((int)pngs[i].Length); w.Write((int)offset);
            offset += pngs[i].Length;
        }
        foreach (var png in pngs) w.Write(png);

        Console.WriteLine($"Icon processed and saved (MAXIMIZED): {dst}");
    }
}
