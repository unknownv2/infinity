using System;
using System.Drawing;
using NoDev.InfinityToolLib.Panels;

namespace NoDev.Infinity.Controls
{
    internal class NotLoggedInPanel : InfinityPanel
    {
        internal NotLoggedInPanel()
        {
            InitializeComponent();
        }

        private DevComponents.DotNetBar.LabelX lblNoAccessLogin;
        private DevComponents.DotNetBar.LabelX lblNoAccessArrow;
        private DevComponents.DotNetBar.LabelX lblNoAccessMessage;
        private DevComponents.DotNetBar.LabelX lblNoAccessLock;

        private void InitializeComponent()
        {
            this.lblNoAccessLogin = new DevComponents.DotNetBar.LabelX();
            this.lblNoAccessArrow = new DevComponents.DotNetBar.LabelX();
            this.lblNoAccessMessage = new DevComponents.DotNetBar.LabelX();
            this.lblNoAccessLock = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // lblNoAccessLogin
            // 
            this.lblNoAccessLogin.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNoAccessLogin.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNoAccessLogin.Font = new System.Drawing.Font("Roboto", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoAccessLogin.ForeColor = System.Drawing.Color.Black;
            this.lblNoAccessLogin.Location = new System.Drawing.Point(508, 7);
            this.lblNoAccessLogin.Name = "lblNoAccessLogin";
            this.lblNoAccessLogin.Size = new System.Drawing.Size(57, 39);
            this.lblNoAccessLogin.TabIndex = 7;
            this.lblNoAccessLogin.Text = "Click here to login";
            this.lblNoAccessLogin.TextAlignment = System.Drawing.StringAlignment.Far;
            this.lblNoAccessLogin.WordWrap = true;
            // 
            // lblNoAccessArrow
            // 
            this.lblNoAccessArrow.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNoAccessArrow.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNoAccessArrow.Font = new System.Drawing.Font("Roboto", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoAccessArrow.ForeColor = System.Drawing.Color.Black;
            this.lblNoAccessArrow.Location = new System.Drawing.Point(569, 3);
            this.lblNoAccessArrow.Name = "lblNoAccessArrow";
            this.lblNoAccessArrow.Size = new System.Drawing.Size(31, 41);
            this.lblNoAccessArrow.Symbol = "";
            this.lblNoAccessArrow.SymbolColor = System.Drawing.Color.Silver;
            this.lblNoAccessArrow.SymbolSize = 30F;
            this.lblNoAccessArrow.TabIndex = 6;
            this.lblNoAccessArrow.WordWrap = true;
            // 
            // lblNoAccessMessage
            // 
            this.lblNoAccessMessage.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNoAccessMessage.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNoAccessMessage.Font = new System.Drawing.Font("Roboto", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoAccessMessage.ForeColor = System.Drawing.Color.Black;
            this.lblNoAccessMessage.Location = new System.Drawing.Point(253, 141);
            this.lblNoAccessMessage.Name = "lblNoAccessMessage";
            this.lblNoAccessMessage.Size = new System.Drawing.Size(251, 125);
            this.lblNoAccessMessage.TabIndex = 5;
            this.lblNoAccessMessage.Text = "You must be logged in to use this tool!";
            this.lblNoAccessMessage.WordWrap = true;
            // 
            // lblNoAccessLock
            // 
            this.lblNoAccessLock.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblNoAccessLock.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblNoAccessLock.ForeColor = System.Drawing.Color.Black;
            this.lblNoAccessLock.Location = new System.Drawing.Point(98, 6);
            this.lblNoAccessLock.Name = "lblNoAccessLock";
            this.lblNoAccessLock.Size = new System.Drawing.Size(137, 360);
            this.lblNoAccessLock.Symbol = "";
            this.lblNoAccessLock.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblNoAccessLock.SymbolSize = 150F;
            this.lblNoAccessLock.TabIndex = 4;
            // 
            // NoAccessPanel
            // 
            this.Controls.Add(this.lblNoAccessLogin);
            this.Controls.Add(this.lblNoAccessArrow);
            this.Controls.Add(this.lblNoAccessMessage);
            this.Controls.Add(this.lblNoAccessLock);
            this.Name = "NotLoggedInPanel";
            this.Load += new System.EventHandler(this.NotLoggedInPanel_Load);
            this.ResumeLayout(false);

        }

        private void NotLoggedInPanel_Load(object sender, EventArgs e)
        {
            lblNoAccessMessage.ForeColor = Color.Silver;
            lblNoAccessLogin.ForeColor = Color.Gray;
        }
    }
}
