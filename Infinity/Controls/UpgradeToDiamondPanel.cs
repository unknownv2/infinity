using System;
using System.Drawing;
using NoDev.InfinityToolLib.Panels;

namespace NoDev.Infinity.Controls
{
    internal class UpgradeToDiamondPanel : InfinityPanel
    {
        internal UpgradeToDiamondPanel()
        {
            InitializeComponent();
        }

        private DevComponents.DotNetBar.ButtonX btnUpgrade;
        private DevComponents.DotNetBar.ButtonX btnShare;
        private DevComponents.DotNetBar.LabelX lblMessage;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpgradeToDiamondPanel));
            this.lblMessage = new DevComponents.DotNetBar.LabelX();
            this.btnUpgrade = new DevComponents.DotNetBar.ButtonX();
            this.btnShare = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblMessage.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblMessage.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(22, 52);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(557, 230);
            this.lblMessage.TabIndex = 5;
            this.lblMessage.Text = resources.GetString("lblMessage.Text");
            this.lblMessage.TextAlignment = System.Drawing.StringAlignment.Center;
            this.lblMessage.WordWrap = true;
            // 
            // btnUpgrade
            // 
            this.btnUpgrade.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnUpgrade.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnUpgrade.Location = new System.Drawing.Point(182, 288);
            this.btnUpgrade.Name = "btnUpgrade";
            this.btnUpgrade.Size = new System.Drawing.Size(118, 27);
            this.btnUpgrade.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnUpgrade.TabIndex = 6;
            this.btnUpgrade.Text = "Upgrade Now!";
            // 
            // btnShare
            // 
            this.btnShare.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnShare.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnShare.Location = new System.Drawing.Point(306, 288);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(118, 27);
            this.btnShare.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnShare.TabIndex = 7;
            this.btnShare.Text = "Share";
            // 
            // UpgradeToDiamondPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.btnShare);
            this.Controls.Add(this.btnUpgrade);
            this.Controls.Add(this.lblMessage);
            this.Name = "UpgradeToDiamondPanel";
            this.Load += new System.EventHandler(this.UpgradeToDiamondPanel_Load);
            this.ResumeLayout(false);

        }

        private void UpgradeToDiamondPanel_Load(object sender, EventArgs e)
        {
            lblMessage.ForeColor = Color.Silver;
            lblMessage.ForeColor = Color.Gray;
        }
    }
}
