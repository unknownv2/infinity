using System;
using System.Drawing;
using DevComponents.DotNetBar;

namespace NoDev.InfinityToolLib.Controls
{
    public class SymbolButtonItem : LabelItem
    {
        private Color _symbolColor;

        public new Color SymbolColor
        {
            get { return _symbolColor; }
            set
            {
                if (ForeColor == _symbolColor)
                    ForeColor = value;

                if (SymbolColorHover == Color.Empty)
                    SymbolColorHover = value;

                if (SymbolColorClick == Color.Empty)
                    SymbolColorClick = value;

                _symbolColor = value;
            }
        }

        public Color SymbolColorHover { get; set; }
        public Color SymbolColorClick { get; set; }

        public override string Text
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        public SymbolButtonItem(string symbol = null)
        {
            if (symbol != null)
                Symbol = symbol;

            ImagePosition = eImagePosition.Top;
            TextAlignment = StringAlignment.Center;
            ImageTextSpacing = -7;

            _symbolColor = ForeColor;

            GotFocus += OnFocus;
            MouseEnter += OnFocus;

            LostFocus += OnLostFocus;
            MouseLeave += OnLostFocus;

            MouseDown += OnMouseDown;
            MouseUp += OnLostFocus;
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ForeColor = SymbolColorClick;
        }

        private void OnFocus(object sender, EventArgs e)
        {
            ForeColor = SymbolColorHover;
        }

        private void OnLostFocus(object sender, EventArgs e)
        {
            ForeColor = SymbolColor;
        }
    }
}
