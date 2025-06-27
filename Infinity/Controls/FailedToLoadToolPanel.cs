using System;
using System.Drawing;
using NoDev.InfinityToolLib.Panels;

namespace NoDev.Infinity.Controls
{
    internal class FailedToLoadToolPanel : InfinityPanel
    {
        internal FailedToLoadToolPanel()
        {
            InitializeComponent();
        }

        private DevComponents.DotNetBar.LabelX lblMessage;
        private DevComponents.DotNetBar.LabelX lblSadFace;

        private void InitializeComponent()
        {
            this.lblMessage = new DevComponents.DotNetBar.LabelX();
            this.lblSadFace = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblMessage.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblMessage.Font = new System.Drawing.Font("Roboto", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(253, 102);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(251, 167);
            this.lblMessage.TabIndex = 5;
            this.lblMessage.Text = "Uh-oh...\r\n\r\nSomething went wrong when trying loading this tool. Try restarting th" +
    "e program.";
            this.lblMessage.WordWrap = true;
            // 
            // lblSadFace
            // 
            this.lblSadFace.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblSadFace.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblSadFace.Font = new System.Drawing.Font("Microsoft Sans Serif", 125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSadFace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSadFace.Location = new System.Drawing.Point(113, 9);
            this.lblSadFace.Name = "lblSadFace";
            this.lblSadFace.Size = new System.Drawing.Size(117, 322);
            this.lblSadFace.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSadFace.SymbolSize = 150F;
            this.lblSadFace.TabIndex = 4;
            this.lblSadFace.Text = ":(";
            // 
            // FailedToLoadToolPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblSadFace);
            this.Name = "FailedToLoadToolPanel";
            this.Load += new System.EventHandler(this.FailedToLoadToolPanel_Load);
            this.ResumeLayout(false);

        }

        private void FailedToLoadToolPanel_Load(object sender, EventArgs e)
        {
            lblMessage.ForeColor = Color.Silver;
            lblSadFace.ForeColor = Color.Silver;
        }
    }
}
