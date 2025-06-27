using System;
using System.Runtime.InteropServices;
using System.Security;

namespace NoDev.Infinity
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr dc, uint reservedFlag);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool AttachConsole(int processId);

        [SecurityCritical]
        [DllImport("mscoree.dll", PreserveSig = false, EntryPoint = "CLRCreateInstance")]
        [return: MarshalAs(UnmanagedType.Interface)]
        internal static extern object nCreateInterface(
            [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid
        );
    }
}
