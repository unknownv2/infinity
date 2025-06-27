using System.Windows.Forms;

namespace NoDev.InfinityToolLib
{
    public static class Extensions
    {
        internal static void ShowHorizontalScrollBar(this ScrollableControl control, bool show = true)
        {
            NativeMethods.ShowScrollBar(control.Handle, 0, show);
        }

        internal static void ShowVerticalScrollBar(this ScrollableControl control, bool show = true)
        {
            NativeMethods.ShowScrollBar(control.Handle, 1, show);
        }
    }
}
