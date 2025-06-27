using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoDev.Infinity.Network;
using NoDev.InfinityToolLib.Panels;
using Newtonsoft.Json.Linq;
using NoDev.Infinity.User;
using NoDev.InfinityToolLib;

namespace NoDev.Infinity.Controls
{
    internal class HomePanel : InfinityPanel
    {
        internal HomePanel()
        {
            InitializeComponent();

            Me.OnLogin += OnLogin;
            Me.OnLogout += OnLogout;
        }

        private DevComponents.DotNetBar.PanelEx panelUpdates;
        private DevComponents.DotNetBar.LabelX lblNewsAndUpdates;
        private DevComponents.DotNetBar.Controls.GroupPanel grpLibrarySettings;
        private DevComponents.DotNetBar.Controls.GroupPanel grpLogin;
        private DevComponents.DotNetBar.ButtonX btnLogin;
        private DevComponents.DotNetBar.LabelX lblPassword;
        private DevComponents.DotNetBar.Controls.TextBoxX txtPassword;
        private DevComponents.DotNetBar.LabelX lblUsername;
        private DevComponents.DotNetBar.Controls.TextBoxX txtUsername;

        private void InitializeComponent()
        {
            this.panelUpdates = new DevComponents.DotNetBar.PanelEx();
            this.lblNewsAndUpdates = new DevComponents.DotNetBar.LabelX();
            this.grpLibrarySettings = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.grpLogin = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.btnLogin = new DevComponents.DotNetBar.ButtonX();
            this.lblPassword = new DevComponents.DotNetBar.LabelX();
            this.txtPassword = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblUsername = new DevComponents.DotNetBar.LabelX();
            this.txtUsername = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.panelUpdates.SuspendLayout();
            this.grpLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelUpdates
            // 
            this.panelUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelUpdates.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelUpdates.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelUpdates.Controls.Add(this.lblNewsAndUpdates);
            this.panelUpdates.DisabledBackColor = System.Drawing.Color.Empty;
            this.panelUpdates.Location = new System.Drawing.Point(358, 0);
            this.panelUpdates.Name = "panelUpdates";
            this.panelUpdates.Size = new System.Drawing.Size(242, 400);
            this.panelUpdates.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelUpdates.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.panelUpdates.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelUpdates.Style.BorderColor.Color = System.Drawing.Color.Silver;
            this.panelUpdates.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.Left;
            this.panelUpdates.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelUpdates.Style.GradientAngle = 90;
            this.panelUpdates.TabIndex = 5;
            // 
            // lblNewsAndUpdates
            // 
            // 
            // 
            // 
            this.lblNewsAndUpdates.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNewsAndUpdates.Font = new System.Drawing.Font("Roboto", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewsAndUpdates.ForeColor = System.Drawing.Color.Black;
            this.lblNewsAndUpdates.Location = new System.Drawing.Point(3, 4);
            this.lblNewsAndUpdates.Name = "lblNewsAndUpdates";
            this.lblNewsAndUpdates.Size = new System.Drawing.Size(236, 37);
            this.lblNewsAndUpdates.TabIndex = 0;
            this.lblNewsAndUpdates.Text = "News and Updates";
            this.lblNewsAndUpdates.TextAlignment = System.Drawing.StringAlignment.Center;
            // 
            // grpLibrarySettings
            // 
            this.grpLibrarySettings.BackColor = System.Drawing.Color.White;
            this.grpLibrarySettings.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.grpLibrarySettings.DisabledBackColor = System.Drawing.Color.Empty;
            this.grpLibrarySettings.Location = new System.Drawing.Point(13, 140);
            this.grpLibrarySettings.Name = "grpLibrarySettings";
            this.grpLibrarySettings.Size = new System.Drawing.Size(325, 231);
            // 
            // 
            // 
            this.grpLibrarySettings.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.grpLibrarySettings.Style.BackColorGradientAngle = 90;
            this.grpLibrarySettings.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLibrarySettings.Style.BorderBottomWidth = 1;
            this.grpLibrarySettings.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.grpLibrarySettings.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLibrarySettings.Style.BorderLeftWidth = 1;
            this.grpLibrarySettings.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLibrarySettings.Style.BorderRightWidth = 1;
            this.grpLibrarySettings.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLibrarySettings.Style.BorderTopWidth = 1;
            this.grpLibrarySettings.Style.CornerDiameter = 4;
            this.grpLibrarySettings.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.grpLibrarySettings.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.grpLibrarySettings.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.grpLibrarySettings.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.grpLibrarySettings.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.grpLibrarySettings.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.grpLibrarySettings.TabIndex = 7;
            this.grpLibrarySettings.Text = "Recent Files";
            // 
            // grpLogin
            // 
            this.grpLogin.BackColor = System.Drawing.Color.White;
            this.grpLogin.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.grpLogin.Controls.Add(this.btnLogin);
            this.grpLogin.Controls.Add(this.lblPassword);
            this.grpLogin.Controls.Add(this.txtPassword);
            this.grpLogin.Controls.Add(this.lblUsername);
            this.grpLogin.Controls.Add(this.txtUsername);
            this.grpLogin.DisabledBackColor = System.Drawing.Color.Empty;
            this.grpLogin.Font = new System.Drawing.Font("Roboto", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpLogin.Location = new System.Drawing.Point(13, 13);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(325, 121);
            // 
            // 
            // 
            this.grpLogin.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.grpLogin.Style.BackColorGradientAngle = 90;
            this.grpLogin.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLogin.Style.BorderBottomWidth = 1;
            this.grpLogin.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.grpLogin.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLogin.Style.BorderLeftWidth = 1;
            this.grpLogin.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLogin.Style.BorderRightWidth = 1;
            this.grpLogin.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.grpLogin.Style.BorderTopWidth = 1;
            this.grpLogin.Style.CornerDiameter = 4;
            this.grpLogin.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.grpLogin.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.grpLogin.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.grpLogin.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.grpLogin.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.grpLogin.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.grpLogin.TabIndex = 6;
            this.grpLogin.Text = "NoMod.com";
            // 
            // btnLogin
            // 
            this.btnLogin.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnLogin.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnLogin.Enabled = false;
            this.btnLogin.Location = new System.Drawing.Point(196, 23);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(84, 69);
            this.btnLogin.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Sign In";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lblPassword
            // 
            this.lblPassword.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblPassword.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblPassword.Font = new System.Drawing.Font("Roboto", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.ForeColor = System.Drawing.Color.Black;
            this.lblPassword.Location = new System.Drawing.Point(9, 49);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(181, 15);
            this.lblPassword.TabIndex = 10;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.txtPassword.Border.Class = "TextBoxBorder";
            this.txtPassword.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtPassword.DisabledBackColor = System.Drawing.Color.White;
            this.txtPassword.ForeColor = System.Drawing.Color.Black;
            this.txtPassword.Location = new System.Drawing.Point(9, 70);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PreventEnterBeep = true;
            this.txtPassword.Size = new System.Drawing.Size(181, 22);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtUsernamePassword_TextChanged);
            // 
            // lblUsername
            // 
            this.lblUsername.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblUsername.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblUsername.Font = new System.Drawing.Font("Roboto", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.ForeColor = System.Drawing.Color.Black;
            this.lblUsername.Location = new System.Drawing.Point(9, 2);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(181, 15);
            this.lblUsername.TabIndex = 8;
            this.lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.txtUsername.Border.Class = "TextBoxBorder";
            this.txtUsername.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtUsername.DisabledBackColor = System.Drawing.Color.White;
            this.txtUsername.ForeColor = System.Drawing.Color.Black;
            this.txtUsername.Location = new System.Drawing.Point(9, 23);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.PreventEnterBeep = true;
            this.txtUsername.Size = new System.Drawing.Size(181, 22);
            this.txtUsername.TabIndex = 0;
            this.txtUsername.TextChanged += new System.EventHandler(this.txtUsernamePassword_TextChanged);
            // 
            // HomePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.panelUpdates);
            this.Controls.Add(this.grpLibrarySettings);
            this.Controls.Add(this.grpLogin);
            this.Name = "HomePanel";
            this.Load += new System.EventHandler(this.HomePanel_Load);
            this.panelUpdates.ResumeLayout(false);
            this.grpLogin.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void HomePanel_Load(object sender, EventArgs e)
        {
            grpLogin.BackColor = Color.Transparent;
            grpLibrarySettings.BackColor = Color.Transparent;
           // this.btnLogin.Location = new System.Drawing.Point(196, 23);
        }

        private void OnLogin(object sender, EventArgs e)
        {
            txtUsername.ReadOnly = true;
            txtPassword.ReadOnly = true;

            txtUsername.Text = Me.Username;
            txtPassword.Clear();

            btnLogin.Enabled = true;
            btnLogin.Text = @"Sign Out";
        }

        private void OnLogout(object sender, EventArgs e)
        {
            txtUsername.ReadOnly = false;
            txtPassword.ReadOnly = false;

            btnLogin.Text = @"Sign In";
            txtUsernamePassword_TextChanged(null, null);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (Me.IsLoggedIn)
            {
                btnLogin.Enabled = false;

                try
                {
                    await Me.LogoutAsync();
                }
                catch
                {
                    btnLogin.Enabled = true;
                }
                
                return;
            }

            txtUsername.ReadOnly = true;
            txtPassword.ReadOnly = true;
            btnLogin.Enabled = false;
            btnLogin.Text = @"Signing in...";

            if (await Me.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text))
            {
                return;
            }

            txtUsername.ReadOnly = false;
            txtPassword.ReadOnly = false;
            btnLogin.Enabled = true;
            btnLogin.Text = @"Sign In";

            DialogBox.Show("Invalid username or password!", "Bad Login",
                MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Error);
        }

        private void txtUsernamePassword_TextChanged(object sender, EventArgs e)
        {
            if (!Me.IsLoggedIn)
            {
                btnLogin.Enabled = txtUsername.Text.Trim().Length != 0 && txtPassword.TextLength != 0;
            }
        }
    }
}
