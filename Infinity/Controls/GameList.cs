using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;

namespace NoDev.Infinity.Controls
{
    internal class SelectedGameChangedEventArgs : EventArgs
    {
        internal string GameID;

        internal SelectedGameChangedEventArgs(string gameId)
        {
            GameID = gameId;
        }
    }

    [ToolboxItem(false)]
    internal partial class GameList : PanelEx
    {
        private const int TileSidePadding = 5;
        private const int TileSpacing = 5;
        private const int SearchBoxHeight = 40;

        private readonly TextBoxX _searchBox;
        private readonly Dictionary<string, GameListItem> _tiles;

        private string _selectedGame;
        private Color _gameColor;
        private Color _gameColorSelected;
        private Color _gameColorHover;
        private Color _gameColorClick;
        private Color _textColor;

        internal event EventHandler<SelectedGameChangedEventArgs> SelectedGameChanged;

        internal new Color BackColor
        {
            get { return Style.BackColor1.Color; }
            set
            {
                CanvasColor = value;
                base.BackColor = value;
                Style.BackColor1.Color = value;
                Style.BackColor2.Color = value;

                if (_searchBox == null)
                    return;

                _searchBox.Border.BorderColor = value;
                _searchBox.Border.BorderColor2 = value;
            }
        }

        internal Color GameColor
        {
            get { return _gameColor; }
            set
            {
                _gameColor = value;
                _searchBox.BackColor = value;

                if (_gameColorSelected.IsEmpty || _gameColorSelected == CanvasColor)
                    _gameColorSelected = value;

                foreach (var tile in _tiles.Values.Where(t => t.GameID != _selectedGame))
                    tile.BackColor = value;
            }
        }

        internal Color TextColor
        {
            get { return _textColor; }
            set
            {
                _searchBox.ForeColor = value;
                _searchBox.WatermarkColor = value;
                _textColor = value;

                foreach (var tile in _tiles.Values.Where(t => t.GameID != _selectedGame))
                    tile.TextColor = value;
            }
        }

        internal Color GameColorSelected
        {
            get { return _gameColorSelected; }
            set
            {
                _gameColorSelected = value;

                if (_selectedGame == null)
                    return;

                _tiles[_selectedGame].BackColor = value;
                _tiles[_selectedGame].BackColorHover = value;
                _tiles[_selectedGame].BackColorClick = value;
            }
        }

        internal Color GameColorHover
        {
            get { return _gameColorHover; }
            set
            {
                _gameColorHover = value;

                foreach (var tile in _tiles.Values.Where(t => t.GameID != _selectedGame))
                    tile.BackColorHover = value;
            }
        }

        internal Color GameColorClick
        {
            get { return _gameColorClick; }
            set
            {
                _gameColorClick = value;

                foreach (var tile in _tiles.Values.Where(t => t.GameID != _selectedGame))
                    tile.BackColorClick = value;
            }
        }

        internal string SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                if (value == _selectedGame)
                    return;

                if (value != null && !_tiles.ContainsKey(value))
                    throw new KeyNotFoundException(string.Format("Game not found in sidebar ({0}).", value));

                if (_selectedGame != null)
                {
                    var t = _tiles[_selectedGame];
                    t.BackColor = _gameColor;
                    t.BackColorHover = _gameColorHover;
                    t.BackColorClick = _gameColorClick;
                    t.TextColor = _textColor;
                    t.Font = MakeNewFont(t, FontStyle.Regular);
                }

                _selectedGame = value;

                if (value != null)
                {
                    var t = _tiles[_selectedGame];
                    t.BackColor = _gameColorSelected;
                    t.BackColorHover = _gameColorSelected;
                    t.BackColorClick = _gameColorSelected;
                    t.TextColor = _gameColor;
                    t.Font = MakeNewFont(t, FontStyle.Bold);
                }

                OnLayout(null, null);

