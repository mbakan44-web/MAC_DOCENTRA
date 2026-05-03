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
        string src = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\image.png";
        string dst = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\PromtAiPdfPro\Assets\app_icon.ico";

        int[] sizes = { 256, 128, 64, 48, 32, 16 };

        using var original = new Bitmap(src);
        var pngs = new byte[sizes.Length][];

        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            using var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawImage(original, 0, 0, s, s);

            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            pngs[i] = ms.ToArray();
        }

        using var fs = new FileStream(dst, FileMode.Create);
        using var w  = new BinaryWriter(fs);

        // ICONDIR
        w.Write((short)0);
        w.Write((short)1);
        w.Write((short)sizes.Length);

        // Calculate offsets: header(6) + entries(16 * count)
        int offset = 6 + 16 * sizes.Length;
        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            w.Write((byte)(s >= 256 ? 0 : s));   // width
            w.Write((byte)(s >= 256 ? 0 : s));   // height
            w.Write((byte)0);  // color count
            w.Write((byte)0);  // reserved
            w.Write((short)1); // planes
            w.Write((short)32);// bpp
            w.Write((int)pngs[i].Length);
            w.Write((int)offset);
            offset += pngs[i].Length;
        }

        foreach (var png in pngs)
            w.Write(png);

        Console.WriteLine($"Multi-size ICO created: {dst}");
    }
}
