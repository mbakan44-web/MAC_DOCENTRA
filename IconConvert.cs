using System;
using System.IO;

public class IconConverter
{
    public static void Main(string[] args)
    {
        try 
        {
            // Yeni Üretilen Premium PNG Yolu
            string inputPath = @"C:\Users\mustafa.bakan\.gemini\antigravity\brain\3b7ac4ec-2a94-4acb-98fd-8ea1bc516258\docentra_premium_icon_v3_1777677426359.png";
            string outputPath = @"c:\Users\mustafa.bakan\Desktop\APP\All-in-One PDF Suite\PromtAiPdfPro\Assets\app_icon.ico";

            if (!File.Exists(inputPath)) {
                Console.WriteLine("Source PNG not found!");
                return;
            }

            byte[] pngBytes = File.ReadAllBytes(inputPath);

            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // ICONDIR Header (6 bytes)
                    writer.Write((short)0);      // Reserved
                    writer.Write((short)1);      // Type (1 = Icon)
                    writer.Write((short)1);      // Number of images

                    // ICONDIRENTRY (16 bytes)
                    writer.Write((byte)0);       // Width (0 = 256)
                    writer.Write((byte)0);       // Height (0 = 256)
                    writer.Write((byte)0);       // Color count
                    writer.Write((byte)0);       // Reserved
                    writer.Write((short)1);      // Color planes
                    writer.Write((short)32);     // Bits per pixel
                    writer.Write((int)pngBytes.Length); // Image size
                    writer.Write((int)22);       // Image offset (6 + 16)

                    // Image Data (Direct PNG)
                    writer.Write(pngBytes);
                }
            }
            Console.WriteLine("Premium 256x256 ICO created successfully without external dependencies!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
