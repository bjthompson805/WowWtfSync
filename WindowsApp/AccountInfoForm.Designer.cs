namespace WowWtfSync.WindowsApp
{
    partial class AccountInfoForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ─────────────────────────────────────────────────────────────
        private System.Windows.Forms.Label     lblHeader;

        private System.Windows.Forms.Panel     pnlSep1;
        private System.Windows.Forms.Label     lblUsernameKey;
        private System.Windows.Forms.Label     lblUsernameValue;
        private System.Windows.Forms.Label     lblEmailKey;
        private System.Windows.Forms.Label     lblEmailValue;

        private System.Windows.Forms.Panel     pnlSep2;
        private System.Windows.Forms.Label     lblSubscriptionKey;
        private System.Windows.Forms.Label     lblSubscriptionValue;
        private System.Windows.Forms.Label     lblPeriodEndKey;
        private System.Windows.Forms.Label     lblPeriodEndValue;

        private System.Windows.Forms.Panel     pnlSep3;
        private System.Windows.Forms.Label     lblLicenseKey;
        private System.Windows.Forms.TextBox   txtLicenseKey;
        private System.Windows.Forms.Button    btnCopyKey;

        private System.Windows.Forms.Panel     pnlSep4;
        private System.Windows.Forms.LinkLabel lnkDashboard;
        private System.Windows.Forms.Button    btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblHeader          = new System.Windows.Forms.Label();
            this.pnlSep1            = new System.Windows.Forms.Panel();
            this.lblUsernameKey     = new System.Windows.Forms.Label();
            this.lblUsernameValue   = new System.Windows.Forms.Label();
            this.lblEmailKey        = new System.Windows.Forms.Label();
            this.lblEmailValue      = new System.Windows.Forms.Label();
            this.pnlSep2            = new System.Windows.Forms.Panel();
            this.lblSubscriptionKey   = new System.Windows.Forms.Label();
            this.lblSubscriptionValue = new System.Windows.Forms.Label();
            this.lblPeriodEndKey    = new System.Windows.Forms.Label();
            this.lblPeriodEndValue  = new System.Windows.Forms.Label();
            this.pnlSep3            = new System.Windows.Forms.Panel();
            this.lblLicenseKey      = new System.Windows.Forms.Label();
            this.txtLicenseKey      = new System.Windows.Forms.TextBox();
            this.btnCopyKey         = new System.Windows.Forms.Button();
            this.pnlSep4            = new System.Windows.Forms.Panel();
            this.lnkDashboard       = new System.Windows.Forms.LinkLabel();
            this.btnClose           = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // ── lblHeader ─────────────────────────────────────────────────────
            this.lblHeader.AutoSize  = true;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHeader.Location  = new System.Drawing.Point(16, 16);
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Text      = "My Account";

            // ── pnlSep1 ───────────────────────────────────────────────────────
            this.pnlSep1.BackColor = System.Drawing.Color.LightGray;
            this.pnlSep1.Location  = new System.Drawing.Point(16, 50);
            this.pnlSep1.Name      = "pnlSep1";
            this.pnlSep1.Size      = new System.Drawing.Size(392, 1);
            this.pnlSep1.TabStop   = false;

            // ── Username row ──────────────────────────────────────────────────
            this.lblUsernameKey.AutoSize  = true;
            this.lblUsernameKey.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblUsernameKey.Location  = new System.Drawing.Point(16, 64);
            this.lblUsernameKey.Name      = "lblUsernameKey";
            this.lblUsernameKey.Text      = "Username:";

            this.lblUsernameValue.AutoSize = true;
            this.lblUsernameValue.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblUsernameValue.Location = new System.Drawing.Point(130, 64);
            this.lblUsernameValue.Name     = "lblUsernameValue";
            this.lblUsernameValue.Text     = "—";

            // ── Email row ─────────────────────────────────────────────────────
            this.lblEmailKey.AutoSize  = true;
            this.lblEmailKey.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblEmailKey.Location  = new System.Drawing.Point(16, 90);
            this.lblEmailKey.Name      = "lblEmailKey";
            this.lblEmailKey.Text      = "Email:";

            this.lblEmailValue.AutoSize    = true;
            this.lblEmailValue.Font        = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblEmailValue.Location    = new System.Drawing.Point(130, 90);
            this.lblEmailValue.MaximumSize = new System.Drawing.Size(278, 0);
            this.lblEmailValue.Name        = "lblEmailValue";
            this.lblEmailValue.Text        = "—";

            // ── pnlSep2 ───────────────────────────────────────────────────────
            this.pnlSep2.BackColor = System.Drawing.Color.LightGray;
            this.pnlSep2.Location  = new System.Drawing.Point(16, 120);
            this.pnlSep2.Name      = "pnlSep2";
            this.pnlSep2.Size      = new System.Drawing.Size(392, 1);
            this.pnlSep2.TabStop   = false;

            // ── Subscription row ──────────────────────────────────────────────
            this.lblSubscriptionKey.AutoSize  = true;
            this.lblSubscriptionKey.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSubscriptionKey.Location  = new System.Drawing.Point(16, 134);
            this.lblSubscriptionKey.Name      = "lblSubscriptionKey";
            this.lblSubscriptionKey.Text      = "Subscription:";

            this.lblSubscriptionValue.AutoSize = true;
            this.lblSubscriptionValue.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSubscriptionValue.Location = new System.Drawing.Point(130, 134);
            this.lblSubscriptionValue.Name     = "lblSubscriptionValue";
            this.lblSubscriptionValue.Text     = "—";

            // ── Renewal date row ──────────────────────────────────────────────
            this.lblPeriodEndKey.AutoSize  = true;
            this.lblPeriodEndKey.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPeriodEndKey.Location  = new System.Drawing.Point(16, 160);
            this.lblPeriodEndKey.Name      = "lblPeriodEndKey";
            this.lblPeriodEndKey.Text      = "Renews:";

            this.lblPeriodEndValue.AutoSize = true;
            this.lblPeriodEndValue.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPeriodEndValue.Location = new System.Drawing.Point(130, 160);
            this.lblPeriodEndValue.Name     = "lblPeriodEndValue";
            this.lblPeriodEndValue.Text     = "—";

            // ── pnlSep3 ───────────────────────────────────────────────────────
            this.pnlSep3.BackColor = System.Drawing.Color.LightGray;
            this.pnlSep3.Location  = new System.Drawing.Point(16, 192);
            this.pnlSep3.Name      = "pnlSep3";
            this.pnlSep3.Size      = new System.Drawing.Size(392, 1);
            this.pnlSep3.TabStop   = false;

            // ── License key row ───────────────────────────────────────────────
            this.lblLicenseKey.AutoSize  = true;
            this.lblLicenseKey.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLicenseKey.Location  = new System.Drawing.Point(16, 208);
            this.lblLicenseKey.Name      = "lblLicenseKey";
            this.lblLicenseKey.Text      = "License Key:";

            this.txtLicenseKey.BackColor   = System.Drawing.SystemColors.Control;
            this.txtLicenseKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLicenseKey.Font        = new System.Drawing.Font("Consolas", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtLicenseKey.Location    = new System.Drawing.Point(130, 205);
            this.txtLicenseKey.Name        = "txtLicenseKey";
            this.txtLicenseKey.ReadOnly    = true;
            this.txtLicenseKey.Size        = new System.Drawing.Size(216, 22);
            this.txtLicenseKey.TabStop     = false;

            this.btnCopyKey.Font     = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCopyKey.Location = new System.Drawing.Point(352, 203);
            this.btnCopyKey.Name     = "btnCopyKey";
            this.btnCopyKey.Size     = new System.Drawing.Size(56, 24);
            this.btnCopyKey.TabIndex = 0;
            this.btnCopyKey.Text     = "Copy";
            this.btnCopyKey.UseVisualStyleBackColor = true;
            this.btnCopyKey.Click   += new System.EventHandler(this.btnCopyKey_Click);

            // ── pnlSep4 ───────────────────────────────────────────────────────
            this.pnlSep4.BackColor = System.Drawing.Color.LightGray;
            this.pnlSep4.Location  = new System.Drawing.Point(16, 242);
            this.pnlSep4.Name      = "pnlSep4";
            this.pnlSep4.Size      = new System.Drawing.Size(392, 1);
            this.pnlSep4.TabStop   = false;

            // ── Footer row ────────────────────────────────────────────────────
            this.lnkDashboard.AutoSize    = true;
            this.lnkDashboard.Font        = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lnkDashboard.Location    = new System.Drawing.Point(16, 258);
            this.lnkDashboard.Name        = "lnkDashboard";
            this.lnkDashboard.TabIndex    = 1;
            this.lnkDashboard.TabStop     = true;
            this.lnkDashboard.Text        = "Open Dashboard";
            this.lnkDashboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDashboard_LinkClicked);

            this.btnClose.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnClose.Location = new System.Drawing.Point(320, 252);
            this.btnClose.Name     = "btnClose";
            this.btnClose.Size     = new System.Drawing.Size(88, 28);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text     = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click   += new System.EventHandler(this.btnClose_Click);

            // ── AccountInfoForm ───────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton        = this.btnClose;
            this.ClientSize          = new System.Drawing.Size(424, 296);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.pnlSep1);
            this.Controls.Add(this.lblUsernameKey);
            this.Controls.Add(this.lblUsernameValue);
            this.Controls.Add(this.lblEmailKey);
            this.Controls.Add(this.lblEmailValue);
            this.Controls.Add(this.pnlSep2);
            this.Controls.Add(this.lblSubscriptionKey);
            this.Controls.Add(this.lblSubscriptionValue);
            this.Controls.Add(this.lblPeriodEndKey);
            this.Controls.Add(this.lblPeriodEndValue);
            this.Controls.Add(this.pnlSep3);
            this.Controls.Add(this.lblLicenseKey);
            this.Controls.Add(this.txtLicenseKey);
            this.Controls.Add(this.btnCopyKey);
            this.Controls.Add(this.pnlSep4);
            this.Controls.Add(this.lnkDashboard);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "AccountInfoForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "My Account";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
