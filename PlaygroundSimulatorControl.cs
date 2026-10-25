using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace RMSThemeStudio
{
    public class PlaygroundSimulatorControl : Control
    {
        private Image _wallpaper = null;
        private Color _senderColor = Color.FromArgb(2, 132, 199);
        private Color _receiverColor = Color.FromArgb(30, 41, 59);
        private Color _fontColor = Color.White;
        private string _quickEmoji = "✨";
        private string _setIconGroup = "✨,⭐,🌟,💫,🔥";
        private string _animationStyle = "ParticleSnow";

        // Particle Snow Engine
        private class SimulatorParticle
        {
            public string Emoji = "❄️";
            public float X;
            public float Y;
            public float SpeedY;
            public float SwayAmp;
            public float SwaySpeed;
            public float SwayPhase;
            public float Size;
            public int Alpha;
        }

        private readonly List<SimulatorParticle> _particles = new();
        private readonly System.Windows.Forms.Timer _particleTimer = new();
        private readonly Random _rand = new();

        // Cinematic Animation Engine
        private readonly System.Windows.Forms.Timer _animTimer = new();
        private bool _isAnimRunning = false;
        private int _animFrame = 0;
        private int _animTotalFrames = 60; // 2 seconds at 30 fps
        private float _bubbleAlpha = 1.0f;
        private Point _shakeOffset = Point.Empty;
        private string _soundFilePath = "";

        public PlaygroundSimulatorControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;

            _particleTimer.Interval = 33; // ~30 FPS
            _particleTimer.Tick += (s, e) => UpdateParticles();

            _animTimer.Interval = 33;
            _animTimer.Tick += (s, e) => UpdateCinematicAnimation();
        }

        public void SetTheme(ThemeModel model, string wallpaperPath, string soundPath)
        {
            try
            {
                _wallpaper?.Dispose();
                _wallpaper = File.Exists(wallpaperPath) ? Image.FromFile(wallpaperPath) : null;
            }
            catch
            {
                _wallpaper = null;
            }

            try { _senderColor = ColorTranslator.FromHtml(model.SenderBubble); } catch { _senderColor = Color.FromArgb(2, 132, 199); }
            try { _receiverColor = ColorTranslator.FromHtml(model.ReceiverBubble); } catch { _receiverColor = Color.FromArgb(30, 41, 59); }
            try { _fontColor = ColorTranslator.FromHtml(model.TextFontColor); } catch { _fontColor = Color.White; }

            _quickEmoji = string.IsNullOrWhiteSpace(model.DefaultIcon) ? "✨" : model.DefaultIcon.Trim();
            _setIconGroup = string.IsNullOrWhiteSpace(model.SetIconGroup) ? _quickEmoji : model.SetIconGroup.Trim();
            _animationStyle = model.AnimationStyle;
            _soundFilePath = soundPath;

            Invalidate();
        }

        public void TriggerSnowfall()
        {
            _particles.Clear();
            var pool = new List<string>();
            foreach (var item in _setIconGroup.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                pool.Add(item.Trim());
            }
            if (pool.Count == 0) pool.Add(_quickEmoji);

            Rectangle scr = GetScreenRect();
            for (int i = 0; i < 35; i++)
            {
                _particles.Add(new SimulatorParticle
                {
                    Emoji = pool[_rand.Next(pool.Count)],
                    X = _rand.Next(scr.Left + 10, scr.Right - 10),
                    Y = scr.Top - _rand.Next(20, 300),
                    SpeedY = 3.0f + (float)_rand.NextDouble() * 4.5f,
                    SwayAmp = 1.5f + (float)_rand.NextDouble() * 3.0f,
                    SwaySpeed = 0.04f + (float)_rand.NextDouble() * 0.05f,
                    SwayPhase = (float)(_rand.NextDouble() * Math.PI * 2),
                    Size = 13f + (float)_rand.NextDouble() * 9f,
                    Alpha = 220 + _rand.Next(35)
                });
            }

            _particleTimer.Start();
            Invalidate();
        }

        private void UpdateParticles()
        {
            Rectangle scr = GetScreenRect();
            bool anyAlive = false;

            foreach (var p in _particles)
            {
                p.Y += p.SpeedY;
                p.SwayPhase += p.SwaySpeed;
                if (p.Y < scr.Bottom - 45)
                {
                    anyAlive = true;
                }
            }

            if (!anyAlive)
            {
                _particleTimer.Stop();
                _particles.Clear();
            }
            Invalidate();
        }

        public void TriggerCinematicAnimation()
        {
            if (_isAnimRunning) return;
            _isAnimRunning = true;
            _animFrame = 0;
            _animTotalFrames = 65; // ~2.1 seconds
            _animTimer.Start();
        }

        private void UpdateCinematicAnimation()
        {
            _animFrame++;
            float progress = (float)_animFrame / _animTotalFrames;

            // Phase 1: Chats dissolve (0% to 15%)
            if (progress <= 0.15f)
            {
                _bubbleAlpha = Math.Max(0f, 1.0f - (progress / 0.15f));
                _shakeOffset = Point.Empty;
            }
            // Phase 2: Screen shake & glitch (15% to 50%)
            else if (progress <= 0.50f)
            {
                _bubbleAlpha = 0f;
                int jitter = (int)(6 * (1.0f - progress));
                _shakeOffset = new Point(_rand.Next(-jitter, jitter + 1), _rand.Next(-jitter, jitter + 1));
            }
            // Phase 3: Climax sound trigger (at 60% mark)
            else if (progress <= 0.65f)
            {
                _shakeOffset = new Point(_rand.Next(-2, 3), _rand.Next(-2, 3));
                if (_animFrame == (int)(_animTotalFrames * 0.60f) && File.Exists(_soundFilePath))
                {
                    AudioHelper.PlayWav(_soundFilePath);
                }
            }
            // Phase 4: Resolution & return (65% to 100%)
            else
            {
                _shakeOffset = Point.Empty;
                float returnProgress = (progress - 0.65f) / 0.35f;
                _bubbleAlpha = Math.Clamp(returnProgress, 0f, 1f);
            }

            if (_animFrame >= _animTotalFrames)
            {
                _animTimer.Stop();
                _isAnimRunning = false;
                _bubbleAlpha = 1.0f;
                _shakeOffset = Point.Empty;
            }
            Invalidate();
        }

        private Rectangle GetScreenRect()
        {
            int phoneW = Math.Min(Width - 40, (int)((Height - 50) * (9.0 / 16.0)));
            int phoneH = (int)(phoneW * (16.0 / 9.0));
            int x = (Width - phoneW) / 2 + _shakeOffset.X;
            int y = (Height - phoneH) / 2 + _shakeOffset.Y;
            return new Rectangle(x, y, phoneW, phoneH);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Background canvas
            using (var bgBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            Rectangle scr = GetScreenRect();
            int bezel = 12;
            Rectangle phoneBezel = new Rectangle(scr.Left - bezel, scr.Top - bezel, scr.Width + bezel * 2, scr.Height + bezel * 2);

            // 1. Draw outer curved metallic bezel
            using (var bezelPath = GetRoundedRectPath(phoneBezel, 32))
            using (var bezelBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (var borderPen = new Pen(Color.FromArgb(51, 65, 85), 2f))
            {
                g.FillPath(bezelBrush, bezelPath);
                g.DrawPath(borderPen, bezelPath);
            }

            // 2. Draw screen clipping region
            using (var screenPath = GetRoundedRectPath(scr, 20))
            {
                g.SetClip(screenPath);

                // Screen wallpaper or default dark slate
                if (_wallpaper != null)
                {
                    g.DrawImage(_wallpaper, scr);
                }
                else
                {
                    using var dark = new SolidBrush(Color.FromArgb(10, 14, 26));
                    g.FillRectangle(dark, scr);
                }

                // Ambient wallpaper dimming
                using (var dim = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                {
                    g.FillRectangle(dim, scr);
                }

                // 3. Status Bar (time, wifi, battery)
                int statH = 26;
                Rectangle statBar = new Rectangle(scr.Left, scr.Top, scr.Width, statH);
                using (var fStat = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (var bWhite = new SolidBrush(Color.White))
                {
                    g.DrawString(DateTime.Now.ToString("HH:mm"), fStat, bWhite, scr.Left + 16, scr.Top + 6);
                    g.DrawString("5G ▮▮▮ 98%", fStat, bWhite, scr.Right - 85, scr.Top + 6);
                }

                // Punch-hole camera
                using (var bCam = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(bCam, scr.Left + scr.Width / 2 - 6, scr.Top + 7, 12, 12);
                }

                // 4. Chat Header
                int headH = 50;
                Rectangle headBar = new Rectangle(scr.Left, scr.Top + statH, scr.Width, headH);
                using (var bHead = new SolidBrush(Color.FromArgb(200, 15, 23, 42)))
                {
                    g.FillRectangle(bHead, headBar);
                }
                using (var fHead = new Font("Segoe UI", 11f, FontStyle.Bold))
                using (var fSub = new Font("Segoe UI", 8f))
                using (var bAvatar = new SolidBrush(Color.FromArgb(56, 189, 248)))
                using (var bWhite = new SolidBrush(Color.White))
                using (var bGreen = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    g.DrawString("←", fHead, bWhite, scr.Left + 12, headBar.Top + 13);
                    g.FillEllipse(bAvatar, scr.Left + 38, headBar.Top + 8, 34, 34);
                    g.DrawString("A", fHead, bWhite, scr.Left + 48, headBar.Top + 14);

                    g.DrawString("Audit Staff", fHead, bWhite, scr.Left + 80, headBar.Top + 7);
                    g.DrawString("● Online", fSub, bGreen, scr.Left + 80, headBar.Top + 27);
                    g.DrawString("📞  ⋮", fHead, bWhite, scr.Right - 55, headBar.Top + 13);
                }

                // 5. Chat Bubbles (drawn with _bubbleAlpha for cinematic fade)
                if (_bubbleAlpha > 0.01f)
                {
                    int alpha = (int)(255 * _bubbleAlpha);
                    using var fChat = new Font("Segoe UI", 10f);
                    using var fTime = new Font("Segoe UI", 7.5f);

                    // Received message
                    Rectangle recvBox = new Rectangle(scr.Left + 16, scr.Top + 110, (int)(scr.Width * 0.65f), 52);
                    using (var bRecv = new SolidBrush(Color.FromArgb(alpha, _receiverColor)))
                    using (var pRecv = GetRoundedRectPath(recvBox, 14))
                    using (var bText = new SolidBrush(Color.FromArgb(alpha, _fontColor)))
                    using (var bMeta = new SolidBrush(Color.FromArgb(alpha, Color.LightGray)))
                    {
                        g.FillPath(bRecv, pRecv);
                        g.DrawString("Hello! Testing new theme?", fChat, bText, recvBox.Left + 12, recvBox.Top + 10);
                        g.DrawString(DateTime.Now.ToString("HH:mm"), fTime, bMeta, recvBox.Right - 42, recvBox.Bottom - 18);
                    }

                    // Sent message
                    Rectangle sendBox = new Rectangle(scr.Right - (int)(scr.Width * 0.70f) - 16, scr.Top + 175, (int)(scr.Width * 0.70f), 65);
                    using (var bSend = new SolidBrush(Color.FromArgb(alpha, _senderColor)))
                    using (var pSend = GetRoundedRectPath(sendBox, 14))
                    using (var bText = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    using (var bMeta = new SolidBrush(Color.FromArgb(alpha, Color.LightGray)))
                    using (var bCheck = new SolidBrush(Color.FromArgb(alpha, 16, 185, 129)))
                    {
                        g.FillPath(bSend, pSend);
                        g.DrawString("Yes! Colors & particles match!", fChat, bText, sendBox.Left + 12, sendBox.Top + 10);
                        g.DrawString(DateTime.Now.ToString("HH:mm") + "  ✓✓", fTime, bCheck, sendBox.Right - 65, sendBox.Bottom - 18);
                    }
                }

                // 6. Cinematic Animation Rendering (if active)
                if (_isAnimRunning)
                {
                    float prog = (float)_animFrame / _animTotalFrames;

                    // Cracks across the glass (at 15% - 65%)
                    if (prog >= 0.15f && prog <= 0.65f)
                    {
                        using var crackPen = new Pen(Color.FromArgb(200, 255, 255, 255), 2f);
                        int cx = scr.Left + scr.Width / 2;
                        int cy = scr.Top + scr.Height / 2;
                        g.DrawLine(crackPen, cx, cy, cx - 70, cy - 80);
                        g.DrawLine(crackPen, cx - 70, cy - 80, cx - 120, cy - 130);
                        g.DrawLine(crackPen, cx, cy, cx + 80, cy - 60);
                        g.DrawLine(crackPen, cx + 80, cy - 60, cx + 110, cy - 120);
                        g.DrawLine(crackPen, cx, cy, cx - 50, cy + 90);
                        g.DrawLine(crackPen, cx, cy, cx + 60, cy + 100);
                    }

                    // Central animated figure scaling up (0.2x to 1.3x)
                    if (prog >= 0.20f && prog <= 0.65f)
                    {
                        float scaleFrac = (prog - 0.20f) / 0.45f;
                        float scale = 0.3f + scaleFrac * 1.0f;
                        int entitySize = (int)(160 * scale);
                        int ex = scr.Left + (scr.Width - entitySize) / 2;
                        int ey = scr.Top + (scr.Height - entitySize) / 2;

                        if (_animationStyle.Contains("Valentine") || _animationStyle.Contains("Heart"))
                        {
                            using var fHeart = new Font("Segoe UI Emoji", entitySize * 0.55f);
                            g.DrawString("💖", fHeart, Brushes.White, ex + 10, ey + 10);
                            using var fSub = new Font("Segoe UI", 11f, FontStyle.Bold);
                            g.DrawString("Happy Valentine's Day! 🌹", fSub, Brushes.LightPink, ex - 15, ey + entitySize);
                        }
                        else if (_animationStyle.Contains("Easter") || _animationStyle.Contains("Egg"))
                        {
                            using var fEgg = new Font("Segoe UI Emoji", entitySize * 0.55f);
                            string eSym = prog < 0.55f ? "🥚" : "🐰";
                            g.DrawString(eSym, fEgg, Brushes.White, ex + 10, ey + 10);
                            using var fSub = new Font("Segoe UI", 11f, FontStyle.Bold);
                            g.DrawString("Happy Easter! 🐣✨", fSub, Brushes.LightYellow, ex - 10, ey + entitySize);
                        }
                        else if (_animationStyle.Contains("Anime") || _animationStyle.Contains("Power"))
                        {
                            using var fP = new Font("Segoe UI Emoji", entitySize * 0.55f);
                            g.DrawString("⚡", fP, Brushes.White, ex + 10, ey + 10);
                            using var fSub = new Font("Segoe UI", 12f, FontStyle.Bold);
                            g.DrawString("MAXIMUM POWER! 💥", fSub, Brushes.Cyan, ex - 20, ey + entitySize);
                        }
                        else if (_animationStyle.Contains("Cartoon") || _animationStyle.Contains("Comic"))
                        {
                            using var fC = new Font("Segoe UI Emoji", entitySize * 0.55f);
                            g.DrawString("💥", fC, Brushes.White, ex + 10, ey + 10);
                            using var fSub = new Font("Segoe UI", 12f, FontStyle.Bold);
                            g.DrawString("POW! BOOM! ⭐", fSub, Brushes.Gold, ex - 10, ey + entitySize);
                        }
                        else if (_animationStyle.Contains("Santa") || _animationStyle.Contains("Christmas"))
                        {
                            using var fSanta = new Font("Segoe UI Emoji", entitySize * 0.5f);
                            g.DrawString("🎅", fSanta, Brushes.White, ex, ey);
                            using var fSub = new Font("Segoe UI", 11f, FontStyle.Bold);
                            g.DrawString("Ho Ho Ho! ❄️", fSub, Brushes.Gold, ex, ey + entitySize);
                        }
                        else
                        {
                            // Dark well vignette + Sadako crawling figure representation
                            using var bHole = new SolidBrush(Color.FromArgb(240, 0, 0, 0));
                            g.FillEllipse(bHole, ex + 20, ey + 40, entitySize - 40, entitySize - 60);
                            using var fMon = new Font("Segoe UI Emoji", entitySize * 0.45f);
                            g.DrawString("👻", fMon, Brushes.White, ex + 25, ey + 15);
                        }
                    }

                    // Climax flash at 60% - 65%
                    if (prog >= 0.58f && prog <= 0.64f)
                    {
                        using var bFlash = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
                        g.FillRectangle(bFlash, scr);
                    }
                }

                // 7. Particle Snow Cascade
                if (_particles.Count > 0)
                {
                    foreach (var p in _particles)
                    {
                        using var fPart = new Font("Segoe UI Emoji", p.Size);
                        using var bPart = new SolidBrush(Color.FromArgb(Math.Clamp(p.Alpha, 0, 255), Color.White));
                        float drawX = p.X + (float)Math.Sin(p.SwayPhase) * p.SwayAmp;
                        g.DrawString(p.Emoji, fPart, bPart, drawX, p.Y);
                    }
                }

                // 8. Bottom Composer Bar
                int compH = 54;
                Rectangle compBar = new Rectangle(scr.Left, scr.Bottom - compH, scr.Width, compH);
                using (var bComp = new SolidBrush(Color.FromArgb(220, 15, 23, 42)))
                {
                    g.FillRectangle(bComp, compBar);
                }

                Rectangle inputPill = new Rectangle(scr.Left + 52, compBar.Top + 8, scr.Width - 110, 38);
                using (var bPill = new SolidBrush(Color.FromArgb(30, 41, 59)))
                using (var pPill = GetRoundedRectPath(inputPill, 19))
                using (var fComp = new Font("Segoe UI", 10f))
                using (var fEmoji = new Font("Segoe UI Emoji", 13f))
                using (var bWhite = new SolidBrush(Color.White))
                using (var bHint = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (var bBlue = new SolidBrush(Color.FromArgb(0, 132, 255)))
                {
                    g.DrawString(">", fComp, bBlue, scr.Left + 18, compBar.Top + 16);
                    g.FillPath(bPill, pPill);
                    g.DrawString("Type message...", fComp, bHint, inputPill.Left + 16, inputPill.Top + 8);

                    // Theme Quick Action Emoji
                    g.DrawString(_quickEmoji, fEmoji, bWhite, scr.Right - 48, compBar.Top + 13);
                }

                g.ResetClip();
            }
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _particleTimer.Dispose();
                _animTimer.Dispose();
                _wallpaper?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
