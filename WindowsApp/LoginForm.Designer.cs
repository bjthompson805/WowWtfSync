namespace WowWtfSync.WindowsApp
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controls
        private System.Windows.Forms.Label     lblTitle;
        private System.Windows.Forms.Label     lblSubtitle;
        private System.Windows.Forms.Label     lblEmail;
        private System.Windows.Forms.TextBox   txtEmail;
        private System.Windows.Forms.Label     lblPassword;
        private System.Windows.Forms.TextBox   txtPassword;
        private System.Windows.Forms.Label     lblError;
        private System.Windows.Forms.Button    btnSignIn;
        private System.Windows.Forms.LinkLabel lnkRegister;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle    = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblEmail    = new System.Windows.Forms.Label();
            this.txtEmail    = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblError    = new System.Windows.Forms.Label();
            this.btnSignIn   = new System.Windows.Forms.Button();
            this.lnkRegister = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();

            // ── lblTitle ──────────────────────────────────────────────────────
            this.lblTitle.AutoSize  = true;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location  = new System.Drawing.Point(30, 28);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.TabIndex  = 0;
            this.lblTitle.Text      = "WoW WTF Sync";

            // ── lblSubtitle ───────────────────────────────────────────────────
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSubtitle.ForeColor = System.Drawing.Color.DimGray;
            this.lblSubtitle.Location  = new System.Drawing.Point(32, 62);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.TabIndex  = 1;
            this.lblSubtitle.Text      = "Sign in to access the application";

            // ── lblEmail ──────────────────────────────────────────────────────
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblEmail.Location = new System.Drawing.Point(30, 108);
            this.lblEmail.Name     = "lblEmail";
            this.lblEmail.TabIndex = 2;
            this.lblEmail.Text     = "Email";

            // ── txtEmail ──────────────────────────────────────────────────────
            this.txtEmail.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtEmail.Location = new System.Drawing.Point(30, 127);
            this.txtEmail.Name     = "txtEmail";
            this.txtEmail.Size     = new System.Drawing.Size(320, 23);
            this.txtEmail.TabIndex = 3;

            // ── lblPassword ───────────────────────────────────────────────────
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPassword.Location = new System.Drawing.Point(30, 168);
            this.lblPassword.Name     = "lblPassword";
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text     = "Password";

            // ── txtPassword ───────────────────────────────────────────────────
            this.txtPassword.Font         = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtPassword.Location     = new System.Drawing.Point(30, 187);
            this.txtPassword.Name         = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size         = new System.Drawing.Size(320, 23);
            this.txtPassword.TabIndex     = 5;

            // ── lblError ──────────────────────────────────────────────────────
            this.lblError.AutoSize    = true;
            this.lblError.ForeColor   = System.Drawing.Color.Firebrick;
            this.lblError.Font        = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblError.Location    = new System.Drawing.Point(30, 224);
            this.lblError.MaximumSize = new System.Drawing.Size(320, 0);
            this.lblError.Name        = "lblError";
            this.lblError.TabIndex    = 6;
            this.lblError.Text        = "";
            this.lblError.Visible     = false;

            // ── btnSignIn ─────────────────────────────────────────────────────
            this.btnSignIn.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSignIn.Location = new System.Drawing.Point(30, 250);
            this.btnSignIn.Name     = "btnSignIn";
            this.btnSignIn.Size     = new System.Drawing.Size(320, 30);
            this.btnSignIn.TabIndex = 7;
            this.btnSignIn.Text     = "Sign In";
            this.btnSignIn.UseVisualStyleBackColor = true;
            this.btnSignIn.Click   += new System.EventHandler(this.btnSignIn_Click);

            // ── lnkRegister ───────────────────────────────────────────────────
            this.lnkRegister.AutoSize    = true;
            this.lnkRegister.Font        = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lnkRegister.Location    = new System.Drawing.Point(30, 298);
            this.lnkRegister.Name        = "lnkRegister";
            this.lnkRegister.TabIndex    = 8;
            this.lnkRegister.TabStop     = true;
            this.lnkRegister.Text        = "Don't have an account? Create one at wowwtfsync.com";
            this.lnkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRegister_LinkClicked);

            // ── LoginForm ─────────────────────────────────────────────────────
            this.AcceptButton    = this.btnSignIn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode   = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize      = new System.Drawing.Size(380, 334);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnSignIn);
            this.Controls.Add(this.lnkRegister);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "LoginForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "WoW WTF Sync — Sign In";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