                if (SelectedGameChanged != null)
                    SelectedGameChanged(this, new SelectedGameChangedEventArgs(value));
            }
        }

        internal GameList()
        {
            _tiles = new Dictionary<string, GameListItem>();

            SuspendLayout();

            _searchBox = new TextBoxX
            {
                Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Point, 0),
                WatermarkFont = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Point, 0),
                WatermarkText = @"Search...",
                TextAlign = HorizontalAlignment.Center,
                Location = new Point(TileSidePadding, -26),
                Size = new Size(240, SearchBoxHeight)
            };

            Controls.Add(_searchBox);

            _searchBox.Border.CornerDiameter = 30;
            _searchBox.Border.Class = "TextBoxBorder";
            _searchBox.Border.CornerType = eCornerType.Rounded;
            _searchBox.Border.BorderColor = Color.FromArgb(0x2b, 0x57, 0x9a);
            _searchBox.Border.BorderColor2 = Color.FromArgb(0x2b, 0x57, 0x9a);
            _searchBox.Border.BorderWidth = 30;
            _searchBox.Border.MarginLeft = -27;
            _searchBox.Border.MarginRight = -27;
            _searchBox.Border.MarginTop = -4;
            _searchBox.Border.MarginBottom = -4;

            _searchBox.TextChanged += OnLayout;

            Style.Border = eBorderType.None;
            ColorSchemeStyle = eDotNetBarStyle.Office2013;
            Layout += OnLayout;

            ResumeLayout(false);
        }

        private static Font MakeNewFont(Control tile, FontStyle style)
        {
            return tile.Font.Style == style ? tile.Font : new Font(tile.Font, style);
        }

        internal void AddOrUpdateGame(string gameId, string tileText, Image tileImage = null)
        {
            if (_tiles.ContainsKey(gameId))
            {
                _tiles[gameId].Text = tileText;
                _tiles[gameId].Image = tileImage;

                return;
            }
            
            var tile = new GameListItem(gameId, tileText, tileImage)
            {
                CanvasColor = CanvasColor,
                BackColor = _gameColor,
                BackColorHover = _gameColorHover,
                BackColorClick = _gameColorClick,
                TextColor = _textColor
            };

            tile.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                var gameItem = (GameListItem)s;

                if (gameItem.GameID == SelectedGame)
                    return;

                gameItem.Focus();

                SelectedGame = gameItem.GameID;
            };

            _tiles.Add(gameId, tile);

            Controls.Add(tile);
        }

        private void OnLayout(object sender, EventArgs e)
        {
            _searchBox.BackColor = _gameColor;
            _searchBox.ForeColor = _textColor;

            var y = 0;
            const int upperMargin = SearchBoxHeight + TileSpacing;

            if (_selectedGame != null)
            {
                var t = _tiles[_selectedGame];
                t.Visible = true;
                t.Location = new Point(TileSidePadding * 2, upperMargin);
                t.Size = new Size(Width - (TileSidePadding * 2) + t.Style.CornerDiameter, t.Height);
                y = 1;
            }

            var tiles = _tiles.Values.OrderBy(t => t.Text);

            var searchText = _searchBox.Text.ToLowerInvariant();

            if (String.IsNullOrWhiteSpace(searchText))
            {
                foreach (var tile in tiles.Where(t => t.GameID != _selectedGame))
                {
                    tile.Visible = true;
                    tile.Location = new Point(TileSidePadding * 2, upperMargin + (tile.Height + TileSpacing) * y++);
                    tile.Size = new Size(Width - (TileSidePadding * 4), tile.Height);
                }
            }
            else
            {
                foreach (var tile in tiles.Where(t => t.GameID != _selectedGame))
                {
                    tile.Visible = tile.Text.ToLowerInvariant().Contains(searchText);

                    if (!tile.Visible)
                        continue;

                    tile.Location = new Point(TileSidePadding * 2, upperMargin + (tile.Height + TileSpacing) * y++);
                    tile.Size = new Size(Width - (TileSidePadding * 4), tile.Height);
                }

                if (y == 0 || (_selectedGame != null && y == 1))
                {
                    // show sad face
                }
            }

            _searchBox.SendToBack();
        }
    }
}
