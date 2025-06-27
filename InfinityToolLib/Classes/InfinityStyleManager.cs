using System;
using System.Drawing;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro.ColorTables;

namespace NoDev.InfinityToolLib
{
    public static class InfinityStyleManager
    {
        private static readonly Color CanvasColor = Color.White;

        public static event EventHandler<StyleColorsChangedEventArgs> ColorsChanged;

        private static InfinityStyleColors _colors;

        public static InfinityStyleColors Colors
        {
            get
            {
                if (_colors.Equals(default(InfinityStyleColors)))
                {
                    StyleManager.Style = eStyle.OfficeMobile2014;

                    _colors = new InfinityStyleColors
                    {
                        Lightest = Color.FromArgb(108, 142, 194),
                        Lighter = Color.FromArgb(68, 108, 167),
                        BaseColor = Color.FromArgb(43, 87, 154),
                        Darker = Color.FromArgb(22, 66, 130),
                        Darkest = Color.FromArgb(10, 48, 103)
                    };
                }
                    
                return _colors;

            }
            set
            {
                _colors = value;

                StyleManager.MetroColorGeneratorParameters = new MetroColorGeneratorParameters(CanvasColor, _colors.BaseColor);
                
                if (ColorsChanged != null)
                    ColorsChanged(null, new StyleColorsChangedEventArgs(_colors));
            }
        }

        public static void LoadDefaultStyle()
        {
            StyleManager.Style = eStyle.OfficeMobile2014;
        }
    }

    public struct InfinityStyleColors : ICloneable
    {
        public Color Lightest { get; set; }
        public Color Lighter { get; set; }
        public Color BaseColor { get; set; }
        public Color Darker { get; set; }
        public Color Darkest { get; set; }

        public object Clone()
        {
            return new InfinityStyleColors
            {
                Lightest = Lightest,
                Lighter = Lighter,
                BaseColor = BaseColor,
                Darker = Darker,
                Darkest = Darkest
            };
        }
    }

    public class StyleColorsChangedEventArgs : EventArgs
    {
        public InfinityStyleColors Colors { get; private set; }

        internal StyleColorsChangedEventArgs(InfinityStyleColors colors)
        {
            Colors = colors;
        }
    }
}
