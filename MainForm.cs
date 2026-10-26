using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMSThemeStudio
{
    public partial class MainForm : Form
    {
        private AppSettings _settings;
        private DbManager _dbManager;
        private ThemeModel _currentTheme;
        private string _tempChatBgPath = "";
        private string _tempCallBgPath = "";
        private string _tempSoundPath = "";

        // UI Controls - Tabs
        private TabControl tabMain;
        private TabPage tabDesigner;
        private TabPage tabPlayground;
        private TabPage tabDatabase;
        private TabPage tabSettings;

        // UI Controls - Designer
        private TextBox txtThemeName;
        private ComboBox cmbStatus;
        private TextBox txtDefaultIcon;
        private TextBox txtSetIconGroup;
        private TextBox txtSenderBubble;
        private Button btnPickSenderColor;
        private Panel pnlSenderPreview;
        private TextBox txtReceiverBubble;
        private Button btnPickReceiverColor;
        private Panel pnlReceiverPreview;
        private TextBox txtTextFontColor;
        private Button btnPickFontColor;
        private Panel pnlFontPreview;
        private ComboBox cmbNotificationSound;
        private Button btnPlaySound;
        private ComboBox cmbSynthStyle;
        private Button btnSynthesize;
        private Button btnBrowseSound;
        private TextBox txtAiPrompt;
        private Button btnOpenCanva;
        private Button btnImportCanva;
        
        private Button btnGenerateProcedural;
        private Button btnBrowseBg;
        private TextBox txtCanvaEmail;
        private PictureBox picChatBgThumb;
        private PictureBox picCallBgThumb;
        private Button btnPublishTheme;
        private Label lblDesignerStatus;

        // UI Controls - Playground
        private PlaygroundSimulatorControl simulator;
        private Button btnPlaySnow;
        private Button btnPlayCinematic;
        private ComboBox cmbAnimStyle;
        private Label lblPlaygroundLog;

        // UI Controls - Database Explorer
        private DataGridView dgvThemes;
        private Button btnDbRefresh;
        private Button btnDbEdit;
        private Button btnDbDelete;
        private Button btnDbOpenFolder;
        private Label lblDbStatus;

        // UI Controls - Settings
        private TextBox txtSqlServer;
        private TextBox txtDatabase;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtTableName;
        private Button btnTestConnection;
        private Label lblConnResult;
        private TextBox txtThemeBasePath;
        private TextBox txtNotifBasePath;
        private TextBox txtBaseWebUrl;
        private Button btnSaveSettings;

        public MainForm()
        {
            _settings = AppSettings.Load();
            _dbManager = new DbManager(_settings);
            _currentTheme = new ThemeModel();

            InitializeComponent();
            ApplyInitialData();
        }

        private void InitializeComponent()
        {
            Text = "RMS Theme Studio & Playground - Executive Edition";
            Size = new Size(1180, 820);
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            // Top Header Banner
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 10, 20, 10)
            };
            var lblTitle = new Label
            {
                Text = "RMS Theme Studio & Interactive Playground",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            var lblSubtitle = new Label
            {
                Text = "Create, AI-Generate, Preview & Publish Chat Themes, Particles and Sound Effects directly to RMS Server",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(480, 22)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            Controls.Add(pnlHeader);

            // Main TabControl
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Padding = new Point(14, 8)
            };

            tabDesigner = new TabPage("🎨 Theme Designer") { BackColor = Color.FromArgb(15, 23, 42) };
            tabPlayground = new TabPage("📱 Mobile Playground") { BackColor = Color.FromArgb(15, 23, 42) };
            tabDatabase = new TabPage("🗄️ Database Explorer") { BackColor = Color.FromArgb(15, 23, 42) };
            tabSettings = new TabPage("⚙️ Studio Settings") { BackColor = Color.FromArgb(15, 23, 42) };

            tabMain.TabPages.Add(tabDesigner);
            tabMain.TabPages.Add(tabPlayground);
            tabMain.TabPages.Add(tabDatabase);
            tabMain.TabPages.Add(tabSettings);
            Controls.Add(tabMain);

            tabMain.BringToFront();

            BuildDesignerTab();
            BuildPlaygroundTab();
            BuildDatabaseTab();
            BuildSettingsTab();
        }

        private void BuildDesignerTab()
        {
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 460,
                Padding = new Padding(15),
                AutoScroll = true
            };

            int y = 15;
            pnlLeft.Controls.Add(CreateHeaderLabel("1. Basic Information", ref y));

            pnlLeft.Controls.Add(CreateLabel("Theme Name:", ref y));
            txtThemeName = new TextBox { Width = 400, Location = new Point(20, y), Text = "Cyber Neon" };
            y += 30;
            pnlLeft.Controls.Add(txtThemeName);

            pnlLeft.Controls.Add(CreateLabel("Status:", ref y));
            cmbStatus = new ComboBox { Width = 400, Location = new Point(20, y), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Public", "Developing", "Unavailable", "Expired" });
            cmbStatus.SelectedIndex = 0;
            y += 35;
            pnlLeft.Controls.Add(cmbStatus);

            pnlLeft.Controls.Add(CreateLabel("Default Theme Emoji (Quick Action):", ref y));
            txtDefaultIcon = new TextBox { Width = 120, Location = new Point(20, y), Text = "⚡", Font = new Font("Segoe UI Emoji", 11f) };
            y += 35;
            pnlLeft.Controls.Add(txtDefaultIcon);

            pnlLeft.Controls.Add(CreateLabel("Set Icon Group (Snowfall Multi-Emoji Set):", ref y));
            txtSetIconGroup = new TextBox { Width = 400, Location = new Point(20, y), Text = "⚡,🔥,✨,💎,🚀", Font = new Font("Segoe UI Emoji", 10f) };
            y += 40;
            pnlLeft.Controls.Add(txtSetIconGroup);

            pnlLeft.Controls.Add(CreateHeaderLabel("2. Bubble Styling (HEX)", ref y));

            pnlLeft.Controls.Add(CreateLabel("Sender Bubble Hex:", ref y));
            txtSenderBubble = new TextBox { Width = 140, Location = new Point(20, y), Text = "#0284C7" };
            pnlSenderPreview = new Panel { Location = new Point(170, y), Size = new Size(24, 24), BackColor = ColorTranslator.FromHtml("#0284C7"), BorderStyle = BorderStyle.FixedSingle };
            btnPickSenderColor = new Button { Location = new Point(205, y - 2), Width = 90, Height = 28, Text = "Pick Color" };
            btnPickSenderColor.Click += (s, e) => PickColor(txtSenderBubble, pnlSenderPreview);
            y += 35;
            pnlLeft.Controls.Add(txtSenderBubble);
            pnlLeft.Controls.Add(pnlSenderPreview);
            pnlLeft.Controls.Add(btnPickSenderColor);

            pnlLeft.Controls.Add(CreateLabel("Receiver Bubble Hex:", ref y));
            txtReceiverBubble = new TextBox { Width = 140, Location = new Point(20, y), Text = "#1E293B" };
            pnlReceiverPreview = new Panel { Location = new Point(170, y), Size = new Size(24, 24), BackColor = ColorTranslator.FromHtml("#1E293B"), BorderStyle = BorderStyle.FixedSingle };
            btnPickReceiverColor = new Button { Location = new Point(205, y - 2), Width = 90, Height = 28, Text = "Pick Color" };
            btnPickReceiverColor.Click += (s, e) => PickColor(txtReceiverBubble, pnlReceiverPreview);
            y += 35;
            pnlLeft.Controls.Add(txtReceiverBubble);
            pnlLeft.Controls.Add(pnlReceiverPreview);
            pnlLeft.Controls.Add(btnPickReceiverColor);

            pnlLeft.Controls.Add(CreateLabel("Text Font Color Hex:", ref y));
            txtTextFontColor = new TextBox { Width = 140, Location = new Point(20, y), Text = "#FFFFFF" };
            pnlFontPreview = new Panel { Location = new Point(170, y), Size = new Size(24, 24), BackColor = ColorTranslator.FromHtml("#FFFFFF"), BorderStyle = BorderStyle.FixedSingle };
            btnPickFontColor = new Button { Location = new Point(205, y - 2), Width = 90, Height = 28, Text = "Pick Color" };
            btnPickFontColor.Click += (s, e) => PickColor(txtTextFontColor, pnlFontPreview);
            y += 40;
            pnlLeft.Controls.Add(txtTextFontColor);
            pnlLeft.Controls.Add(pnlFontPreview);
            pnlLeft.Controls.Add(btnPickFontColor);

            pnlLeft.Controls.Add(CreateHeaderLabel("3. Custom Notification Audio", ref y));
            pnlLeft.Controls.Add(CreateLabel("Audio File (.wav):", ref y));
            cmbNotificationSound = new ComboBox { Width = 260, Location = new Point(20, y) };
            cmbNotificationSound.Items.AddRange(new object[] { "default_ping.wav", "jinglebells.wav", "halloween_spooky.wav", "freddy_whisper.wav", "jason_ki_ki_ma.wav", "sadako_creak.wav", "bonifacio_kapayapaan.wav" });
            cmbNotificationSound.SelectedIndex = 0;
            btnPlaySound = new Button { Location = new Point(290, y - 2), Width = 80, Height = 28, Text = "▶ Test", BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White };
            btnPlaySound.Click += (s, e) => PlaySelectedSound();
            y += 35;
            pnlLeft.Controls.Add(cmbNotificationSound);
            pnlLeft.Controls.Add(btnPlaySound);

            cmbSynthStyle = new ComboBox { Width = 180, Location = new Point(20, y), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSynthStyle.Items.AddRange(new object[] { "Christmas Bells", "Halloween Spooky", "Heartbeat Pulse", "Jumpscare Scream", "Bonifacio Fanfare", "Default Ping" });
            cmbSynthStyle.SelectedIndex = 0;
            btnSynthesize = new Button { Location = new Point(210, y - 2), Width = 110, Height = 28, Text = "Synthesize WAV" };
            btnSynthesize.Click += (s, e) => SynthesizeCurrentSound();
            btnBrowseSound = new Button { Location = new Point(330, y - 2), Width = 70, Height = 28, Text = "Browse" };
            btnBrowseSound.Click += (s, e) => BrowseSoundFile();
            y += 45;
            pnlLeft.Controls.Add(cmbSynthStyle);
            pnlLeft.Controls.Add(btnSynthesize);
            pnlLeft.Controls.Add(btnBrowseSound);

            // Right Panel: Background Generator & Preview
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            int yr = 15;
            pnlRight.Controls.Add(CreateHeaderLabel("4. Background Wallpaper Generation", ref yr));

            pnlRight.Controls.Add(CreateLabel("Topic / Prompt for Background (Canva/AI Theme Prompt):", ref yr));
            txtAiPrompt = new TextBox { Width = 550, Location = new Point(20, yr), Text = "Cyberpunk neon city alleyway, midnight rain, dark sleek atmosphere" };
            yr += 35;
            pnlRight.Controls.Add(txtAiPrompt);

            btnOpenCanva = new Button
            {
                Location = new Point(20, yr),
                Width = 270,
                Height = 34,
                Text = "🎨 Design in Canva (Google Login)",
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnOpenCanva.Click += (s, e) => OpenCanvaWithGoogleLogin();

            btnImportCanva = new Button
            {
                Location = new Point(300, yr),
                Width = 270,
                Height = 34,
                Text = "📥 Auto-Import from Canva Downloads",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnImportCanva.Click += (s, e) => AutoImportFromCanvaDownloads();
            yr += 40;
            pnlRight.Controls.Add(btnOpenCanva);
            pnlRight.Controls.Add(btnImportCanva);

            btnGenerateProcedural = new Button { Location = new Point(20, yr), Width = 270, Height = 32, Text = "✨ Generate Atmospheric Gradient", BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White };
            btnGenerateProcedural.Click += (s, e) => GenerateProceduralWallpaper();

            btnBrowseBg = new Button { Location = new Point(300, yr), Width = 270, Height = 32, Text = "📁 Browse Local Image File", BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White };
            btnBrowseBg.Click += (s, e) => BrowseBgFile();
            yr += 45;
            pnlRight.Controls.Add(btnGenerateProcedural);
            pnlRight.Controls.Add(btnBrowseBg);

            // Thumbnails
            var lblChatThumb = new Label { Text = "Chat Wallpaper (1080x1920):", Location = new Point(20, yr), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            var lblCallThumb = new Label { Text = "Call Wallpaper (1080x1920):", Location = new Point(250, yr), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            yr += 25;
            pnlRight.Controls.Add(lblChatThumb);
            pnlRight.Controls.Add(lblCallThumb);

            picChatBgThumb = new PictureBox { Location = new Point(20, yr), Size = new Size(180, 320), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(10, 14, 26), BorderStyle = BorderStyle.FixedSingle };
            picCallBgThumb = new PictureBox { Location = new Point(250, yr), Size = new Size(180, 320), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(10, 14, 26), BorderStyle = BorderStyle.FixedSingle };
            yr += 335;
            pnlRight.Controls.Add(picChatBgThumb);
            pnlRight.Controls.Add(picCallBgThumb);

            btnPublishTheme = new Button
            {
                Location = new Point(20, yr),
                Width = 410,
                Height = 44,
                Text = "🚀 SAVE & PUBLISH THEME TO RMS SERVER & DATABASE",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            btnPublishTheme.Click += async (s, e) => await PublishThemeAsync();
            yr += 50;
            pnlRight.Controls.Add(btnPublishTheme);

            lblDesignerStatus = new Label { Location = new Point(20, yr), AutoSize = true, ForeColor = Color.FromArgb(56, 189, 248) };
            pnlRight.Controls.Add(lblDesignerStatus);

            tabDesigner.Controls.Add(pnlRight);
            tabDesigner.Controls.Add(pnlLeft);
        }

        private void BuildPlaygroundTab()
        {
            var pnlPlayLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                Padding = new Padding(20)
            };

            int y = 20;
            pnlPlayLeft.Controls.Add(CreateHeaderLabel("Interactive Playground Controls", ref y));

            var lblDesc = new Label
            {
                Text = "Experience live chat wallpaper, sender & receiver colors, falling particle snow, and cinematic jumpscare/Santa animations in this real-time simulator.",
                Width = 340,
                Height = 60,
                Location = new Point(20, y),
                ForeColor = Color.FromArgb(148, 163, 184)
            };
            y += 65;
            pnlPlayLeft.Controls.Add(lblDesc);

            btnPlaySnow = new Button
            {
                Location = new Point(20, y),
                Width = 340,
                Height = 40,
                Text = "❄️ Test Particle Snowfall Cascade",
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnPlaySnow.Click += (s, e) =>
            {
                SyncSimulatorFromDesigner();
                simulator.TriggerSnowfall();
                lblPlaygroundLog.Text = $"[Snowfall] Triggered {txtSetIconGroup.Text} cascade!";
            };
            y += 55;
            pnlPlayLeft.Controls.Add(btnPlaySnow);

            pnlPlayLeft.Controls.Add(CreateLabel("Cinematic Animation Easter Egg Style:", ref y));
            cmbAnimStyle = new ComboBox { Width = 340, Location = new Point(20, y), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAnimStyle.Items.AddRange(new object[] {

                "Sadako / Horror Crawl (Well + Scream + Shake)",

                "Santa Flyby (Snow + Sleigh + Ho Ho Ho!)",

                "Valentine Hearts (3D Hearts + Romantic Harp)",

                "Easter Egg Hunt (Bunny + Golden Egg Hatch)",

                "Anime Action (Speedlines + Energy Blast)",

                "Cartoon Comic (POW! BOOM! + Whistle Boing)",

                "Cosmic Supernova (Custom Emoji Particle Storm)"

            });
            cmbAnimStyle.SelectedIndex = 0;
            y += 35;
            pnlPlayLeft.Controls.Add(cmbAnimStyle);

            btnPlayCinematic = new Button
            {
                Location = new Point(20, y),
                Width = 340,
                Height = 45,
                Text = "🎬 TRIGGER FULL CINEMATIC ANIMATION",
                BackColor = Color.FromArgb(225, 29, 72),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnPlayCinematic.Click += (s, e) =>
            {
                SyncSimulatorFromDesigner();
                simulator.TriggerCinematicAnimation();
                lblPlaygroundLog.Text = "[Cinematic] Sequence started: Chat dissolve -> Screen fracture -> Entity scaling -> Audio climax!";
            };
            y += 65;
            pnlPlayLeft.Controls.Add(btnPlayCinematic);

            var btnSyncPlayground = new Button
            {
                Location = new Point(20, y),
                Width = 340,
                Height = 35,
                Text = "🔄 Refresh Simulator from Designer",
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White
            };
            btnSyncPlayground.Click += (s, e) => SyncSimulatorFromDesigner();
            y += 50;
            pnlPlayLeft.Controls.Add(btnSyncPlayground);

            lblPlaygroundLog = new Label
            {
                Location = new Point(20, y),
                Width = 340,
                Height = 80,
                ForeColor = Color.FromArgb(56, 189, 248),
                Text = "Ready to test."
            };
            pnlPlayLeft.Controls.Add(lblPlaygroundLog);

            // Right side: Simulator Control
            simulator = new PlaygroundSimulatorControl
            {
                Dock = DockStyle.Fill
            };

            tabPlayground.Controls.Add(simulator);
            tabPlayground.Controls.Add(pnlPlayLeft);
        }

        private void BuildDatabaseTab()
        {
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(15)
            };

            btnDbRefresh = new Button { Location = new Point(15, 12), Width = 130, Height = 34, Text = "🔄 Refresh Themes", BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White };
            btnDbRefresh.Click += async (s, e) => await LoadThemesGridAsync();

            btnDbEdit = new Button { Location = new Point(155, 12), Width = 150, Height = 34, Text = "✏️ Load into Designer", BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White };
            btnDbEdit.Click += (s, e) => LoadSelectedThemeToDesigner();

            btnDbDelete = new Button { Location = new Point(315, 12), Width = 120, Height = 34, Text = "🗑️ Delete Theme", BackColor = Color.FromArgb(225, 29, 72), ForeColor = Color.White };
            btnDbDelete.Click += async (s, e) => await DeleteSelectedThemeAsync();

            btnDbOpenFolder = new Button { Location = new Point(445, 12), Width = 160, Height = 34, Text = "📁 Open Server Folder", BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White };
            btnDbOpenFolder.Click += (s, e) => OpenServerThemeFolder();

            lblDbStatus = new Label { Location = new Point(620, 20), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };

            pnlTop.Controls.Add(btnDbRefresh);
            pnlTop.Controls.Add(btnDbEdit);
            pnlTop.Controls.Add(btnDbDelete);
            pnlTop.Controls.Add(btnDbOpenFolder);
            pnlTop.Controls.Add(lblDbStatus);

            dgvThemes = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };

            tabDatabase.Controls.Add(dgvThemes);
            tabDatabase.Controls.Add(pnlTop);
        }

        private void BuildSettingsTab()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), AutoScroll = true };
            int y = 20;

            pnl.Controls.Add(CreateHeaderLabel("SQL Server Database Configuration", ref y));

            pnl.Controls.Add(CreateLabel("Server / IP & Instance:", ref y));
            txtSqlServer = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.SqlServer };
            y += 35;
            pnl.Controls.Add(txtSqlServer);

            pnl.Controls.Add(CreateLabel("Database Name:", ref y));
            txtDatabase = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.Database };
            y += 35;
            pnl.Controls.Add(txtDatabase);

            pnl.Controls.Add(CreateLabel("Database Username:", ref y));
            txtUsername = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.Username };
            y += 35;
            pnl.Controls.Add(txtUsername);

            pnl.Controls.Add(CreateLabel("Database Password:", ref y));
            txtPassword = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.Password, UseSystemPasswordChar = true };
            y += 35;
            pnl.Controls.Add(txtPassword);

            pnl.Controls.Add(CreateLabel("Target Table:", ref y));
            txtTableName = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.TableName };
            y += 35;
            pnl.Controls.Add(txtTableName);

            btnTestConnection = new Button { Location = new Point(30, y), Width = 180, Height = 35, Text = "🔌 Test DB Connection", BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White };
            btnTestConnection.Click += async (s, e) => await TestDbConnectionAsync();
            lblConnResult = new Label { Location = new Point(220, y + 8), AutoSize = true, ForeColor = Color.FromArgb(56, 189, 248) };
            y += 50;
            pnl.Controls.Add(btnTestConnection);
            pnl.Controls.Add(lblConnResult);

            pnl.Controls.Add(CreateHeaderLabel("Server Paths & Web Endpoints", ref y));

            pnl.Controls.Add(CreateLabel("MessageTheme Directory on Server:", ref y));
            txtThemeBasePath = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.ThemeBaseDirectory };
            y += 35;
            pnl.Controls.Add(txtThemeBasePath);

            pnl.Controls.Add(CreateLabel("Notification Audio Directory on Server:", ref y));
            txtNotifBasePath = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.NotificationBaseDirectory };
            y += 35;
            pnl.Controls.Add(txtNotifBasePath);

            pnl.Controls.Add(CreateLabel("Base Web URL:", ref y));
            txtBaseWebUrl = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.BaseWebUrl };
            y += 40;
            pnl.Controls.Add(txtBaseWebUrl);

            pnl.Controls.Add(CreateHeaderLabel("Connected Canva Account (Google Login)", ref y));
            pnl.Controls.Add(CreateLabel("Canva / Google Account Email:", ref y));
            txtCanvaEmail = new TextBox { Width = 500, Location = new Point(30, y), Text = _settings.CanvaAccountEmail, PlaceholderText = "e.g. audit.staff@gmail.com" };
            y += 45;
            pnl.Controls.Add(txtCanvaEmail);

            btnSaveSettings = new Button { Location = new Point(30, y), Width = 220, Height = 40, Text = "💾 Save All Settings", BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnSaveSettings.Click += (s, e) => SaveSettings();
            pnl.Controls.Add(btnSaveSettings);

            tabSettings.Controls.Add(pnl);
        }

        private void ApplyInitialData()
        {
            GenerateProceduralWallpaper();
            SynthesizeCurrentSound();
            SyncSimulatorFromDesigner();
            _ = LoadThemesGridAsync();
        }

        private void PickColor(TextBox targetBox, Panel targetPreview)
        {
            using var cd = new ModernColorPickerDialog(targetBox.Text);
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                targetBox.Text = cd.SelectedHex;
                targetPreview.BackColor = cd.SelectedColor;
                SyncSimulatorFromDesigner();
            }
        }

        private void PlaySelectedSound()
        {
            string soundName = cmbNotificationSound.Text.Trim();
            string serverFile = Path.Combine(_settings.NotificationBaseDirectory, soundName);

            if (File.Exists(_tempSoundPath))
            {
                AudioHelper.PlayWav(_tempSoundPath);
            }
            else if (File.Exists(serverFile))
            {
                AudioHelper.PlayWav(serverFile);
            }
            else
            {
                MessageBox.Show($"Audio file not found at: {serverFile}\nSynthesize or browse an audio file first.", "Sound Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SynthesizeCurrentSound()
        {
            string style = cmbSynthStyle.Text;
            string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_audio");
            Directory.CreateDirectory(outDir);
            string safeName = style.Replace(" ", "_").ToLower() + ".wav";
            _tempSoundPath = Path.Combine(outDir, safeName);

            AudioHelper.SynthesizeSound(style, _tempSoundPath);
            cmbNotificationSound.Text = safeName;
            lblDesignerStatus.Text = $"Synthesized '{safeName}' successfully!";
            PlaySelectedSound();
        }

        private void BrowseSoundFile()
        {
            using var ofd = new OpenFileDialog { Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tempSoundPath = ofd.FileName;
                cmbNotificationSound.Text = Path.GetFileName(ofd.FileName);
                lblDesignerStatus.Text = "Loaded sound: " + Path.GetFileName(ofd.FileName);
                PlaySelectedSound();
            }
        }

        private void OpenCanvaWithGoogleLogin()
        {
            using var inputForm = new Form
            {
                Text = "Canva Account Login via Google",
                Size = new Size(520, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White
            };

            var lblHeader = new Label
            {
                Text = "🎨 Canva Design Studio (Google Authentication)",
                Location = new Point(20, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248)
            };

            var lblMsg = new Label
            {
                Text = "Enter your Google / Canva account email to continue.\nA tab will open in your default browser for you to sign in via Google and access Canva directly.",
                Location = new Point(20, 48),
                Size = new Size(460, 42),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(203, 213, 225)
            };

            var lblEmail = new Label
            {
                Text = "Canva / Google Account Email:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtEmail = new TextBox
            {
                Location = new Point(20, 124),
                Width = 460,
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = !string.IsNullOrWhiteSpace(_settings.CanvaAccountEmail) ? _settings.CanvaAccountEmail : Environment.UserName + "@gmail.com"
            };

            var btnGo = new Button
            {
                Text = "🌐 Open Browser & Sign in via Google",
                Location = new Point(20, 175),
                Width = 300,
                Height = 40,
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnGo.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(335, 175),
                Width = 145,
                Height = 40,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            inputForm.Controls.Add(lblHeader);
            inputForm.Controls.Add(lblMsg);
            inputForm.Controls.Add(lblEmail);
            inputForm.Controls.Add(txtEmail);
            inputForm.Controls.Add(btnGo);
            inputForm.Controls.Add(btnCancel);
            inputForm.AcceptButton = btnGo;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string enteredEmail = txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(enteredEmail))
            {
                _settings.CanvaAccountEmail = enteredEmail;
                _settings.Save();
                if (txtCanvaEmail != null) txtCanvaEmail.Text = enteredEmail;
            }

            string prompt = txtAiPrompt.Text.Trim();
            string targetUrl;
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                string encodedSearch = Uri.EscapeDataString("phone wallpaper " + prompt);
                targetUrl = $"https://www.canva.com/login?redirect_to=%2Fsearch%2Ftemplates%3Fq%3D{encodedSearch}";
            }
            else
            {
                targetUrl = "https://www.canva.com/login?redirect_to=%2Fcreate%2Fphone-wallpapers%2F";
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetUrl,
                    UseShellExecute = true
                });

                lblDesignerStatus.Text = $"✓ Opened Canva in browser for {enteredEmail}! Sign in with Google, design 9:16 (1080x1920) wallpaper, download, then click 'Auto-Import from Canva Downloads'.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open browser: " + ex.Message, "Browser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AutoImportFromCanvaDownloads()
        {
            try
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsPath))
                {
                    BrowseBgFile();
                    return;
                }

                var dir = new DirectoryInfo(downloadsPath);
                var cutoff = DateTime.Now.AddHours(-4);
                var candidates = new System.Collections.Generic.List<FileInfo>();
                foreach (var f in dir.GetFiles("*.*"))
                {
                    string ext = f.Extension.ToLowerInvariant();
                    if ((ext == ".png" || ext == ".jpg" || ext == ".jpeg") && f.LastWriteTime >= cutoff)
                    {
                        candidates.Add(f);
                    }
                }
                candidates.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                if (candidates.Count > 0)
                {
                    var newest = candidates[0];
                    ReleaseThumbnailPreviews();

                    string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_bg");
                    Directory.CreateDirectory(outDir);
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                    _tempChatBgPath = Path.Combine(outDir, $"Background_{stamp}.png");
                    _tempCallBgPath = Path.Combine(outDir, $"CallBackground_{stamp}.png");

                    File.Copy(newest.FullName, _tempChatBgPath, true);
                    ImageGenerator.GenerateCallBackground(_tempChatBgPath, _tempCallBgPath);

                    LoadThumbnailPreviews();
                    SyncSimulatorFromDesigner();

                    lblDesignerStatus.Text = $"✓ Auto-imported latest Canva design: '{newest.Name}'";
                    MessageBox.Show($"Successfully auto-imported your Canva design:\n{newest.FullName}\n\nChat Wallpaper & Ambient Call Screen generated!", "Canva Wallpaper Imported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using var ofd = new OpenFileDialog
                    {
                        InitialDirectory = downloadsPath,
                        Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                        Title = "Select your downloaded Canva Wallpaper"
                    };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        ReleaseThumbnailPreviews();

                        string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_bg");
                        Directory.CreateDirectory(outDir);
                        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                        _tempChatBgPath = Path.Combine(outDir, $"Background_{stamp}.png");
                        _tempCallBgPath = Path.Combine(outDir, $"CallBackground_{stamp}.png");

                        File.Copy(ofd.FileName, _tempChatBgPath, true);
                        ImageGenerator.GenerateCallBackground(_tempChatBgPath, _tempCallBgPath);

                        LoadThumbnailPreviews();
                        SyncSimulatorFromDesigner();

                        lblDesignerStatus.Text = $"Loaded Canva wallpaper from: {Path.GetFileName(ofd.FileName)}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing Canva wallpaper: " + ex.Message, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void GenerateProceduralWallpaper()
        {
            ReleaseThumbnailPreviews();

            string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_bg");
            Directory.CreateDirectory(outDir);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            _tempChatBgPath = Path.Combine(outDir, $"Background_{stamp}.png");
            _tempCallBgPath = Path.Combine(outDir, $"CallBackground_{stamp}.png");

            Color dom = pnlSenderPreview.BackColor;
            Color sec = pnlReceiverPreview.BackColor;

            ImageGenerator.GenerateAtmosphericProcedural(txtThemeName.Text, dom, sec, _tempChatBgPath);
            ImageGenerator.GenerateCallBackground(_tempChatBgPath, _tempCallBgPath);

            LoadThumbnailPreviews();
            SyncSimulatorFromDesigner();
            lblDesignerStatus.Text = "Atmospheric procedural wallpaper generated!";
        }

        private void BrowseBgFile()
        {
            using var ofd = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.svg)|*.png;*.jpg;*.svg" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ReleaseThumbnailPreviews();

                string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_bg");
                Directory.CreateDirectory(outDir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                _tempChatBgPath = Path.Combine(outDir, $"Background_{stamp}.png");
                _tempCallBgPath = Path.Combine(outDir, $"CallBackground_{stamp}.png");

                File.Copy(ofd.FileName, _tempChatBgPath, true);
                ImageGenerator.GenerateCallBackground(_tempChatBgPath, _tempCallBgPath);

                LoadThumbnailPreviews();
                SyncSimulatorFromDesigner();
                lblDesignerStatus.Text = "Loaded wallpaper from: " + Path.GetFileName(ofd.FileName);
            }
        }

        private void ReleaseThumbnailPreviews()
        {
            try
            {
                if (picChatBgThumb.Image != null)
                {
                    var img = picChatBgThumb.Image;
                    picChatBgThumb.Image = null;
                    img.Dispose();
                }
                if (picCallBgThumb.Image != null)
                {
                    var img = picCallBgThumb.Image;
                    picCallBgThumb.Image = null;
                    img.Dispose();
                }
            }
            catch { }
        }

        private void LoadThumbnailPreviews()
        {
            try
            {
                ReleaseThumbnailPreviews();
                if (File.Exists(_tempChatBgPath)) picChatBgThumb.Image = ImageGenerator.LoadImageNoLock(_tempChatBgPath);
                if (File.Exists(_tempCallBgPath)) picCallBgThumb.Image = ImageGenerator.LoadImageNoLock(_tempCallBgPath);
            }
            catch { }
        }

        private void SyncSimulatorFromDesigner()
        {
            var model = new ThemeModel
            {
                ThemeName = txtThemeName.Text,
                SenderBubble = txtSenderBubble.Text,
                ReceiverBubble = txtReceiverBubble.Text,
                TextFontColor = txtTextFontColor.Text,
                DefaultIcon = txtDefaultIcon.Text,
                SetIconGroup = txtSetIconGroup.Text,
                AnimationStyle = cmbAnimStyle.Text
            };

            simulator.SetTheme(model, _tempChatBgPath, _tempSoundPath);
        }

        private async Task PublishThemeAsync()
        {
            string themeName = txtThemeName.Text.Trim();
            if (string.IsNullOrWhiteSpace(themeName))
            {
                MessageBox.Show("Theme Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnPublishTheme.Enabled = false;
            lblDesignerStatus.Text = "Publishing theme files and updating database...";

            try
            {
                // 1. Create Theme Directory on Server
                string themeDir = Path.Combine(_settings.ThemeBaseDirectory, themeName);
                Directory.CreateDirectory(themeDir);

                // 2. Save Wallpapers
                string destChatBg = Path.Combine(themeDir, "Background.png");
                string destCallBg = Path.Combine(themeDir, "CallBackground.png");

                if (File.Exists(_tempChatBgPath)) File.Copy(_tempChatBgPath, destChatBg, true);
                if (File.Exists(_tempCallBgPath)) File.Copy(_tempCallBgPath, destCallBg, true);

                // 3. Save Notification Sound
                string soundName = cmbNotificationSound.Text.Trim();
                if (string.IsNullOrEmpty(soundName)) soundName = "default_ping.wav";
                string destSound = Path.Combine(_settings.NotificationBaseDirectory, soundName);
                Directory.CreateDirectory(_settings.NotificationBaseDirectory);

                if (File.Exists(_tempSoundPath))
                {
                    File.Copy(_tempSoundPath, destSound, true);
                }

                // 4. Update Database
                var theme = new ThemeModel
                {
                    ThemeID = _currentTheme.ThemeID,
                    ThemeName = themeName,
                    ThemeBackgroundPath = $"{_settings.BaseWebUrl}MessageTheme/{themeName}/Background.png",
                    CallBackgroundPath = $"{_settings.BaseWebUrl}MessageTheme/{themeName}/CallBackground.png",
                    SenderBubble = txtSenderBubble.Text.Trim(),
                    ReceiverBubble = txtReceiverBubble.Text.Trim(),
                    TextFontColor = txtTextFontColor.Text.Trim(),
                    DefaultIcon = txtDefaultIcon.Text.Trim(),
                    SetIconGroup = txtSetIconGroup.Text.Trim(),
                    Status = cmbStatus.Text
                };

                var dbRes = await _dbManager.UpsertThemeAsync(theme);

                if (dbRes.Success)
                {
                    _currentTheme = theme;
                    lblDesignerStatus.Text = "Theme published successfully! " + dbRes.Message;
                    MessageBox.Show($"Theme '{themeName}' published!\nFiles written to: {themeDir}\nSound saved to: {destSound}\nDatabase: {dbRes.Message}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadThemesGridAsync();
                }
                else
                {
                    lblDesignerStatus.Text = "Database error: " + dbRes.Message;
                    MessageBox.Show(dbRes.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblDesignerStatus.Text = "Publish error: " + ex.Message;
                MessageBox.Show("Error publishing theme: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPublishTheme.Enabled = true;
            }
        }

        private async Task LoadThemesGridAsync()
        {
            lblDbStatus.Text = "Loading themes from database...";
            var themes = await _dbManager.LoadThemesAsync();
            dgvThemes.DataSource = themes;
            lblDbStatus.Text = $"Loaded {themes.Count} theme(s) from SQL Server.";
        }

        private void LoadSelectedThemeToDesigner()
        {
            if (dgvThemes.CurrentRow?.DataBoundItem is ThemeModel m)
            {
                _currentTheme = m;
                txtThemeName.Text = m.ThemeName;
                cmbStatus.Text = m.Status;
                txtDefaultIcon.Text = m.DefaultIcon;
                txtSetIconGroup.Text = m.SetIconGroup;
                txtSenderBubble.Text = m.SenderBubble;
                txtReceiverBubble.Text = m.ReceiverBubble;
                txtTextFontColor.Text = string.IsNullOrWhiteSpace(m.TextFontColor) ? "#FFFFFF" : m.TextFontColor;
                try { pnlSenderPreview.BackColor = ColorTranslator.FromHtml(m.SenderBubble); } catch { }
                try { pnlReceiverPreview.BackColor = ColorTranslator.FromHtml(m.ReceiverBubble); } catch { }
                try { pnlFontPreview.BackColor = ColorTranslator.FromHtml(txtTextFontColor.Text); } catch { }

                string serverThemeDir = Path.Combine(_settings.ThemeBaseDirectory, m.ThemeName);
                string localChatBg = Path.Combine(serverThemeDir, "Background.png");
                string localCallBg = Path.Combine(serverThemeDir, "CallBackground.png");

                if (File.Exists(localChatBg)) _tempChatBgPath = localChatBg;
                if (File.Exists(localCallBg)) _tempCallBgPath = localCallBg;

                LoadThumbnailPreviews();
                SyncSimulatorFromDesigner();

                tabMain.SelectedTab = tabDesigner;
                lblDesignerStatus.Text = $"Loaded theme '{m.ThemeName}' (ID {m.ThemeID}) into Designer.";
            }
        }

        private async Task DeleteSelectedThemeAsync()
        {
            if (dgvThemes.CurrentRow?.DataBoundItem is ThemeModel m)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete theme '{m.ThemeName}' (ID {m.ThemeID}) from database?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    var res = await _dbManager.DeleteThemeAsync(m.ThemeID);
                    MessageBox.Show(res.Message, "Delete Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadThemesGridAsync();
                }
            }
        }

        private void OpenServerThemeFolder()
        {
            if (Directory.Exists(_settings.ThemeBaseDirectory))
            {
                Process.Start("explorer.exe", _settings.ThemeBaseDirectory);
            }
            else
            {
                MessageBox.Show("Theme directory not found: " + _settings.ThemeBaseDirectory, "Folder Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task TestDbConnectionAsync()
        {
            var tempSettings = new AppSettings
            {
                SqlServer = txtSqlServer.Text.Trim(),
                Database = txtDatabase.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                TableName = txtTableName.Text.Trim()
            };

            lblConnResult.Text = "Testing connection...";
            lblConnResult.ForeColor = Color.FromArgb(56, 189, 248);

            var tempDb = new DbManager(tempSettings);
            var result = await tempDb.TestConnectionAsync();

            lblConnResult.Text = result.Message;
            lblConnResult.ForeColor = result.Success ? Color.FromArgb(16, 185, 129) : Color.FromArgb(225, 29, 72);
        }

        private void SaveSettings()
        {
            _settings.SqlServer = txtSqlServer.Text.Trim();
            _settings.Database = txtDatabase.Text.Trim();
            _settings.Username = txtUsername.Text.Trim();
            _settings.Password = txtPassword.Text.Trim();
            _settings.TableName = txtTableName.Text.Trim();
            _settings.ThemeBaseDirectory = txtThemeBasePath.Text.Trim();
            _settings.NotificationBaseDirectory = txtNotifBasePath.Text.Trim();
            _settings.BaseWebUrl = txtBaseWebUrl.Text.Trim();
            if (txtCanvaEmail != null) _settings.CanvaAccountEmail = txtCanvaEmail.Text.Trim();

            _settings.Save();
            _dbManager = new DbManager(_settings);
            MessageBox.Show("Settings saved and applied successfully!", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Label CreateLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(148, 163, 184)
            };
            y += 20;
            return lbl;
        }

        private Label CreateHeaderLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(15, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248)
            };
            y += 30;
            return lbl;
        }
    }
}
