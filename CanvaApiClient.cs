using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RMSThemeStudio
{
    public class CanvaApiClient
    {
        private const string AuthUrl = "https://www.canva.com/api/oauth/authorize";
        private const string TokenUrl = "https://api.canva.com/rest/v1/oauth/token";
        private const string UserMeUrl = "https://api.canva.com/rest/v1/users/me";
        private const string ImageGenUrl = "https://api.canva.com/rest/v1/image-generations";
        public const string DefaultRedirectUri = "http://127.0.0.1:8088/callback";

        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        public static (string codeVerifier, string codeChallenge) GeneratePkce()
        {
            byte[] bytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            string codeVerifier = Base64UrlEncode(bytes);

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
                string codeChallenge = Base64UrlEncode(hash);
                return (codeVerifier, codeChallenge);
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static string BuildAuthorizeUrl(string clientId, string redirectUri, string codeChallenge, string state)
        {
            return $"{AuthUrl}?response_type=code" +
                   $"&client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   $"&scope={Uri.EscapeDataString("asset:read asset:write")}" +
                   $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                   $"&code_challenge_method=s256" +
                   $"&state={Uri.EscapeDataString(state)}";
        }

        public static async Task<(bool success, string accessToken, string refreshToken, int expiresIn, string error)> ExchangeCodeForTokenAsync(
            string clientId, string clientSecret, string code, string codeVerifier, string redirectUri)
        {
            try
            {
                var dict = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", clientId },
                    { "code", code },
                    { "code_verifier", codeVerifier },
                    { "redirect_uri", redirectUri }
                };

                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    dict["client_secret"] = clientSecret;
                }

                using var req = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
                {
                    Content = new FormUrlEncodedContent(dict)
                };

                // If client secret provided, also add Basic Auth header for Canva REST OAuth compliance
                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                }

                var resp = await _httpClient.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return (false, "", "", 0, $"Token exchange failed ({resp.StatusCode}): {json}");
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() ?? "" : "";
                string refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? "" : "";
                int expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 14400;

                return (true, accessToken, refreshToken, expiresIn, "");
            }
            catch (Exception ex)
            {
                return (false, "", "", 0, ex.Message);
            }
        }

        public static async Task<(bool success, string accessToken, int expiresIn, string error)> RefreshTokenAsync(
            string clientId, string clientSecret, string refreshToken)
        {
            try
            {
                var dict = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", clientId },
                    { "refresh_token", refreshToken }
                };

                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    dict["client_secret"] = clientSecret;
                }

                using var req = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
                {
                    Content = new FormUrlEncodedContent(dict)
                };

                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                }

                var resp = await _httpClient.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return (false, "", 0, $"Token refresh failed ({resp.StatusCode}): {json}");
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() ?? "" : "";
                int expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 14400;

                return (true, accessToken, expiresIn, "");
            }
            catch (Exception ex)
            {
                return (false, "", 0, ex.Message);
            }
        }

        public static async Task<(bool success, string displayName, string userId, string error)> VerifyConnectionAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, "", "", "No access token configured.");
            }

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, UserMeUrl);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var resp = await _httpClient.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return (false, "", "", $"Canva verification failed ({resp.StatusCode}): {json}");
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string displayName = "Canva Pro User";
                string userId = "";

                if (root.TryGetProperty("user", out var userElem))
                {
                    if (userElem.TryGetProperty("display_name", out var dn)) displayName = dn.GetString() ?? displayName;
                    if (userElem.TryGetProperty("id", out var uid)) userId = uid.GetString() ?? "";
                }

                return (true, displayName, userId, "");
            }
            catch (Exception ex)
            {
                return (false, "", "", ex.Message);
            }
        }

        public static async Task<(bool success, byte[] imageBytes, string error)> GenerateImageAsync(
            string accessToken, string prompt, Action<string> statusCallback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (false, null, "Canva Access Token is missing. Please authenticate with Canva first.");
            }

            try
            {
                statusCallback?.Invoke("Submitting image generation request to Canva AI...");

                string idempotencyKey = Guid.NewGuid().ToString();

                // Payload for Canva Connect API image-generations (idempotency_key is required by Canva API)
                var payloadObj = new Dictionary<string, object>
                {
                    { "prompt", prompt },
                    { "aspect_ratio", "portrait_2_3" }, // High resolution mobile phone ratio (2:3 / 9:16)
                    { "model", "craft_v1_2k" },
                    { "idempotency_key", idempotencyKey }
                };

                string postJson = JsonSerializer.Serialize(payloadObj);
                using var initialReq = new HttpRequestMessage(HttpMethod.Post, ImageGenUrl)
                {
                    Content = new StringContent(postJson, Encoding.UTF8, "application/json")
                };
                initialReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                initialReq.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);

                var initialResp = await _httpClient.SendAsync(initialReq, cancellationToken);
                string initialBody = await initialResp.Content.ReadAsStringAsync(cancellationToken);

                // If craft_v1_2k model was rejected or unavailable, retry without pinning model
                if (!initialResp.IsSuccessStatusCode && (initialBody.Contains("model_not_available") || initialBody.Contains("invalid_model") || initialBody.Contains("craft_v1_2k")))
                {
                    statusCallback?.Invoke("Switching to standard Canva AI model...");
                    payloadObj.Remove("model");
                    string retryKey = Guid.NewGuid().ToString();
                    payloadObj["idempotency_key"] = retryKey;
                    postJson = JsonSerializer.Serialize(payloadObj);

                    using var fallbackReq = new HttpRequestMessage(HttpMethod.Post, ImageGenUrl)
                    {
                        Content = new StringContent(postJson, Encoding.UTF8, "application/json")
                    };
                    fallbackReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    fallbackReq.Headers.TryAddWithoutValidation("Idempotency-Key", retryKey);

                    initialResp = await _httpClient.SendAsync(fallbackReq, cancellationToken);
                    initialBody = await initialResp.Content.ReadAsStringAsync(cancellationToken);
                }

                if (!initialResp.IsSuccessStatusCode)
                {
                    return (false, null, $"Canva AI generation request failed ({initialResp.StatusCode}): {initialBody}");
                }

                string jobId = "";
                using (var doc = JsonDocument.Parse(initialBody))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("job", out var jobProp) && jobProp.TryGetProperty("id", out var idProp))
                    {
                        jobId = idProp.GetString() ?? "";
                    }
                    else if (root.TryGetProperty("id", out var directId))
                    {
                        jobId = directId.GetString() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(jobId))
                {
                    return (false, null, "Could not retrieve Job ID from Canva AI response.");
                }

                statusCallback?.Invoke($"Canva AI generation job started (ID: {jobId}). Rendering layout...");

                // Poll job status
                string jobPollUrl = $"{ImageGenUrl}/{jobId}";
                int maxAttempts = 60; // 60 * 2.5s = ~150s timeout for high-res 2K models
                string downloadUrl = "";

                for (int i = 0; i < maxAttempts; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(2500, cancellationToken);

                    using var pollReq = new HttpRequestMessage(HttpMethod.Get, jobPollUrl);
                    pollReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var pollResp = await _httpClient.SendAsync(pollReq, cancellationToken);
                    if (!pollResp.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    string pollBody = await pollResp.Content.ReadAsStringAsync(cancellationToken);
                    using var pollDoc = JsonDocument.Parse(pollBody);
                    var pollRoot = pollDoc.RootElement;

                    string status = "";
                    JsonElement jobElem = pollRoot.TryGetProperty("job", out var jProp) ? jProp : pollRoot;

                    if (jobElem.TryGetProperty("status", out var sProp))
                    {
                        status = sProp.GetString() ?? "";
                    }
                    else if (pollRoot.TryGetProperty("status", out var rootStatusProp))
                    {
                        status = rootStatusProp.GetString() ?? "";
                    }

                    statusCallback?.Invoke($"Canva AI status: {status} (elapsed {(i + 1) * 2.5:F1}s)...");

                    // Check for success or completion
                    if (status.Equals("success", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("completed", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("done", StringComparison.OrdinalIgnoreCase))
                    {
                        // Canva Connect API places "result" as a sibling to "job" in pollRoot: pollRoot.result.url
                        downloadUrl = ExtractImageUrl(pollRoot);
                        if (!string.IsNullOrWhiteSpace(downloadUrl))
                        {
                            statusCallback?.Invoke("Canva AI generation completed! Download URL resolved.");
                            break;
                        }
                    }
                    else if (status.Equals("failed", StringComparison.OrdinalIgnoreCase))
                    {
                        string errMsg = "Canva AI generation failed.";
                        if (jobElem.TryGetProperty("error", out var errElem))
                        {
                            errMsg = errElem.ToString();
                        }
                        else if (pollRoot.TryGetProperty("error", out var rootErrElem))
                        {
                            errMsg = rootErrElem.ToString();
                        }
                        return (false, null, errMsg);
                    }
                    else
                    {
                        // In case status is missing but result is already ready
                        downloadUrl = ExtractImageUrl(pollRoot);
                        if (!string.IsNullOrWhiteSpace(downloadUrl))
                        {
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    return (false, null, "Canva AI generation timed out or download URL was not found.");
                }

                statusCallback?.Invoke("Downloading 2K high-resolution AI wallpaper from Canva...");
                
                // Safely download: attach Bearer token only if requesting api.canva.com, otherwise use plain GET for presigned CDN URLs
                using var downloadReq = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                if (downloadUrl.StartsWith("https://api.canva.com", StringComparison.OrdinalIgnoreCase))
                {
                    downloadReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                using var downloadResp = await _httpClient.SendAsync(downloadReq, cancellationToken);
                downloadResp.EnsureSuccessStatusCode();
                byte[] imgBytes = await downloadResp.Content.ReadAsByteArrayAsync(cancellationToken);

                return (true, imgBytes, "");
            }
            catch (Exception ex)
            {
                return (false, null, "Error during Canva AI generation: " + ex.Message);
            }
        }

        private static string ExtractImageUrl(JsonElement root)
        {
            // 1. Direct result.url (Official Canva Connect API format: root.result.url)
            if (root.TryGetProperty("result", out var res))
            {
                if (res.TryGetProperty("url", out var ru) && !string.IsNullOrWhiteSpace(ru.GetString()))
                    return ru.GetString();

                if (res.TryGetProperty("image", out var img) && img.TryGetProperty("url", out var iu) && !string.IsNullOrWhiteSpace(iu.GetString()))
                    return iu.GetString();

                if (res.TryGetProperty("urls", out var rus) && rus.ValueKind == JsonValueKind.Array && rus.GetArrayLength() > 0)
                {
                    string firstUrl = rus[0].GetString();
                    if (!string.IsNullOrWhiteSpace(firstUrl)) return firstUrl;
                }
            }

            // 2. Direct job.result.url
            if (root.TryGetProperty("job", out var job))
            {
                if (job.TryGetProperty("result", out var jRes))
                {
                    if (jRes.TryGetProperty("url", out var jru) && !string.IsNullOrWhiteSpace(jru.GetString()))
                        return jru.GetString();
                }
                if (job.TryGetProperty("generations", out var jGens) && jGens.ValueKind == JsonValueKind.Array && jGens.GetArrayLength() > 0)
                {
                    if (jGens[0].TryGetProperty("url", out var jgu) && !string.IsNullOrWhiteSpace(jgu.GetString()))
                        return jgu.GetString();
                }
            }

            // 3. generations array on root
            if (root.TryGetProperty("generations", out var gens) && gens.ValueKind == JsonValueKind.Array && gens.GetArrayLength() > 0)
            {
                if (gens[0].TryGetProperty("url", out var gu) && !string.IsNullOrWhiteSpace(gu.GetString()))
                    return gu.GetString();
            }

            // 4. asset.url
            if (root.TryGetProperty("asset", out var asset) && asset.TryGetProperty("url", out var au) && !string.IsNullOrWhiteSpace(au.GetString()))
            {
                return au.GetString();
            }

            // 5. Recursive fallback: find any string property named "url" or "download_url" starting with "http"
            return FindAnyHttpUrl(root);
        }

        private static string FindAnyHttpUrl(JsonElement elem)
        {
            if (elem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in elem.EnumerateObject())
                {
                    if ((prop.Name.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                         prop.Name.Equals("download_url", StringComparison.OrdinalIgnoreCase)) &&
                        prop.Value.ValueKind == JsonValueKind.String)
                    {
                        string val = prop.Value.GetString() ?? "";
                        if (val.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            val.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            return val;
                        }
                    }

                    string nested = FindAnyHttpUrl(prop.Value);
                    if (!string.IsNullOrWhiteSpace(nested)) return nested;
                }
            }
            else if (elem.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in elem.EnumerateArray())
                {
                    string nested = FindAnyHttpUrl(item);
                    if (!string.IsNullOrWhiteSpace(nested)) return nested;
                }
            }
            return "";
        }
    }
}
