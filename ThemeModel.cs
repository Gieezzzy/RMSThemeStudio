using System;

namespace RMSThemeStudio
{
    public class ThemeModel
    {
        public int ThemeID { get; set; } = 0;
        public string ThemeName { get; set; } = "New Theme";
        public string ThemeBackgroundPath { get; set; } = "Background.png";
        public string ReceiverBubble { get; set; } = "#1E293B";
        public string SenderBubble { get; set; } = "#0284C7";
        public string TextFontColor { get; set; } = "#FFFFFF";
        public string DefaultIcon { get; set; } = "✨";
        public string SetIconGroup { get; set; } = "✨,⭐,🌟,💫,🔥";
        public string CallBackgroundPath { get; set; } = "CallBackground.png";
        public string Status { get; set; } = "Public"; // Public, Developing, Unavailable, Expired
        public string NotificationSound { get; set; } = "default_ping.wav";
        public string TriggerEmoji { get; set; } = "✨";
        public string AnimationStyle { get; set; } = "ParticleSnow"; // ParticleSnow, JumpscareCrawl, SantaFlyby, GlitchShake

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
