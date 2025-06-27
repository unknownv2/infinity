namespace NoDev.Infinity
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtWindowTitle = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.cbWindowTitleMode = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.lblWindowTitleMode = new DevComponents.DotNetBar.LabelX();
            this.lblColorScheme = new DevComponents.DotNetBar.LabelX();
            this.cbColorScheme = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.btnClose = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // txtWindowTitle
            // 
            this.txtWindowTitle.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.txtWindowTitle.Border.Class = "TextBoxBorder";
            this.txtWindowTitle.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtWindowTitle.DisabledBackColor = System.Drawing.Color.White;
            this.txtWindowTitle.ForeColor = System.Drawing.Color.Black;
            this.txtWindowTitle.Location = new System.Drawing.Point(115, 68);
            this.txtWindowTitle.Name = "txtWindowTitle";
            this.txtWindowTitle.PreventEnterBeep = true;
            this.txtWindowTitle.Size = new System.Drawing.Size(148, 22);
            this.txtWindowTitle.TabIndex = 3;
            this.txtWindowTitle.TextChanged += new System.EventHandler(this.txtWindowTitle_TextChanged);
            // 
            // cbWindowTitleMode
            // 
            this.cbWindowTitleMode.DisplayMember = "Text";
            this.cbWindowTitleMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbWindowTitleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbWindowTitleMode.ForeColor = System.Drawing.Color.Black;
            this.cbWindowTitleMode.FormattingEnabled = true;
            this.cbWindowTitleMode.ItemHeight = 16;
            this.cbWindowTitleMode.Location = new System.Drawing.Point(115, 40);
            this.cbWindowTitleMode.Name = "cbWindowTitleMode";
            this.cbWindowTitleMode.Size = new System.Drawing.Size(148, 22);
            this.cbWindowTitleMode.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cbWindowTitleMode.TabIndex = 2;
            // 
            // lblWindowTitleMode
            // 
            this.lblWindowTitleMode.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.lblWindowTitleMode.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblWindowTitleMode.ForeColor = System.Drawing.Color.Black;
            this.lblWindowTitleMode.Location = new System.Drawing.Point(6, 39);
            this.lblWindowTitleMode.Name = "lblWindowTitleMode";
            this.lblWindowTitleMode.Size = new System.Drawing.Size(100, 22);
            this.lblWindowTitleMode.TabIndex = 4;
            this.lblWindowTitleMode.Text = "Window Title Mode:";
            this.lblWindowTitleMode.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // lblColorScheme
            // 
            this.lblColorScheme.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.lblColorScheme.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblColorScheme.ForeColor = System.Drawing.Color.Black;
            this.lblColorScheme.Location = new System.Drawing.Point(6, 12);
            this.lblColorScheme.Name = "lblColorScheme";
            this.lblColorScheme.Size = new System.Drawing.Size(100, 22);
            this.lblColorScheme.TabIndex = 5;
            this.lblColorScheme.Text = "Color Scheme:";
            this.lblColorScheme.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // cbColorScheme
            // 
            this.cbColorScheme.DisplayMember = "Text";
            this.cbColorScheme.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbColorScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbColorScheme.ForeColor = System.Drawing.Color.Black;
            this.cbColorScheme.FormattingEnabled = true;
            this.cbColorScheme.ItemHeight = 16;
            this.cbColorScheme.Location = new System.Drawing.Point(115, 12);
            this.cbColorScheme.Name = "cbColorScheme";
            this.cbColorScheme.Size = new System.Drawing.Size(148, 22);
            this.cbColorScheme.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cbColorScheme.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnClose.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnClose.Location = new System.Drawing.Point(184, 96);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(79, 23);
            this.btnClose.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(275, 124);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.cbColorScheme);
            this.Controls.Add(this.lblColorScheme);
            this.Controls.Add(this.lblWindowTitleMode);
            this.Controls.Add(this.cbWindowTitleMode);
            this.Controls.Add(this.txtWindowTitle);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Infinity Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.TextBoxX txtWindowTitle;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cbWindowTitleMode;
        private DevComponents.DotNetBar.LabelX lblWindowTitleMode;
        private DevComponents.DotNetBar.LabelX lblColorScheme;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cbColorScheme;
        private DevComponents.DotNetBar.ButtonX btnClose;

    }
}