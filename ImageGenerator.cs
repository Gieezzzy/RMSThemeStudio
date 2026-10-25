using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RMSThemeStudio
{
    public static class ImageGenerator
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        public static async Task<(bool Success, string Message)> GenerateWithAiAsync(string prompt, string outputFilePath)
        {
            try
            {
                string encoded = Uri.EscapeDataString(prompt + ", dark aesthetic, portrait mobile wallpaper 9:16, masterpiece, 8k resolution, cinematic lighting");
                string url = $"https://image.pollinations.ai/prompt/{encoded}?width=1080&height=1920&model=flux&nologo=true";

                byte[] data = await _httpClient.GetByteArrayAsync(url);
                if (data.Length < 1000)
                {
                    return (false, "Response too small, generation may have timed out.");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
                await File.WriteAllBytesAsync(outputFilePath, data);
                return (true, "AI Wallpaper generated and saved successfully!");
            }
            catch (Exception ex)
            {
                return (false, "AI generation failed: " + ex.Message);
            }
        }

        public static void GenerateAtmosphericProcedural(string themeName, Color dominant, Color secondary, string outputFilePath)
        {
            int w = 1080;
            int h = 1920;
            using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Deep gradient background
            using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, h), secondary, Color.FromArgb(10, 14, 26)))
            {
                g.FillRectangle(brush, 0, 0, w, h);
            }

            // 2. Radial glowing orb at center
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(w * 0.15f, h * 0.25f, w * 0.70f, w * 0.70f);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(120, dominant.R, dominant.G, dominant.B);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, dominant.R, dominant.G, dominant.B) };
                    g.FillPath(pgb, path);
                }
            }

            // 3. Subtle grid lines / geometric aura
            using (var pen = new Pen(Color.FromArgb(20, 255, 255, 255), 1.5f))
            {
                for (int y = 0; y < h; y += 80)
                {
                    g.DrawLine(pen, 0, y, w, y);
                }
                for (int x = 0; x < w; x += 80)
                {
                    g.DrawLine(pen, x, 0, x, h);
                }
            }

            // 4. Dark radial vignette around edges
            using (var pathVignette = new GraphicsPath())
            {
                pathVignette.AddRectangle(new Rectangle(0, 0, w, h));
                using (var pgbVig = new PathGradientBrush(pathVignette))
                {
                    pgbVig.CenterColor = Color.FromArgb(0, 0, 0, 0);
                    pgbVig.SurroundColors = new[] { Color.FromArgb(220, 5, 8, 16) };
                    g.FillRectangle(pgbVig, 0, 0, w, h);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
            bmp.Save(outputFilePath, ImageFormat.Png);
        }

        public static void GenerateCallBackground(string sourceChatBgPath, string outputCallBgPath)
        {
            int w = 1080;
            int h = 1920;
            using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (File.Exists(sourceChatBgPath))
            {
                using var src = Image.FromFile(sourceChatBgPath);
                g.DrawImage(src, new Rectangle(0, 0, w, h));
            }
            else
            {
                g.Clear(Color.FromArgb(10, 14, 26));
            }

            // Darken and apply deep acoustic vignette for voice call clarity
            using (var darkBrush = new SolidBrush(Color.FromArgb(140, 5, 8, 16)))
            {
                g.FillRectangle(darkBrush, 0, 0, w, h);
            }

            // Concentric acoustic rings around call avatar position (y = 520)
            using (var ringPen1 = new Pen(Color.FromArgb(40, 56, 189, 248), 3f))
            using (var ringPen2 = new Pen(Color.FromArgb(25, 56, 189, 248), 2f))
            {
                g.DrawEllipse(ringPen1, w / 2f - 180, 520 - 180, 360, 360);
                g.DrawEllipse(ringPen2, w / 2f - 240, 520 - 240, 480, 480);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputCallBgPath)!);
            bmp.Save(outputCallBgPath, ImageFormat.Png);
        }
    }
}
