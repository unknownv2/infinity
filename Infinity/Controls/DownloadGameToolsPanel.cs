using System;
using System.Drawing;
using NoDev.InfinityToolLib.Panels;

namespace NoDev.Infinity.Controls
{
    internal class DownloadGameToolsPanel : InfinityPanel
    {
        internal enum PanelState
        {
            Idle,
            Downloading
        }

        internal string GameId { get; private set; }
        internal event EventHandler DownloadRequested;
        internal event EventHandler CancelRequested;
        internal PanelState State 
        {
            get { return _state; }
            set
            {
                _state = value;

                if (value == PanelState.Idle)
                {
                    progressDownload.Value = 0;
                    btnCancel.Enabled = false;
                    btnDownload.Enabled = true;
                }
                else
                {
                    btnDownload.Enabled = false;
                    btnCancel.Enabled = true;
                }
            } 
        }

        private PanelState _state;

        internal long BytesToDownload
        {
            get { return progressDownload.Maximum; }
            set
            {
                if (_state == PanelState.Idle)
                    throw new Exception("Cannot set download progress in idle state.");

                if (!progressDownload.InvokeRequired)
                    progressDownload.Maximum = (int)value;
                else
                {
                    progressDownload.Invoke((Action)(() =>
                    {
                        progressDownload.Maximum = (int)value;
                    }));
                }
            }
        }

        internal long BytesDownloaded
        {
            get { return progressDownload.Value; }
            set
            {
                if (_state == PanelState.Idle)
                    throw new Exception("Cannot set download progress in idle state.");

                if (!progressDownload.InvokeRequired)
                    progressDownload.Value = (int)value;
                else
                {
                    progressDownload.Invoke((Action)(() =>
                    {
                        progressDownload.Value = (int)value;
                    }));
                }
            }
        }

        internal DownloadGameToolsPanel(string gameId, string gameName, string[] toolNames)
        {
            GameId = gameId;

            InitializeComponent();

            lblMessage.Text = string.Format(lblMessage.Text, gameName);
            lblToolList.Text = string.Format(lblToolList.Text, string.Join(Environment.NewLine, toolNames));
        }

        private void NoAccessPanel_Load(object sender, EventArgs e)
        {
            lblToolList.ForeColor = Color.Gray;
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (CancelRequested != null)
                CancelRequested(this, e);
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (DownloadRequested != null)
                DownloadRequested(this, e);
        }

        private DevComponents.DotNetBar.Controls.ProgressBarX progressDownload;
        private DevComponents.DotNetBar.LabelX lblMessage;
        private DevComponents.DotNetBar.ButtonX btnDownload;
        private DevComponents.DotNetBar.LabelX lblToolList;
        private DevComponents.DotNetBar.ButtonX btnCancel;
        private DevComponents.DotNetBar.LabelX lblDownloadIcon;

        private void InitializeComponent()
        {
            this.lblToolList = new DevComponents.DotNetBar.LabelX();
            this.lblDownloadIcon = new DevComponents.DotNetBar.LabelX();
            this.progressDownload = new DevComponents.DotNetBar.Controls.ProgressBarX();
            this.lblMessage = new DevComponents.DotNetBar.LabelX();
            this.btnDownload = new DevComponents.DotNetBar.ButtonX();
            this.btnCancel = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // lblToolList
            // 
            this.lblToolList.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblToolList.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblToolList.Font = new System.Drawing.Font("Roboto", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToolList.ForeColor = System.Drawing.Color.Black;
            this.lblToolList.Location = new System.Drawing.Point(98, 117);
            this.lblToolList.Name = "lblToolList";
            this.lblToolList.Size = new System.Drawing.Size(406, 91);
            this.lblToolList.TabIndex = 5;
            this.lblToolList.Text = "Tools:\r\n{0}";
            this.lblToolList.TextAlignment = System.Drawing.StringAlignment.Center;
            this.lblToolList.WordWrap = true;
            // 
            // lblDownloadIcon
            // 
            this.lblDownloadIcon.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblDownloadIcon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblDownloadIcon.ForeColor = System.Drawing.Color.Black;
            this.lblDownloadIcon.Location = new System.Drawing.Point(98, 117);
            this.lblDownloadIcon.Name = "lblDownloadIcon";
            this.lblDownloadIcon.Size = new System.Drawing.Size(71, 91);
            this.lblDownloadIcon.Symbol = "";
            this.lblDownloadIcon.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblDownloadIcon.SymbolSize = 50F;
            this.lblDownloadIcon.TabIndex = 4;
            // 
            // progressDownload
            // 
            // 
            // 
            // 
            this.progressDownload.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.progressDownload.Location = new System.Drawing.Point(98, 214);
            this.progressDownload.Name = "progressDownload";
            this.progressDownload.Size = new System.Drawing.Size(406, 47);
            this.progressDownload.TabIndex = 7;
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblMessage.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblMessage.Font = new System.Drawing.Font("Roboto", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(6, 85);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(594, 26);
            this.lblMessage.TabIndex = 8;
            this.lblMessage.Text = "You have not downloaded the tools for {0} yet!";
            this.lblMessage.TextAlignment = System.Drawing.StringAlignment.Center;
            this.lblMessage.WordWrap = true;
            // 
            // btnDownload
            // 
            this.btnDownload.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnDownload.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnDownload.Location = new System.Drawing.Point(400, 267);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(104, 23);
            this.btnDownload.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnDownload.TabIndex = 9;
            this.btnDownload.Text = "Download Now";
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(327, 267);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(67, 23);
            this.btnCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // DownloadGameToolsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblDownloadIcon);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblToolList);
            this.Name = "DownloadGameToolsPanel";
            this.Load += new System.EventHandler(this.NoAccessPanel_Load);
            this.ResumeLayout(false);

        }
    }
}
