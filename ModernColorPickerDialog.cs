using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RMSThemeStudio
{
    public class ModernColorPickerDialog : Form
    {
        private Color _currentColor;
        private Color _originalColor;
        private bool _isUpdatingUi = false;

        private Panel pnlCurrentColor;
        private Panel pnlOriginalColor;
        private TextBox txtHexInput;
        private Button btnCopyHex;
        private TrackBar trackR, trackG, trackB;
        private Label lblValR, lblValG, lblValB;
        private FlowLayoutPanel pnlSwatches;
        private Button btnOk, btnCancel;

        // Curated Modern Executive Theme Palette
        private static readonly string[] PresetHexCodes = new[]
        {
            "#0084FF", "#38BDF8", "#0284C7", "#0369A1",
            "#10B981", "#059669", "#047857", "#064E3B",
            "#DC2626", "#B91C1C", "#991B1B", "#7F1D1D",
            "#F59E0B", "#D97706", "#B45309", "#78350F",
            "#8B5CF6", "#7C3AED", "#6D28D9", "#4C1D95",
            "#EC4899", "#DB2777", "#BE185D", "#831843",
            "#F43F5E", "#E11D48", "#BE123C", "#881337",
            "#10B981", "#14B8A6", "#06B6D4", "#0EA5E9",
            "#FFFFFF", "#F8FAFC", "#F1F5F9", "#E2E8F0",
            "#94A3B8", "#64748B", "#475569", "#334155",
            "#1E293B", "#0F172A", "#0B1120", "#020617"
        };

        public Color SelectedColor => _currentColor;
        public string SelectedHex => "#" + _currentColor.R.ToString("X2") + _currentColor.G.ToString("X2") + _currentColor.B.ToString("X2");

        public ModernColorPickerDialog(string initialHex = "#0084FF")
        {
            Color initCol;
            try
            {
                initCol = ColorTranslator.FromHtml(initialHex);
            }
            catch
            {
                initCol = Color.FromArgb(0, 132, 255);
            }

            _currentColor = initCol;
            _originalColor = initCol;

            InitializeComponent();
            UpdateUiFromColor(_currentColor);
        }

        private void InitializeComponent()
        {
            Text = "Executive Theme Studio Color Picker";
            Size = new Size(580, 560);
            MinimumSize = new Size(580, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            int y = 16;

            // Header title
            var lblTitle = new Label
            {
                Text = "HEX & RGB Modern Color Selector",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 32;

            // Top preview and HEX row
            var grpPreview = new GroupBox
            {
                Text = "Live Color Preview & HEX",
                Location = new Point(20, y),
                Size = new Size(520, 105),
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            pnlCurrentColor = new Panel
            {
                Location = new Point(16, 26),
                Size = new Size(90, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _currentColor
            };
            var lblCur = new Label { Text = "Selected", Location = new Point(16, 88), AutoSize = true, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(148, 163, 184) };

            pnlOriginalColor = new Panel
            {
                Location = new Point(116, 26),
                Size = new Size(90, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _originalColor,
                Cursor = Cursors.Hand
            };
            pnlOriginalColor.Click += (s, e) => SetColor(_originalColor);
            var lblOrig = new Label { Text = "Original (Click)", Location = new Point(116, 88), AutoSize = true, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(148, 163, 184) };

            var lblHex = new Label
            {
                Text = "HEX Code:",
                Location = new Point(226, 26),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            txtHexInput = new TextBox
            {
                Location = new Point(226, 50),
                Width = 160,
                Font = new Font("Consolas", 14f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = SelectedHex
            };
            txtHexInput.TextChanged += TxtHexInput_TextChanged;

            btnCopyHex = new Button
            {
                Location = new Point(396, 50),
                Width = 105,
                Height = 30,
                Text = "📋 Copy Hex",
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopyHex.FlatAppearance.BorderSize = 0;
            btnCopyHex.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(SelectedHex);
                    btnCopyHex.Text = "✓ Copied!";
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ts, te) => { btnCopyHex.Text = "📋 Copy Hex"; t.Stop(); t.Dispose(); };
                    t.Start();
                }
                catch { }
            };

            grpPreview.Controls.Add(pnlCurrentColor);
            grpPreview.Controls.Add(lblCur);
            grpPreview.Controls.Add(pnlOriginalColor);
            grpPreview.Controls.Add(lblOrig);
            grpPreview.Controls.Add(lblHex);
            grpPreview.Controls.Add(txtHexInput);
            grpPreview.Controls.Add(btnCopyHex);
            Controls.Add(grpPreview);
            y += 120;

            // RGB Sliders Group
            var grpRgb = new GroupBox
            {
                Text = "Precision RGB Tuning (0 - 255)",
                Location = new Point(20, y),
                Size = new Size(520, 135),
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            int ry = 22;
            trackR = CreateRgbRow(grpRgb, "Red (R):", Color.FromArgb(239, 68, 68), ref ry, out lblValR);
            trackG = CreateRgbRow(grpRgb, "Green (G):", Color.FromArgb(16, 185, 129), ref ry, out lblValG);
            trackB = CreateRgbRow(grpRgb, "Blue (B):", Color.FromArgb(59, 130, 246), ref ry, out lblValB);
            Controls.Add(grpRgb);
            y += 150;

            // Preset Swatches Palette
            var grpSwatches = new GroupBox
            {
                Text = "Curated Executive Theme Palette (Click Swatch to Select)",
                Location = new Point(20, y),
                Size = new Size(520, 130),
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            pnlSwatches = new FlowLayoutPanel
            {
                Location = new Point(10, 20),
                Size = new Size(500, 100),
                AutoScroll = false
            };

            foreach (var hex in PresetHexCodes)
            {
                Color col;
                try { col = ColorTranslator.FromHtml(hex); } catch { continue; }
                var p = new Panel
                {
                    Size = new Size(36, 22),
                    BackColor = col,
                    Margin = new Padding(2),
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand
                };
                var tip = new ToolTip();
                tip.SetToolTip(p, hex);
                p.Click += (s, e) => SetColor(col);
                pnlSwatches.Controls.Add(p);
            }
            grpSwatches.Controls.Add(pnlSwatches);
            Controls.Add(grpSwatches);
            y += 145;

            // Action Buttons
            btnOk = new Button
            {
                Location = new Point(280, y),
                Width = 130,
                Height = 36,
                Text = "✓ Select Color",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Location = new Point(420, y),
                Width = 120,
                Height = 36,
                Text = "Cancel",
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private TrackBar CreateRgbRow(GroupBox parent, string label, Color barTint, ref int ry, out Label valLabel)
        {
            var lbl = new Label { Text = label, Location = new Point(16, ry + 4), Width = 80, ForeColor = Color.White };
            var tb = new TrackBar
            {
                Location = new Point(100, ry),
                Width = 340,
                Minimum = 0,
                Maximum = 255,
                TickStyle = TickStyle.None
            };
            var lblVal = new Label { Text = "0", Location = new Point(455, ry + 4), Width = 45, ForeColor = barTint, Font = new Font("Consolas", 10f, FontStyle.Bold) };

            tb.ValueChanged += (s, e) =>
            {
                lblVal.Text = tb.Value.ToString();
                if (!_isUpdatingUi)
                {
                    SetColor(Color.FromArgb(trackR.Value, trackG.Value, trackB.Value));
                }
            };

            parent.Controls.Add(lbl);
            parent.Controls.Add(tb);
            parent.Controls.Add(lblVal);

            valLabel = lblVal;
            ry += 35;
            return tb;
        }

        private void SetColor(Color c)
        {
            _currentColor = c;
            UpdateUiFromColor(c);
        }

        private void UpdateUiFromColor(Color c)
        {
            _isUpdatingUi = true;
            try
            {
                pnlCurrentColor.BackColor = c;
                trackR.Value = c.R;
                trackG.Value = c.G;
                trackB.Value = c.B;
                lblValR.Text = c.R.ToString();
                lblValG.Text = c.G.ToString();
                lblValB.Text = c.B.ToString();

                string hex = SelectedHex;
                if (!txtHexInput.Text.Equals(hex, StringComparison.OrdinalIgnoreCase))
                {
                    txtHexInput.Text = hex;
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void TxtHexInput_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            string text = txtHexInput.Text.Trim();
            if (!text.StartsWith("#")) text = "#" + text;

            try
            {
                Color parsed = ColorTranslator.FromHtml(text);
                _currentColor = parsed;
                pnlCurrentColor.BackColor = parsed;

                _isUpdatingUi = true;
                trackR.Value = parsed.R;
                trackG.Value = parsed.G;
                trackB.Value = parsed.B;
                lblValR.Text = parsed.R.ToString();
                lblValG.Text = parsed.G.ToString();
                lblValB.Text = parsed.B.ToString();
                _isUpdatingUi = false;
            }
            catch
            {
                // Partial hex while typing - ignore
            }
        }
    }
}
