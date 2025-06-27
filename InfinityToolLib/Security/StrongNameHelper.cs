using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NoDev.InfinityToolLib.Security
{
    internal static class StrongNameHelper
    {
        private static Guid _metaHostClsIdGuid;
        private static Guid _metaHostGuid;
        private static Guid _clsIdInterfaceGuid;
        private static ICLRStrongName _strongName;

        private static void Initialize()
        {
            if (_strongName != null)
                return;

            _metaHostClsIdGuid = new Guid(0x9280188D, 0xE8E, 0x4867, 0xB3, 0xC, 0x7F, 0xA8, 0x38, 0x84, 0xE8, 0xDE);
            _metaHostGuid = new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);
            _clsIdInterfaceGuid = new Guid(0xB79B0ACD, 0xF5CD, 0x409b, 0xB5, 0xA5, 0xA1, 0x62, 0x44, 0x61, 0x0B, 0x92);

            var metaHost = (IClrMetaHost)NativeMethods.nCreateInterface(_metaHostClsIdGuid, _metaHostGuid);
            var runtimeInfo = (IClrRuntimeInfo)metaHost.GetRuntime(RuntimeEnvironment.GetSystemVersion(), typeof(IClrRuntimeInfo).GUID);
            _strongName = (ICLRStrongName)runtimeInfo.GetInterface(_clsIdInterfaceGuid, typeof(ICLRStrongName).GUID);
        }

        internal static ulong GetPublicKeyToken(string assemblyPath)
        {
            try
            {
                Initialize();

                int pTokenInt;
                IntPtr pToken;
                _strongName.StrongNameTokenFromAssembly(assemblyPath, out pToken, out pTokenInt);

                return (ulong)Marshal.ReadInt64(pToken);
            }
            catch
            {
                return 0;
            }
        }

        internal static bool ValidateSignature(string assemblyPath)
        {
            try
            {
                Initialize();

                bool pfWasVerified;
                return _strongName.StrongNameSignatureVerificationEx(assemblyPath, true, out pfWasVerified) == 0 && pfWasVerified;
            }
            catch
            {
                return false;
            }
        }

        internal static unsafe bool ValidateSignature(byte[] assemblyImage)
        {
            try
            {
                Initialize();

                fixed (byte* p = assemblyImage)
                {
                    var ptr = (IntPtr)p;
                    int pfWasVerified;

                    var sigVer = _strongName.StrongNameSignatureVerificationFromImage(ptr, assemblyImage.Length, 0x03, out pfWasVerified);

                    return sigVer == 0 && pfWasVerified == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        [SecurityCritical]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
        public interface IClrMetaHost
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            object GetRuntime(
                [In, MarshalAs(UnmanagedType.LPWStr)] string version,
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig]
            int GetVersionFromFile(
                [In, MarshalAs(UnmanagedType.LPWStr)] string filePath,
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);

            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            IEnumUnknown EnumerateInstalledRuntimes();

            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            IEnumUnknown EnumerateLoadedRuntimes([In] IntPtr processHandle);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Reserved01([In] IntPtr reserved1);

            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            object QueryLegacyV2RuntimeBinding([In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void ExitProcess([In, MarshalAs(UnmanagedType.U4)] int exitCode);
        }

        [SecurityCritical]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000100-0000-0000-C000-000000000046")]
        public interface IEnumUnknown
        {
            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Next(
                [In, MarshalAs(UnmanagedType.U4)] int elementArrayLength,
                [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] object[] elementArray,
                [MarshalAs(UnmanagedType.U4)] out int fetchedElementCount);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int count);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Reset();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumUnknown enumerator);
        }

        [SecurityCritical]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
        public interface IClrRuntimeInfo
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig]
            int GetVersionString(
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] StringBuilder buffer,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig]
            int GetRuntimeDirectory(
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] StringBuilder buffer,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);

            [return: MarshalAs(UnmanagedType.Bool)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            bool IsLoaded(
                [In] IntPtr processHandle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), LCIDConversion(3)]
            [PreserveSig]
            int LoadErrorString(
                [In, MarshalAs(UnmanagedType.U4)] int resourceId,
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder buffer,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            IntPtr LoadLibrary(
                [In, MarshalAs(UnmanagedType.LPWStr)] string dllName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            IntPtr GetProcAddress(
                [In, MarshalAs(UnmanagedType.LPStr)] string procName);

            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            object GetInterface(
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid coClassId,
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);

            [return: MarshalAs(UnmanagedType.Bool)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            bool IsLoadable();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultStartupFlags(
                [In, MarshalAs(UnmanagedType.U4)] uint startupFlags,
                [In, MarshalAs(UnmanagedType.LPStr)] string hostConfigFile);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig]
            int GetDefaultStartupFlags(
                [Out, MarshalAs(UnmanagedType.U4)] out uint startupFlags,
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder hostConfigFile,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int hostConfigFileLength);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void BindAsLegacyV2Runtime();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void IsStarted(
                [Out, MarshalAs(UnmanagedType.Bool)] out bool started,
                [Out, MarshalAs(UnmanagedType.U4)] out uint startupFlags);
        }

        [SecurityCritical]
        [ComImport, ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
        public interface ICLRStrongName
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromAssemblyFile(
                [In, MarshalAs(UnmanagedType.LPStr)] string pszFilePath,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromAssemblyFileW(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromBlob(
                [In] IntPtr pbBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cchBlob,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromFile(
                [In, MarshalAs(UnmanagedType.LPStr)] string pszFilePath,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromFileW(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetHashFromHandle(
                [In] IntPtr hFile,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash,
                [In, MarshalAs(UnmanagedType.U4)] int cchHash,
                [MarshalAs(UnmanagedType.U4)] out int pchHash);

            [return: MarshalAs(UnmanagedType.U4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int StrongNameCompareAssemblies(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly1,
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly2);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameFreeBuffer(
                [In] IntPtr pbMemory);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameGetBlob(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbBlob,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameGetBlobFromImage(
                [In] IntPtr pbBase,
                [In, MarshalAs(UnmanagedType.U4)] int dwLength,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbBlob,
                [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameGetPublicKey(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In] IntPtr pbKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob,
                out IntPtr ppbPublicKeyBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

            [return: MarshalAs(UnmanagedType.U4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int StrongNameHashSize(
                [In, MarshalAs(UnmanagedType.U4)] int ulHashAlg);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameKeyDelete(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameKeyGen(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In, MarshalAs(UnmanagedType.U4)] int dwFlags,
                out IntPtr ppbKeyBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameKeyGenEx(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In, MarshalAs(UnmanagedType.U4)] int dwFlags,
                [In, MarshalAs(UnmanagedType.U4)] int dwKeySize,
                out IntPtr ppbKeyBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameKeyInstall(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In] IntPtr pbKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameSignatureGeneration(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In] IntPtr pbKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob,
                out IntPtr ppbSignatureBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameSignatureGenerationEx(
                [In, MarshalAs(UnmanagedType.LPWStr)] string wszFilePath,
                [In, MarshalAs(UnmanagedType.LPWStr)] string wszKeyContainer,
                [In] IntPtr pbKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbKeyBlob,
                out IntPtr ppbSignatureBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob,
                [In, MarshalAs(UnmanagedType.U4)] int dwFlags);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameSignatureSize(
                [In] IntPtr pbPublicKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbSize);

            [return: MarshalAs(UnmanagedType.U4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int StrongNameSignatureVerification(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [In, MarshalAs(UnmanagedType.U4)] int dwInFlags,
                [MarshalAs(UnmanagedType.U4)] out int pdwOutFlags);

            [return: MarshalAs(UnmanagedType.I1)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            byte StrongNameSignatureVerificationEx(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [In, MarshalAs(UnmanagedType.Bool)] bool fForceVerification,
                [MarshalAs(UnmanagedType.Bool)] out bool pfWasVerified);

            [return: MarshalAs(UnmanagedType.U4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int StrongNameSignatureVerificationFromImage(
                [In] IntPtr pbBase,
                [In, MarshalAs(UnmanagedType.U4)] int dwLength,
                [In, MarshalAs(UnmanagedType.U4)] int dwInFlags,
                [MarshalAs(UnmanagedType.U4)] out int pdwOutFlags);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameTokenFromAssembly(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                out IntPtr ppbStrongNameToken,
                [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameTokenFromAssemblyEx(
                [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                out IntPtr ppbStrongNameToken,
                [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken,
                out IntPtr ppbPublicKeyBlob,
                [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StrongNameTokenFromPublicKey(
                [In] IntPtr pbPublicKeyBlob,
                [In, MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob,
                out IntPtr ppbStrongNameToken,
                [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
        }
    }
}
