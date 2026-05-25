using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace WowWtfSync.WindowsApp
{
    // ── DTOs ────────────────────────────────────────────────────────────────────

    public class LoginResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class UserInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("sub_status")]
        public string? SubStatus { get; set; }

        [JsonPropertyName("license_key")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("current_period_end")]
        public DateTime? CurrentPeriodEnd { get; set; }

        [JsonPropertyName("cancel_at_period_end")]
        public int CancelAtPeriodEnd { get; set; }
    }

    public class ValidateResult
    {
        public bool Valid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // ── Service ─────────────────────────────────────────────────────────────────

    public static class LicenseService
    {
        // Production API base URL. Update this if your API is hosted at a different path.
        private const string ApiBase = "https://wowwtfsync.com/api";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>Current short-lived JWT access token set after a successful login or refresh.</summary>
        public static string? AccessToken { get; private set; }

        // ── Login ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Authenticates with email + password.
        /// On success the caller is responsible for persisting result.RefreshToken.
        /// </summary>
        public static async Task<LoginResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"{ApiBase}/auth/login",
                    new { email, password }
                );

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await ReadErrorAsync(response);
                    return new LoginResult { Success = false, ErrorMessage = errorMsg };
                }

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                string accessToken  = json.GetProperty("accessToken").GetString()!;
                string refreshToken = json.GetProperty("refreshToken").GetString()!;

                AccessToken = accessToken;

                return new LoginResult
                {
                    Success      = true,
                    AccessToken  = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (TaskCanceledException)
            {
                return new LoginResult
                {
                    Success      = false,
                    ErrorMessage = "The request timed out. Please check your internet connection.",
                };
            }
            catch (HttpRequestException ex)
            {
                return new LoginResult
                {
                    Success      = false,
                    ErrorMessage = $"Could not reach the server. Please check your internet connection.\n({ex.Message})",
                };
            }
        }

        // ── Refresh ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Silently re-authenticates using a stored refresh token (token rotation).
        /// Returns (true, newRefreshToken) on success; the caller persists the new token.
        /// Returns (false, null) on failure — the stored token should be cleared.
        /// </summary>
        public static async Task<(bool Success, string? NewRefreshToken)> TryRefreshTokenAsync(
            string refreshToken)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"{ApiBase}/auth/refresh",
                    new { refreshToken }
                );

                if (!response.IsSuccessStatusCode)
                    return (false, null);

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                AccessToken = json.GetProperty("accessToken").GetString()!;
                string newRefreshToken = json.GetProperty("refreshToken").GetString()!;

                return (true, newRefreshToken);
            }
            catch
            {
                return (false, null);
            }
        }

        // ── Current user / subscription info ─────────────────────────────────────

        /// <summary>
        /// Fetches the currently authenticated user's profile, subscription status,
        /// and license key from GET /api/auth/me.
        /// Requires a valid AccessToken to be set on this service.
        /// </summary>
        public static async Task<UserInfo?> GetCurrentUserAsync()
        {
            if (string.IsNullOrEmpty(AccessToken))
                return null;

            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{ApiBase}/auth/me"
                );
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<UserInfo>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch
            {
                return null;
            }
        }

        // ── License validation ───────────────────────────────────────────────────

        /// <summary>
        /// Calls POST /api/license/validate with the user's license key and a stable
        /// hardware-derived machine ID.  Registers new machines automatically (up to 4).
        /// </summary>
        public static async Task<ValidateResult> ValidateLicenseAsync(string licenseKey)
        {
            try
            {
                string machineId    = GetMachineId();
                string machineLabel = Environment.MachineName;

                var response = await _httpClient.PostAsJsonAsync(
                    $"{ApiBase}/license/validate",
                    new { licenseKey, machineId, machineLabel }
                );

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                bool valid = json.TryGetProperty("valid", out var validProp) && validProp.GetBoolean();
                string? errorMsg = null;
                if (!valid && json.TryGetProperty("error", out var errProp))
                    errorMsg = errProp.GetString();

                return new ValidateResult { Valid = valid, ErrorMessage = errorMsg };
            }
            catch (TaskCanceledException)
            {
                return new ValidateResult
                {
                    Valid        = false,
                    ErrorMessage = "License validation timed out. Please check your internet connection.",
                };
            }
            catch (HttpRequestException ex)
            {
                return new ValidateResult
                {
                    Valid        = false,
                    ErrorMessage = $"Could not validate license. Please check your internet connection.\n({ex.Message})",
                };
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a stable, unique machine identifier.
        /// Uses the Windows CryptographyMachineGuid from the registry (preferred),
        /// falling back to the machine hostname if the registry is unavailable.
        /// </summary>
        public static string GetMachineId()
        {
            try
            {
                string? guid = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
                    "MachineGuid",
                    null
                ) as string;

                if (!string.IsNullOrWhiteSpace(guid))
                    return guid;
            }
            catch { /* fall through */ }

            return Environment.MachineName;
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (json.TryGetProperty("error", out var errProp))
                    return errProp.GetString() ?? "An error occurred.";
            }
            catch { /* ignore */ }

            return $"Request failed ({(int)response.StatusCode}).";
        }
    }
}
