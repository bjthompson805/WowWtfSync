using System.Diagnostics;
using System.Windows.Forms;

namespace WowWtfSync.WindowsApp
{
    /// <summary>
    /// Modal login dialog shown when no valid session is found on startup.
    /// Returns DialogResult.OK when the user successfully authenticates.
    /// </summary>
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        // ── Event handlers ───────────────────────────────────────────────────────

        private async void btnSignIn_Click(object sender, EventArgs e)
        {
            string email    = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your email and password.");
                return;
            }

            SetBusy(true);

            try
            {
                var result = await LicenseService.LoginAsync(email, password);

                if (result.Success)
                {
                    // Persist the refresh token so the user doesn't need to log in next time
                    JsonConfigFile.RefreshToken = result.RefreshToken;
                    JsonConfigFile.Save();

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError(result.ErrorMessage ?? "Login failed. Please try again.");
                    SetBusy(false);
                }
            }
            catch (Exception ex)
            {
                ShowError($"An unexpected error occurred: {ex.Message}");
                SetBusy(false);
            }
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = "https://wowwtfsync.com/register.html",
                UseShellExecute = true,
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void ShowError(string message)
        {
            lblError.Text    = message;
            lblError.Visible = true;
        }

        private void SetBusy(bool busy)
        {
            btnSignIn.Enabled = !busy;
            btnSignIn.Text    = busy ? "Signing in…" : "Sign In";
            if (!busy) lblError.Visible = false;
        }
    }
}
