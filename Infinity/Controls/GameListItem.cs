using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.Infinity.Controls
{
    partial class GameList
    {
        [ToolboxItem(false)]
        private class GameListItem : PanelEx
        {
            private readonly PictureBox _gameIcon;
            private readonly LabelX _gameName;

            private Color _backColor;

            internal Color BackColorHover { get; set; }
            internal Color BackColorClick { get; set; }

            //Color.FromArgb(0x3361a7);
            internal new Color BackColor
            {
                //get { return _backColor; }
                set
                {
                    if (Style.BackColor1.Color == _backColor)
                    {
                        Style.BackColor1.Color = value;
                        Style.BorderColor.Color = value;
                    }

                    _backColor = value;
                }
            }

            //private Color _tileTextColor = Color.FromArgb(0xd3, 0xe4, 0xff);

            internal string GameID { get; private set; }
            internal new event MouseEventHandler MouseClick;

            internal GameListItem(string gameId, string gameName, Image gameIcon)
            {
                GameID = gameId;

                SuspendLayout();

                _gameIcon = new PictureBox
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                    Location = new Point(8, 8),
                    Size = new Size(48, 23),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = gameIcon
                };

                Controls.Add(_gameIcon);

                _gameName = new LabelX
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                    Font = new Font("Roboto", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    Location = new Point(64, 7),
                    Size = new Size(167, 23),
                    Text = gameName,
                    WordWrap = true
                };

                _gameName.BackgroundStyle.CornerType = eCornerType.Square;

                Controls.Add(_gameName);

                Size = new Size(240, 40);
                Style.CornerType = eCornerType.Rounded;

                ResumeLayout(false);

                MouseEnter += OnMouseEnter;
                MouseLeave += OnMouseLeave;
                MouseDown += OnMouseDown;
                MouseUp += OnMouseEnter;

                foreach (Control c in Controls)
                {
                    c.MouseClick += OnMouseClick;
                    c.MouseEnter += OnMouseEnter;
                    c.MouseLeave += OnMouseLeave;
                    c.MouseDown += OnMouseDown;
                    c.MouseUp += OnMouseEnter;
                }

                base.MouseClick += OnMouseClick;
            }

            private void OnMouseClick(object sender, MouseEventArgs e)
            {
                if (MouseClick != null)
                    MouseClick(this, e);
            }

            public override string Text
            {
                get { return _gameName.Text; }
                set { _gameName.Text = value; }
            }

            public override Font Font
            {
                get { return _gameName != null ? _gameName.Font : base.Font; }
                set
                {
                    if (_gameName != null)
                        _gameName.Font = value;
                }
            }

            internal Image Image
            {
                set { _gameIcon.Image = value; }
            }

            internal Color TextColor
            {
                set { _gameName.ForeColor = value; }
            }

            private void OnMouseLeave(object sender, EventArgs e)
            {
                Style.BackColor1.Color = _backColor;
                Style.BorderColor.Color = _backColor;
            }

            private void OnMouseEnter(object sender, EventArgs e)
            {
                if (BackColorHover == Color.Empty)
                    return;

                Style.BackColor1.Color = BackColorHover;
                Style.BorderColor.Color = BackColorHover;
            }

            private void OnMouseDown(object sender, EventArgs e)
            {
                if (BackColorClick == Color.Empty)
                    return;

                Style.BackColor1.Color = BackColorClick;
                Style.BorderColor.Color = BackColorClick;
            }
        }
    }
}
