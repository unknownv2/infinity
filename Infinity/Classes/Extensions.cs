using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NoDev.Infinity
{
    internal static class Extensions
    {
        internal static bool IsTypeOf<T>(this Type type)
        {
            var baseType = typeof(T);

            while (type.BaseType != null)
            {
                if (type.BaseType == baseType)
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        internal static Bitmap ToBitmap(this Control control)
        {
            var hwnd = control.Handle;
            var bitmap = new Bitmap(control.Size.Width, control.Size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var bitmapHandle = g.GetHdc();
                NativeMethods.PrintWindow(hwnd, bitmapHandle, 0);
                g.ReleaseHdc(bitmapHandle);
            }
            return bitmap;
        }

        internal static Image ToImage(this byte[] buffer)
        {
            try
            {
                var ms = new MemoryStream(buffer);
                var img = Image.FromStream(ms);
                ms.Close();
                return img;
            }
            catch
            {
                return null;
            }
        }
    }
}
