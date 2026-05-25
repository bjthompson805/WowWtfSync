using System.Windows.Forms;
using WowWtfSync.WindowsApp;

namespace WowWtfSync
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Load saved config (includes any stored refresh token)
            JsonConfigFile.Load();

            // ── Step 1: Try silent authentication with the stored refresh token ──
            bool isAuthenticated = false;

            if (!string.IsNullOrEmpty(JsonConfigFile.RefreshToken))
            {
                // Run on thread pool — no SynchronizationContext installed yet, so no deadlock.
                var (refreshed, newToken) = Task.Run(async () =>
                    await LicenseService.TryRefreshTokenAsync(JsonConfigFile.RefreshToken!)
                ).GetAwaiter().GetResult();

                if (refreshed && newToken != null)
                {
                    // Rotate the persisted refresh token (server uses one-time tokens)
                    JsonConfigFile.RefreshToken = newToken;
                    JsonConfigFile.Save();
                    isAuthenticated = true;
                }
                else
                {
                    // Token is expired or invalid — force a fresh login
                    JsonConfigFile.RefreshToken = null;
                    JsonConfigFile.Save();
                }
            }

            // ── Step 2: Show login form if not silently authenticated ─────────────
            if (!isAuthenticated)
            {
                using var loginForm = new LoginForm();
                var dialogResult = loginForm.ShowDialog();

                // User dismissed the login window without signing in
                if (dialogResult != DialogResult.OK)
                    return;

                isAuthenticated = true;
            }

            // ── Step 3: Fetch account / subscription info ─────────────────────────
            UserInfo? userInfo = Task.Run(async () =>
                await LicenseService.GetCurrentUserAsync()
            ).GetAwaiter().GetResult();

            if (userInfo == null)
            {
                MessageBox.Show(
                    "Could not retrieve your account information.\n" +
                    "Please check your internet connection and try again.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // ── Step 4: Verify active subscription ────────────────────────────────
            if (userInfo.SubStatus != "active")
            {
                MessageBox.Show(
                    "Your subscription is not active.\n\n" +
                    "Please visit wowwtfsync.com to manage your subscription.",
                    "Subscription Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // ── Step 5: Verify license key exists ─────────────────────────────────
            if (string.IsNullOrEmpty(userInfo.LicenseKey))
            {
                MessageBox.Show(
                    "No license key was found for your account.\n\n" +
                    "Please visit your dashboard at wowwtfsync.com.",
                    "License Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // ── Step 6: Validate license key against this machine ─────────────────
            ValidateResult validateResult = Task.Run(async () =>
                await LicenseService.ValidateLicenseAsync(userInfo.LicenseKey)
            ).GetAwaiter().GetResult();

            if (!validateResult.Valid)
            {
                MessageBox.Show(
                    validateResult.ErrorMessage ?? "License validation failed.",
                    "License Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // ── All checks passed — launch the application ────────────────────────
            Application.Run(new wowWtfSyncForm(userInfo));
        }
    }
}
