using System.Diagnostics;
using System.Windows.Forms;

namespace WowWtfSync.WindowsApp
{
    /// <summary>
    /// Read-only dialog that displays the current user's account details,
    /// subscription status, and license key.
    /// </summary>
    public partial class AccountInfoForm : Form
    {
        private readonly UserInfo _userInfo;

        public AccountInfoForm(UserInfo userInfo)
        {
            InitializeComponent();
            _userInfo = userInfo;
            PopulateFields();
        }

        // ── Population ───────────────────────────────────────────────────────────

        private void PopulateFields()
        {
            lblUsernameValue.Text    = _userInfo.Username ?? "—";
            lblEmailValue.Text       = _userInfo.Email    ?? "—";

            bool isActive = _userInfo.SubStatus == "active";
            lblSubscriptionValue.Text      = isActive ? "Active" : FormatStatus(_userInfo.SubStatus);
            lblSubscriptionValue.ForeColor = isActive
                ? System.Drawing.Color.SeaGreen
                : System.Drawing.Color.Firebrick;

            if (_userInfo.CurrentPeriodEnd.HasValue)
            {
                lblPeriodEndKey.Visible   = true;
                lblPeriodEndValue.Visible = true;
                lblPeriodEndValue.Text    = _userInfo.CurrentPeriodEnd.Value.ToString("MMMM d, yyyy");
            }
            else
            {
                lblPeriodEndKey.Visible   = false;
                lblPeriodEndValue.Visible = false;
            }

            if (!string.IsNullOrEmpty(_userInfo.LicenseKey))
            {
                txtLicenseKey.Text   = _userInfo.LicenseKey;
                btnCopyKey.Enabled   = true;
            }
            else
            {
                txtLicenseKey.Text   = "(none)";
                btnCopyKey.Enabled   = false;
            }
        }

        private static string FormatStatus(string? status) => status switch
        {
            "inactive"  => "Inactive",
            "trialing"  => "Trial",
            "past_due"  => "Past Due",
            "cancelled" => "Cancelled",
            "unpaid"    => "Unpaid",
            _           => status ?? "Unknown",
        };

        // ── Event handlers ───────────────────────────────────────────────────────

        private void btnCopyKey_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userInfo.LicenseKey)) return;

            Clipboard.SetText(_userInfo.LicenseKey);
            btnCopyKey.Text = "Copied!";

            // Reset button label after 2 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, ev) =>
            {
                btnCopyKey.Text = "Copy";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void lnkDashboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = "https://wowwtfsync.com/dashboard.html",
                UseShellExecute = true,
            });
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }
}
