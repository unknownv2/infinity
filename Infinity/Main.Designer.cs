using NoDev.Infinity.Controls;

namespace NoDev.Infinity
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            this.mainShell = new DevComponents.DotNetBar.Metro.MetroShell();
            this.statusBar = new DevComponents.DotNetBar.Metro.MetroStatusBar();
            this.tabStrip = new NoDev.Infinity.Controls.InfinityTabStrip();
            this.homeIcon = new DevComponents.DotNetBar.LabelItem();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.gameList = new GameList();
            this.SuspendLayout();
            // 
            // shellMain
            // 
            this.mainShell.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.mainShell.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.mainShell.CaptionVisible = true;
            this.mainShell.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainShell.Font = new System.Drawing.Font("Roboto", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainShell.ForeColor = System.Drawing.Color.Black;
            this.mainShell.HelpButtonText = null;
            this.mainShell.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.mainShell.Location = new System.Drawing.Point(1, 1);
            this.mainShell.Name = "shellMain";
            this.mainShell.Size = new System.Drawing.Size(852, 33);
            this.mainShell.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.mainShell.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.mainShell.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.mainShell.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.mainShell.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.mainShell.SystemText.QatDialogAddButton = "&Add >>";
            this.mainShell.SystemText.QatDialogCancelButton = "Cancel";
            this.mainShell.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.mainShell.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.mainShell.SystemText.QatDialogOkButton = "OK";
            this.mainShell.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.mainShell.SystemText.QatDialogRemoveButton = "&Remove";
            this.mainShell.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.mainShell.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.mainShell.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            this.mainShell.TabIndex = 0;
            this.mainShell.TabStripFont = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainShell.Text = "metroShell1";
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.statusBar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.statusBar.ContainerControlProcessDialogKey = true;
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.DragDropSupport = true;
            this.statusBar.Font = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBar.ForeColor = System.Drawing.Color.Black;
            this.statusBar.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.statusBar.Location = new System.Drawing.Point(253, 465);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(600, 22);
            this.statusBar.TabIndex = 7;
            this.statusBar.Text = "metroStatusBar1";
            // 
            // tabStrip
            // 
            this.tabStrip.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.tabStrip.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.tabStrip.ContainerControlProcessDialogKey = true;
            this.tabStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabStrip.Font = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabStrip.ForeColor = System.Drawing.Color.Black;
            this.tabStrip.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.homeIcon});
            this.tabStrip.KeyTipsFont = new System.Drawing.Font("Tahoma", 7F);
            this.tabStrip.Location = new System.Drawing.Point(253, 34);
            this.tabStrip.Name = "tabStrip";
            this.tabStrip.Size = new System.Drawing.Size(600, 22);
            this.tabStrip.TabIndex = 1;
            this.tabStrip.Text = "metroShell2";
            // 
            // lblHomeIcon
            // 
            this.homeIcon.ForeColor = System.Drawing.Color.White;
            this.homeIcon.ImageTextSpacing = 0;
            this.homeIcon.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.homeIcon.Name = "lblHomeIcon";
            this.homeIcon.Symbol = "";
            this.homeIcon.SymbolColor = System.Drawing.Color.White;
            this.homeIcon.Tooltip = "Go to home tab";
            this.homeIcon.Visible = false;
            this.homeIcon.Click += new System.EventHandler(this.HomeIcon_Click);
            this.homeIcon.MouseEnter += new System.EventHandler(this.HomeIcon_MouseEnter);
            this.homeIcon.MouseLeave += new System.EventHandler(this.HomeIcon_MouseLeave);
            // 
            // gameList
            // 
            this.gameList.CanvasColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.gameList.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2013;
            this.gameList.DisabledBackColor = System.Drawing.Color.Empty;
            this.gameList.Dock = System.Windows.Forms.DockStyle.Left;
            this.gameList.Location = new System.Drawing.Point(1, 34);
            this.gameList.Name = "gameList";
            this.gameList.Size = new System.Drawing.Size(252, 453);
            this.gameList.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.gameList.Style.BackColor1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.gameList.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.gameList.Style.BorderWidth = 0;
            this.gameList.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.gameList.Style.GradientAngle = 90;
            this.gameList.TabIndex = 2;
            // 
            // panelContainer
            // 
            this.panelContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.panelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContainer.ForeColor = System.Drawing.Color.Black;
            this.panelContainer.Location = new System.Drawing.Point(253, 56);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(600, 409);
            this.panelContainer.TabIndex = 8;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(854, 488);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.tabStrip);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.gameList);
            this.Controls.Add(this.mainShell);
            this.Font = new System.Drawing.Font("Roboto", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(854, 10000);
            this.MinimumSize = new System.Drawing.Size(854, 488);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Infinity - PC Game Modder";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.Metro.MetroShell mainShell;
        private NoDev.Infinity.Controls.GameList gameList;
        private DevComponents.DotNetBar.Metro.MetroStatusBar statusBar;
        private NoDev.Infinity.Controls.InfinityTabStrip tabStrip;
        private System.Windows.Forms.Panel panelContainer;
        private DevComponents.DotNetBar.LabelItem homeIcon;
    }
}

