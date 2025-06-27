using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace NoDev.Common
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int memcmp(
            byte[] buf1, 
            byte[] buf2, 
            long count
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName, 
            FileAccess dwDesiredAccess, 
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes, 
            FileMode dwCreationDisposition, 
            uint dwFlagsAndAttributes, 
            IntPtr hTemplateFile
        );

        [SecurityCritical]
        [DllImport("mscoree.dll", PreserveSig = false, EntryPoint = "CLRCreateInstance")]
        [return: MarshalAs(UnmanagedType.Interface)]
        internal static extern object nCreateInterface(
            [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid
        );
    }
}
