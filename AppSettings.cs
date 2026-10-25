using System;
using System.IO;
using System.Text.Json;

namespace RMSThemeStudio
{
    public class AppSettings
    {
        public string SqlServer { get; set; } = @"192.168.32.45\sqlexpress, 1433";
        public string Database { get; set; } = "dbRMSAndroid";
        public string Username { get; set; } = "processaudit_user";
        public string Password { get; set; } = "processaudit1234*";
        public string TableName { get; set; } = "dbo.tbl_MessageThemes";

        public string ThemeBaseDirectory { get; set; } = @"D:\html\Android\RMS\MessageTheme";
        public string NotificationBaseDirectory { get; set; } = @"D:\html\Android\RMS\Notification";
        public string BaseWebUrl { get; set; } = "http://192.168.32.43/Android/RMS/";

        public string AiImageEndpoint { get; set; } = "https://image.pollinations.ai/prompt/{prompt}?width=1080&height=1920&model=flux&nologo=true";

        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_studio_settings.json");

        public string GetConnectionString()
        {
            return $"Server={SqlServer};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True;Connect Timeout=8;";
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading settings: " + ex.Message);
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving settings: " + ex.Message);
            }
        }
    }
}
