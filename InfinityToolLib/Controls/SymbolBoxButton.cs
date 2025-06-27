using System;
using System.Drawing;
using DevComponents.DotNetBar.Controls;

namespace NoDev.InfinityToolLib.Controls
{
    public sealed class SymbolBoxButton : SymbolBox
    {
        private Color _symbolColor;

        public new Color SymbolColor
        {
            get { return _symbolColor; }
            set
            {
                if (ForeColor == _symbolColor && Enabled)
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
        public Color SymbolColorDisabled { get; set; }

        public SymbolBoxButton(string symbol = null)
        {
            if (symbol != null)
                Symbol = symbol;

            _symbolColor = ForeColor;

            SymbolColorDisabled = _symbolColor;

            GotFocus += OnFocus;
            MouseEnter += OnFocus;

            LostFocus += OnLostFocus;
            MouseLeave += OnLostFocus;

            MouseDown += OnMouseDown;
            MouseUp += OnLostFocus;

            EnabledChanged += OnEnabledChanged;
        }

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            ForeColor = Enabled ? SymbolColor : SymbolColorDisabled;
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Enabled)
                ForeColor = SymbolColorClick;
        }

        private void OnFocus(object sender, EventArgs e)
        {
            if (Enabled)
                ForeColor = SymbolColorHover;
        }

        private void OnLostFocus(object sender, EventArgs e)
        {
            if (Enabled)
                ForeColor = SymbolColor;
        }
    }
}
