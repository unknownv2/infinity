using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NoDev.InfinityToolLib.Controls;
using DevComponents.DotNetBar;

namespace NoDev.InfinityToolLib.Panels
{
    public class SideMenuInfinityPanel : ScrollableInfinityPanel
    {
        private const int SideMenuWidth = 35;
        private const int SideMenuItemSpacing = 5;
        private const int SideMenuItemSidePadding = 5;
        private const int SideMenuSymbolSize = SideMenuWidth - SideMenuItemSidePadding * 2;

        private System.Windows.Forms.ToolTip _toolTip;
        private List<SymbolBoxButton> _sideMenuButtons;

        protected SideMenuInfinityPanel()
        {
            Layout += OnLayout;
        }

        private void OnLayout(object sender, LayoutEventArgs e)
        {
            if (_sideMenuButtons == null)
                return;

            var y = 0;

            foreach (var button in _sideMenuButtons.Where(b => b.Visible))
            {
                button.Location = new Point(
                    SideMenuItemSidePadding + 1, 
                    y++ * (SideMenuSymbolSize + SideMenuItemSpacing) + SideMenuItemSpacing
                );
            }
        }

        protected SymbolBoxButton AddSideMenuButton(string symbol, string tooltip = null)
        {
            if (_sideMenuButtons == null)
            {
                _sideMenuButtons = new List<SymbolBoxButton>(1);

                ScrollPanel.Dock = DockStyle.Right;
                ScrollPanel.Width -= SideMenuWidth;
                ScrollPanel.Location = new Point(SideMenuWidth, 0);
                ScrollPanel.Style.Border = eBorderType.SingleLine;
                ScrollPanel.Style.BorderColor.Color = InfinityStyleManager.Colors.BaseColor;
                ScrollPanel.Style.BorderSide = eBorderSide.Left;
            }

            var symbolBox = new SymbolBoxButton(symbol)
            {
                SymbolColor = InfinityStyleManager.Colors.BaseColor,
                SymbolColorHover = InfinityStyleManager.Colors.Darkest,
                SymbolColorClick = InfinityStyleManager.Colors.Lightest,
                SymbolColorDisabled = Color.LightGray,
                Size = new Size(SideMenuSymbolSize, SideMenuSymbolSize)
            };

            if (!string.IsNullOrEmpty(tooltip))
            {
                if (_toolTip == null)
                {
                    _toolTip = new System.Windows.Forms.ToolTip
                    {
                        AutoPopDelay = 5000,
                        InitialDelay = 750,
                        ReshowDelay = 500
                    };
                }

                _toolTip.SetToolTip(symbolBox, tooltip);
            }

            _sideMenuButtons.Add(symbolBox);

            Controls.Add(symbolBox);

            return symbolBox;
        }

        protected override void OnStyleColorsChanged(InfinityStyleColors colors)
        {
            foreach (var c in _sideMenuButtons)
            {
                c.SymbolColor = colors.BaseColor;
                c.SymbolColorHover = colors.Darkest;
                c.SymbolColorClick = colors.Lighter;
            }

            base.OnStyleColorsChanged(colors);
        }
    }
}
