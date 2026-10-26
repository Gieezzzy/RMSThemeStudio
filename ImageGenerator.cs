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

        public static Image LoadImageNoLock(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

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
                using (var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
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
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                File.WriteAllBytes(outputFilePath, ms.ToArray());
            }
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
                using var src = LoadImageNoLock(sourceChatBgPath);
                if (src != null)
                {
                    g.DrawImage(src, new Rectangle(0, 0, w, h));
                }
                else
                {
                    g.Clear(Color.FromArgb(10, 14, 26));
                }
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
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                File.WriteAllBytes(outputCallBgPath, ms.ToArray());
            }
        }

        /// <summary>
        /// Fully stretches and resamples an image to exact 1080x1920 mobile screen dimensions with zero letterboxing.
        /// </summary>
        public static void SaveImageStretchedToMobile(byte[] rawBytes, string outputPath, int targetWidth = 1080, int targetHeight = 1920)
        {
            if (rawBytes == null || rawBytes.Length == 0) return;
            using var ms = new MemoryStream(rawBytes);
            using var src = Image.FromStream(ms);
            SaveImageStretchedToMobile(src, outputPath, targetWidth, targetHeight);
        }

        /// <summary>
        /// Fully stretches and resamples an image file to exact 1080x1920 mobile screen dimensions with zero letterboxing.
        /// </summary>
        public static void SaveImageStretchedToMobile(string inputPath, string outputPath, int targetWidth = 1080, int targetHeight = 1920)
        {
            if (!File.Exists(inputPath)) return;
            using var src = LoadImageNoLock(inputPath);
            if (src == null) return;
            SaveImageStretchedToMobile(src, outputPath, targetWidth, targetHeight);
        }

        /// <summary>
        /// Fully stretches and resamples an Image object to exact target mobile dimensions with high-quality bicubic interpolation.
        /// </summary>
        public static void SaveImageStretchedToMobile(Image src, string outputPath, int targetWidth = 1080, int targetHeight = 1920)
        {
            if (src == null) return;
            using var bmp = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Fully stretch the image edge-to-edge across the entire screen area
            g.DrawImage(src, new Rectangle(0, 0, targetWidth, targetHeight));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var outMs = new MemoryStream();
            bmp.Save(outMs, ImageFormat.Png);
            File.WriteAllBytes(outputPath, outMs.ToArray());
        }
    }
}
