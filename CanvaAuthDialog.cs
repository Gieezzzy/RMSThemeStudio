using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMSThemeStudio
{
    public class CanvaAuthDialog : Form
    {
        private readonly AppSettings _settings;

        private Label lblStatusIndicator;
        private Label lblStatusDetail;
        private TextBox txtClientId;
        private TextBox txtClientSecret;
        private TextBox txtManualCode;
        private Button btnOpenPortal;
        private Button btnStartAuth;
        private Button btnVerifyToken;
        private Button btnSave;
        private Button btnCancel;
        private ProgressBar progressBar;

        private HttpListener _httpListener;
        private CancellationTokenSource _authCts;

        public CanvaAuthDialog(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeCustomUi();
            LoadCurrentState();
        }

        private void InitializeCustomUi()
        {
            Text = "Canva Connect API & Pro AI Studio Configuration";
            Width = 680;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            int y = 20;

            // 1. Header
            var lblTitle = new Label
            {
                Text = "Canva Connect API Integration",
                Location = new Point(25, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252)
            };
            Controls.Add(lblTitle);
            y += 28;

            var lblSubtitle = new Label
            {
                Text = "Connect your Canva Pro account via the Canva Connect REST API to generate 2K AI backgrounds directly inside Theme Studio.",
                Location = new Point(25, y),
                Size = new Size(615, 36),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5f)
            };
            Controls.Add(lblSubtitle);
            y += 45;

            // 2. Status Card
            var pnlStatus = new Panel
            {
                Location = new Point(25, y),
                Size = new Size(615, 68),
                BackColor = Color.FromArgb(30, 41, 59), // Slate 800
                BorderStyle = BorderStyle.None
            };

            lblStatusIndicator = new Label
            {
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Text = "● Checking Connection...",
                ForeColor = Color.FromArgb(245, 158, 11)
            };
            pnlStatus.Controls.Add(lblStatusIndicator);

            lblStatusDetail = new Label
            {
                Location = new Point(16, 36),
                Size = new Size(580, 24),
                Font = new Font("Segoe UI", 8.5f),
                Text = "Verifying stored Canva access credentials...",
                ForeColor = Color.FromArgb(148, 163, 184)
            };
            pnlStatus.Controls.Add(lblStatusDetail);

            Controls.Add(pnlStatus);
            y += 80;

            // 3. Credentials Configuration
            var lblCredsHeader = new Label
            {
                Text = "Canva Developer Credentials",
                Location = new Point(25, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(124, 58, 237) // Purple Accent
            };
            Controls.Add(lblCredsHeader);
            y += 26;

            var lblClientPrompt = new Label
            {
                Text = "Client ID:",
                Location = new Point(25, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(226, 232, 240)
            };
            Controls.Add(lblClientPrompt);
            y += 22;

            txtClientId = new TextBox
            {
                Location = new Point(25, y),
                Size = new Size(460, 28),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10f)
            };
            Controls.Add(txtClientId);

            btnOpenPortal = new Button
            {
                Location = new Point(495, y - 1),
                Size = new Size(145, 30),
                Text = "🌐 Canva Portal",
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnOpenPortal.FlatAppearance.BorderSize = 0;
            btnOpenPortal.Click += (s, e) => OpenDeveloperPortal();
            Controls.Add(btnOpenPortal);
            y += 38;

            var lblSecretPrompt = new Label
            {
                Text = "Client Secret:",
                Location = new Point(25, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(226, 232, 240)
            };
            Controls.Add(lblSecretPrompt);
            y += 22;

            txtClientSecret = new TextBox
            {
                Location = new Point(25, y),
                Size = new Size(615, 28),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                Font = new Font("Consolas", 10f)
            };
            Controls.Add(txtClientSecret);
            y += 42;

            // 4. Authentication Action Button & Progress
            btnStartAuth = new Button
            {
                Location = new Point(25, y),
                Size = new Size(350, 36),
                Text = "🔐 Connect & Authorize Canva Account",
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnStartAuth.FlatAppearance.BorderSize = 0;
            btnStartAuth.Click += async (s, e) => await StartOAuthFlowAsync();
            Controls.Add(btnStartAuth);

            btnVerifyToken = new Button
            {
                Location = new Point(385, y),
                Size = new Size(255, 36),
                Text = "🔄 Verify Connection",
                BackColor = Color.FromArgb(37, 99, 235), // Blue
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnVerifyToken.FlatAppearance.BorderSize = 0;
            btnVerifyToken.Click += async (s, e) => await VerifyConnectionAsync();
            Controls.Add(btnVerifyToken);
            y += 44;

            progressBar = new ProgressBar
            {
                Location = new Point(25, y),
                Size = new Size(615, 6),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            Controls.Add(progressBar);
            y += 14;

            // 5. Manual Fallback Authorization Code (if loopback port 8088 is firewalled)
            var lblManualPrompt = new Label
            {
                Text = "Manual Auth Code (Optional Fallback if redirect doesn't auto-capture):",
                Location = new Point(25, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5f)
            };
            Controls.Add(lblManualPrompt);
            y += 20;

            txtManualCode = new TextBox
            {
                Location = new Point(25, y),
                Size = new Size(615, 24),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.5f)
            };
            Controls.Add(txtManualCode);
            y += 40;

            // 6. Bottom Action Buttons
            btnSave = new Button
            {
                Location = new Point(385, y),
                Size = new Size(120, 36),
                Text = "Save",
                BackColor = Color.FromArgb(16, 185, 129), // Emerald
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveAndClose();
            Controls.Add(btnSave);

            btnCancel = new Button
            {
                Location = new Point(515, y),
                Size = new Size(125, 36),
                Text = "Close",
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);
        }

        private async void LoadCurrentState()
        {
            txtClientId.Text = _settings.CanvaClientId ?? "";
            txtClientSecret.Text = _settings.CanvaClientSecret ?? "";

            if (!string.IsNullOrWhiteSpace(_settings.CanvaAccessToken))
            {
                await VerifyConnectionAsync();
            }
            else
            {
                lblStatusIndicator.Text = "● Not Connected";
                lblStatusIndicator.ForeColor = Color.FromArgb(148, 163, 184);
                lblStatusDetail.Text = "No active Canva API access token. Enter Client ID/Secret and click 'Connect & Authorize'.";
            }
        }

        private void OpenDeveloperPortal()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.canva.dev/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not launch browser: " + ex.Message, "Browser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task VerifyConnectionAsync()
        {
            string token = _settings.CanvaAccessToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                lblStatusIndicator.Text = "● Not Connected";
                lblStatusIndicator.ForeColor = Color.FromArgb(148, 163, 184);
                lblStatusDetail.Text = "Access token is empty. Please authorize with Canva.";
                return;
            }

            SetLoading(true, "Verifying connection with Canva Connect API...");
            var result = await CanvaApiClient.VerifyConnectionAsync(token);

            // If token expired and refresh token available, attempt refresh
            if (!result.success && !string.IsNullOrWhiteSpace(_settings.CanvaRefreshToken))
            {
                SetLoading(true, "Token expired. Refreshing Canva access token...");
                var refreshRes = await CanvaApiClient.RefreshTokenAsync(
                    _settings.CanvaClientId, _settings.CanvaClientSecret, _settings.CanvaRefreshToken);

                if (refreshRes.success)
                {
                    _settings.CanvaAccessToken = refreshRes.accessToken;
                    _settings.CanvaTokenExpiry = DateTime.UtcNow.AddSeconds(refreshRes.expiresIn);
                    _settings.Save();
                    result = await CanvaApiClient.VerifyConnectionAsync(_settings.CanvaAccessToken);
                }
            }

            SetLoading(false);

            if (result.success)
            {
                _settings.CanvaUserName = result.displayName;
                _settings.Save();

                lblStatusIndicator.Text = $"✓ Connected: {result.displayName}";
                lblStatusIndicator.ForeColor = Color.FromArgb(16, 185, 129); // Green
                lblStatusDetail.Text = $"Active Canva Connect API session verified. Ready to AI-generate 2K backgrounds.";
            }
            else
            {
                lblStatusIndicator.Text = "● Verification Failed";
                lblStatusIndicator.ForeColor = Color.FromArgb(239, 68, 68); // Red
                lblStatusDetail.Text = result.error;
            }
        }

        private async Task StartOAuthFlowAsync()
        {
            string clientId = txtClientId.Text.Trim();
            string clientSecret = txtClientSecret.Text.Trim();

            if (string.IsNullOrWhiteSpace(clientId))
            {
                MessageBox.Show("Please enter your Canva Client ID first.\n(Available from canva.dev under your App integration)",
                    "Client ID Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientId.Focus();
                return;
            }

            _settings.CanvaClientId = clientId;
            _settings.CanvaClientSecret = clientSecret;
            _settings.Save();

            var (codeVerifier, codeChallenge) = CanvaApiClient.GeneratePkce();
            string state = Guid.NewGuid().ToString("N");
            string redirectUri = CanvaApiClient.DefaultRedirectUri;

            string authUrl = CanvaApiClient.BuildAuthorizeUrl(clientId, redirectUri, codeChallenge, state);

            SetLoading(true, "Waiting for Canva authorization in browser...");

            _authCts?.Cancel();
            _authCts = new CancellationTokenSource();

            // Start loopback listener on http://127.0.0.1:8088/callback/
            Task<string> listenTask = StartLoopbackListenerAsync(redirectUri, state, _authCts.Token);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                SetLoading(false);
                MessageBox.Show("Could not launch browser: " + ex.Message, "Browser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string code = "";

            // Check if user manually pasted a code first, otherwise await loopback listener with 90s timeout
            var timeoutTask = Task.Delay(90000, _authCts.Token);
            var completedTask = await Task.WhenAny(listenTask, timeoutTask);

            if (completedTask == listenTask && listenTask.Status == TaskStatus.RanToCompletion)
            {
                code = listenTask.Result;
            }
            else if (!string.IsNullOrWhiteSpace(txtManualCode.Text))
            {
                code = txtManualCode.Text.Trim();
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                SetLoading(false);
                lblStatusIndicator.Text = "● Authorization Timed Out";
                lblStatusIndicator.ForeColor = Color.FromArgb(245, 158, 11);
                lblStatusDetail.Text = "If your browser did not redirect, paste the 'code' parameter into the Manual Auth Code box and retry.";
                return;
            }

            SetLoading(true, "Exchanging authorization code for Canva access token...");
            var tokenRes = await CanvaApiClient.ExchangeCodeForTokenAsync(clientId, clientSecret, code, codeVerifier, redirectUri);

            if (!tokenRes.success)
            {
                SetLoading(false);
                lblStatusIndicator.Text = "● Token Exchange Failed";
                lblStatusIndicator.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatusDetail.Text = tokenRes.error;
                MessageBox.Show("Token exchange failed:\n" + tokenRes.error, "OAuth Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _settings.CanvaAccessToken = tokenRes.accessToken;
            _settings.CanvaRefreshToken = tokenRes.refreshToken;
            _settings.CanvaTokenExpiry = DateTime.UtcNow.AddSeconds(tokenRes.expiresIn);
            _settings.Save();

            await VerifyConnectionAsync();
            MessageBox.Show($"Successfully authenticated with Canva Connect API!\nUser: {_settings.CanvaUserName}\n\nYou can now generate AI backgrounds directly inside Theme Studio.",
                "Canva Connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task<string> StartLoopbackListenerAsync(string redirectUri, string expectedState, CancellationToken ct)
        {
            try
            {
                _httpListener?.Close();
                _httpListener = new HttpListener();
                string prefix = redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/";
                _httpListener.Prefixes.Add(prefix);
                _httpListener.Start();

                var getContextTask = _httpListener.GetContextAsync();
                using (ct.Register(() => { try { _httpListener?.Stop(); } catch { } }))
                {
                    var ctx = await getContextTask;
                    var req = ctx.Request;
                    string code = req.QueryString["code"] ?? "";
                    string state = req.QueryString["state"] ?? "";

                    string responseHtml = @"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Canva Authorized</title><style>body{font-family:Segoe UI,sans-serif;background:#0F172A;color:white;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;}.card{background:#1E293B;padding:40px;border-radius:12px;text-align:center;box-shadow:0 10px 25px rgba(0,0,0,0.5);}h1{color:#10B981;margin:0 0 10px;}p{color:#94A3B8;margin:0;}</style></head><body><div class='card'><h1>✓ Canva Connected!</h1><p>Authorization successful. You may close this browser tab and return to RMS Theme Studio.</p></div></body></html>";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                    ctx.Response.ContentType = "text/html";
                    ctx.Response.ContentLength64 = buffer.Length;
                    await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    ctx.Response.OutputStream.Close();

                    return code;
                }
            }
            catch (Exception)
            {
                return "";
            }
            finally
            {
                try { _httpListener?.Close(); } catch { }
            }
        }

        private void SetLoading(bool loading, string message = "")
        {
            progressBar.Visible = loading;
            btnStartAuth.Enabled = !loading;
            btnVerifyToken.Enabled = !loading;
            btnSave.Enabled = !loading;
            if (!string.IsNullOrWhiteSpace(message))
            {
                lblStatusDetail.Text = message;
            }
        }

        private void SaveAndClose()
        {
            _settings.CanvaClientId = txtClientId.Text.Trim();
            _settings.CanvaClientSecret = txtClientSecret.Text.Trim();
            _settings.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _authCts?.Cancel();
            try { _httpListener?.Close(); } catch { }
            base.OnFormClosing(e);
        }
    }
}
