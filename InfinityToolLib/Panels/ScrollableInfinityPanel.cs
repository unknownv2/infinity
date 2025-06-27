using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;

namespace NoDev.InfinityToolLib.Panels
{
    public class ScrollableInfinityPanel : InfinityPanel
    {
        protected PanelEx ScrollPanel;

        public ScrollableInfinityPanel()
        {
            SuspendLayout();

            ScrollPanel = new PanelEx
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                Size = Size
            };

            ScrollPanel.Layout += (s, e) => ScrollPanel.ShowHorizontalScrollBar(false);

            Controls.Add(ScrollPanel);

            /* executed in designer mode only */
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                /* draw a dotted line in the designer so we know what the user sees by default */
                var designerLine = new Line
                {
                    Size = new Size(Size.Width, 1),
                    Location = new Point(0, Size.Height),
                    DashStyle = DashStyle.Dash
                };

                /* show the scrollbar and line in the designer if the tool is greater than the default window height */
                Layout += (s, e) =>
                {
                    if (Height > MinimumSize.Height)
                    {
                        ScrollPanel.ShowVerticalScrollBar();
                        ScrollPanel.Controls.Add(designerLine);
                    }
                    else if (ScrollPanel.Controls.Contains(designerLine))
                        ScrollPanel.Controls.Remove(designerLine);
                };

                Paint += (s, e) => designerLine.BringToFront();
            }

            ResumeLayout(false);
        }
    }
}
