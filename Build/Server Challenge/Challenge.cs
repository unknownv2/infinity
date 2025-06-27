using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using DNGuard;

[assembly: ComVisible(false)]

namespace NoDev.__Server
{
    public static class RemoteObject
    {
        public static string S1 = "WyMu`m1W(fd}b'%/:5";
        public static string S2 = "EP*|hbd`ibd";
        public static string S3 = "0vr'/30sC#4'ixci'W";

        public static byte[] CreateAuthKey(byte[] sessionId, byte[] nonce)
        {
            try
            {
                var asms = new[]
                {
                    Assembly.ReflectionOnlyLoad(Assembly.GetCallingAssembly().FullName),
                    Assembly.GetExecutingAssembly()
                };

                Challenge.Assemblies = asms;
                Challenge.AsmStreams = new Stream[asms.Length];

                var buffer = new byte[0x400];

                for (var x = 0; x < asms.Length; x++)
                {
                    Stream io;

                    if (asms[x].Location.Length != 0)
                    {
                        io = File.OpenRead(asms[x].Location);
                        io.Read(buffer, 0, buffer.Length);
                        Challenge.UpdateHash(buffer);
                    }
                    else
                    {
                        io = new MemoryStream();

                        var asmName = Challenge.GetBytes(asms[x].FullName);
                        io.Write(asmName, 0, asmName.Length);

                        foreach (var m in asms[x].Modules)
                        {
                            io.Write(m.ModuleVersionId.ToByteArray(), 0, 16);

                            var cert = m.GetSignerCertificate();

                            if (cert == null)
                                continue;

                            var certData = cert.GetRawCertData();
                            io.Write(certData, 0, certData.Length);
                        }

                        io.Position = 0;
                    }

                    Challenge.AsmStreams[x] = io;
                }

                return Challenge.CreateAuthKey(sessionId, nonce);
            }
            catch
            {
                return null;
            }
        }
    }

    internal static class Challenge
    {
        internal static Assembly[] Assemblies;
        internal static Stream[] AsmStreams;

        private static InternalSHA256Managed _hash;
        private static byte[] _asmBuffer;
        private static long _asmIndex;

        static Challenge()
        {
            CctorProxy();
        }

        private static void CctorProxy()
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return;

            try
            {
                _hash = new InternalSHA256Managed();
                _asmBuffer = new byte[AppDomain.CurrentDomain.GetAssemblies().Length * 5];
            }
            catch
            {
                //
            }
        }

        internal unsafe static byte[] GetBytes(string s)
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return new byte[1];

            var len = s.Length * 2;
            var bytes = new byte[len];
            fixed (void* p = s)
            {
                Marshal.Copy((IntPtr)p, bytes, 0, len);
            }
            return bytes;
        }

        internal static void UpdateHash(byte[] data)
        {
            if (Assembly.GetCallingAssembly() == Assembly.GetExecutingAssembly())
                _hash.TransformBlock(data, 0, data.Length);
        }

        internal static unsafe void UpdateHash(int value)
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return;

            var bytes = new byte[4];
            fixed (byte* b = bytes)
                *((int*)b) = value;

            UpdateHash(bytes);
        }

        private static void UpdateHash(string str)
        {
            if (Assembly.GetCallingAssembly() == Assembly.GetExecutingAssembly())
                UpdateHash(GetBytes(str));
        }

        [SecureMethod]
        internal static byte[] CreateAuthKey(byte[] sessionId, byte[] nonce)
        {
            if (Assembly.GetCallingAssembly() == Assembly.GetExecutingAssembly() && StackWalk())
            {
                foreach (var asm in Assemblies)
                    ProcessAssembly(asm);
            }

            foreach (var asmStream in AsmStreams)
            {
                var buffer = new byte[4096];
                while (asmStream.Read(buffer, 0, 4096) != 0)
                    UpdateHash(buffer);
                asmStream.Close();
            }

            if (sessionId == null || sessionId.Length < 16)
                return null;

            if (nonce == null || nonce.Length < 16)
                return null;

            var loadedAsms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var loadedAsm in loadedAsms)
                UpdateHash(loadedAsm.FullName);

            // a buhg in DNGuard keeps strings encrypted in new app domains, so we will hash the unknown encrypted string.
            UpdateHash(GetBytes(RemoteObject.S1));
            UpdateHash(GetBytes(RemoteObject.S2));

            var finalString = GetBytes(RemoteObject.S3);
            _hash.TransformFinalBlock(finalString, 0, finalString.Length);

            var baseHash = _hash.Hash;

            _hash.Clear();

            var sha256 = new InternalSHA256Managed();
            sha256.TransformBlock(sessionId, 0, sessionId.Length);
            sha256.TransformFinalBlock(nonce, 0, nonce.Length);
            var clientHash = sha256.Hash;
            sha256.Clear();

            var internalHash = GetInternalHash();

            if (internalHash.Length == clientHash.Length)
            {
                var flag = true;

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var x = 0; x < internalHash.Length; x++)
                {
                    if (internalHash[x] == clientHash[x])
                        continue;

                    flag = false;
                    break;
                }

                if (flag)
                    return baseHash;
            }

            sha256 = new InternalSHA256Managed();
            sha256.TransformBlock(baseHash, 0, baseHash.Length);
            sha256.TransformBlock(clientHash, 0, clientHash.Length);

            var machineId = GetMachineID();
            sha256.TransformBlock(machineId, 0, machineId.Length);

            var salt = GetFinalSalt();
            sha256.TransformFinalBlock(salt, 0, salt.Length);

            var finalHash = sha256.Hash;
            sha256.Clear();

            return finalHash;
        }

        private static byte[] GetMachineID()
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return null;

            #region Select {0} FROM {1}
            var temp = new byte[7];
            temp[16] = 0x7b;
            temp[18] = 0x7d;
            temp[1] = 0x45;
            temp[10] = 0x20;
            temp[14] = 0x4d;
            temp[5] = 0x54;
            temp[7] = 0x7b;
            temp[8] = 0x30;
            temp[17] = 0x31;
            temp[15] = 0x20;
            temp[4] = 0x43;
            temp[3] = 0x45;
            temp[12] = 0x52;
            temp[0] = 0x53;
            temp[6] = 0x20;
            temp[11] = 0x46;
            temp[2] = 0x4c;
            temp[9] = 0x7d;
            temp[13] = 0x4f;
            var formatString = Encoding.ASCII.GetString(temp);
            #endregion

            #region Machine ID Query Strings
            var machineQueryTables = new string[4][];

            machineQueryTables[0] = new string[2];

            #region SerialNumber
            temp = new byte[12];
            temp[11] = 114;
            temp[8] = 109;
            temp[0] = 83;
            temp[3] = 105;
            temp[10] = 101;
            temp[4] = 97;
            temp[6] = 78;
            temp[1] = 101;
            temp[9] = 98;
            temp[5] = 108;
            temp[7] = 117;
            temp[2] = 114;
            machineQueryTables[0][0] = Encoding.ASCII.GetString(temp);
            #endregion

            #region Win32_BIOS
            temp = new byte[10];
            temp[5] = 95;
            temp[3] = 51;
            temp[1] = 105;
            temp[9] = 83;
            temp[6] = 66;
            temp[0] = 87;
            temp[2] = 110;
            temp[7] = 73;
            temp[4] = 50;
            temp[8] = 79;

            machineQueryTables[0][1] = Encoding.ASCII.GetString(temp);
            #endregion

            machineQueryTables[1] = new string[2];

            #region Product, SerialNumber
            temp = new byte[21];
            temp[14] = 108;
            temp[15] = 78;
            temp[0] = 80;
            temp[4] = 117;
            temp[13] = 97;
            temp[17] = 109;
            temp[8] = 32;
            temp[7] = 44;
            temp[11] = 114;
            temp[1] = 114;
            temp[3] = 100;
            temp[10] = 101;
            temp[20] = 114;
            temp[12] = 105;
            temp[16] = 117;
            temp[9] = 83;
            temp[2] = 111;
            temp[6] = 116;
            temp[18] = 98;
            temp[5] = 99;
            temp[19] = 101;
            machineQueryTables[1][0] = Encoding.ASCII.GetString(temp);
            #endregion

            #region Win32_BaseBoard
            temp = new byte[15];
            temp[2] = 110;
            temp[8] = 115;
            temp[11] = 111;
            temp[7] = 97;
            temp[12] = 97;
            temp[14] = 100;
            temp[6] = 66;
            temp[4] = 50;
            temp[1] = 105;
            temp[9] = 101;
            temp[3] = 51;
            temp[0] = 87;
            temp[10] = 66;
            temp[5] = 95;
            temp[13] = 114;
            machineQueryTables[1][1] = Encoding.ASCII.GetString(temp);
            #endregion

            machineQueryTables[2] = new string[2];

            #region DeviceID, UniqueId
            temp = new byte[18];
            temp[14] = 117;
            temp[8] = 44;
            temp[16] = 73;
            temp[7] = 68;
            temp[11] = 110;
            temp[4] = 99;
            temp[0] = 68;
            temp[3] = 105;
            temp[13] = 113;
            temp[15] = 101;
            temp[9] = 32;
            temp[10] = 85;
            temp[6] = 73;
            temp[5] = 101;
            temp[1] = 101;
            temp[17] = 100;
            temp[2] = 118;
            temp[12] = 105;
            machineQueryTables[2][0] = Encoding.ASCII.GetString(temp);
            #endregion

            #region Win32_Processor
            temp = new byte[15];
            temp[9] = 99;
            temp[6] = 80;
            temp[2] = 110;
            temp[14] = 114;
            temp[10] = 101;
            temp[8] = 111;
            temp[1] = 105;
            temp[13] = 111;
            temp[12] = 115;
            temp[3] = 51;
            temp[0] = 87;
            temp[4] = 50;
            temp[11] = 115;
            temp[5] = 95;
            temp[7] = 114;
            machineQueryTables[2][1] = Encoding.ASCII.GetString(temp);
            #endregion

            machineQueryTables[3] = new string[2];

            #region DeviceID
            temp = new byte[8];
            temp[2] = 118;
            temp[7] = 68;
            temp[0] = 68;
            temp[1] = 101;
            temp[5] = 101;
            temp[6] = 73;
            temp[4] = 99;
            temp[3] = 105;
            machineQueryTables[3][0] = Encoding.ASCII.GetString(temp);
            #endregion

            #region Win32_MotherboardDevice
            temp = new byte[23];
            temp[5] = 95;
            temp[15] = 114;
            temp[11] = 114;
            temp[17] = 68;
            temp[10] = 101;
            temp[2] = 110;
            temp[3] = 51;
            temp[21] = 99;
            temp[13] = 111;
            temp[18] = 101;
            temp[4] = 50;
            temp[0] = 87;
            temp[1] = 105;
            temp[16] = 100;
            temp[20] = 105;
            temp[6] = 77;
            temp[7] = 111;
            temp[22] = 101;
            temp[19] = 118;
            temp[8] = 116;
            temp[9] = 104;
            temp[12] = 98;
            temp[14] = 97;
            machineQueryTables[3][1] = Encoding.ASCII.GetString(temp);
            #endregion

            #endregion

            var sha = new InternalSHA256Managed();

            foreach (var strHash in machineQueryTables.Cast<object[]>().Select(table => new ManagementObjectSearcher(string.Format(formatString, table)).Get()).SelectMany(objs => objs.Cast<ManagementObject>().SelectMany(obj => obj.Properties.Cast<PropertyData>().Where(data => data.Value != null))).Select(data => GetBytes(data.Value.ToString() + ':')))
                sha.TransformBlock(strHash, 0, strHash.Length);

            var procCount = BitConverter.GetBytes(Environment.ProcessorCount);
            sha.TransformFinalBlock(procCount, 0, 4);

            var machineId = sha.Hash;

            sha.Clear();

            return machineId;
        }

        private static void ProcessAssembly(Assembly asm)
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly() || !StackWalk())
                return;

            var types = asm.GetTypes();

            foreach (var type in types.Where(t => t.IsClass))
            {
                UpdateHash(type.FullName);
                UpdateHash(new byte[] { 0 });

                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
                    .Concat(type.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public))
                    .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic))
                    .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)).ToArray();

                foreach (var method in methods)
                {
                    if (AsmStreams[_asmIndex++ % AsmStreams.Length].Read(_asmBuffer, 0, _asmBuffer.Length) != 0)
                        UpdateHash(_asmBuffer);

                    UpdateHash(method.Name);

                    var methodBody = method.GetMethodBody();

                    if (methodBody != null)
                        UpdateHash(methodBody.GetILAsByteArray());
                }
            }
        }

        [SecureCallbackMethod]
        private static unsafe bool StackWalk()
        {
            var frames = new StackTrace().GetFrames();

            if (frames == null)
                return false;

            foreach (var frame in frames)
            {
                if (AsmStreams[_asmIndex++ % AsmStreams.Length].Read(_asmBuffer, 0, _asmBuffer.Length) != 0)
                    UpdateHash(_asmBuffer);

                var declaringType = frame.GetMethod().DeclaringType;

                if (declaringType == null)
                    continue;

                var tokenArr = declaringType.Assembly.GetName().GetPublicKeyToken();

                if (tokenArr == null || tokenArr.Length == 0)
                    continue;

                ulong token;

                fixed (byte* pbyte = tokenArr)
                    token = *((ulong*) pbyte);

                if (token != 0x1079dc643ca4e1c6)
                    continue;

                UpdateHash(declaringType.FullName);
                UpdateHash(new byte[] { 0 });

                var method = frame.GetMethod();
                UpdateHash(method.Name);

                var methodBody = method.GetMethodBody();

                if (methodBody != null)
                    UpdateHash(methodBody.GetILAsByteArray());

                var ilOffset = frame.GetILOffset();
                var nativeOffset = frame.GetNativeOffset();
                var combined = ilOffset + nativeOffset;
                
                if (combined >= 0x04)
                    _asmBuffer = new byte[combined % 20];

                UpdateHash(ilOffset);
                UpdateHash(nativeOffset);
            }

            return true;
        }

        private static byte[] GetFinalSalt()
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return new byte[0];

            var arr = new byte[16];
            arr[5] = 0x5b;
            arr[12] = 0x81;
            arr[4] = 0x78;
            arr[14] = 0x18;
            arr[3] = 0xa2;
            arr[8] = 0x1a;
            arr[11] = 0x72;
            arr[0] = 0x88;
            arr[1] = 0x29;
            arr[7] = 0x9f;
            arr[9] = 0x36;
            arr[15] = 0x14;
            arr[13] = 0x5c;
            arr[10] = 0x77;
            arr[2] = 0x00;
            arr[6] = 0x20;
            return arr;
        }

        private static byte[] GetInternalHash()
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return new byte[0];

            var arr = new byte[32];
            arr[31] = 0x6d;
            arr[11] = 0xa2;
            arr[25] = 0x8c;
            arr[15] = 0x09;
            arr[7] = 0x46;
            arr[13] = 0x0a;
            arr[19] = 0xf4;
            arr[5] = 0x42;
            arr[16] = 0x14;
            arr[8] = 0x1e;
            arr[12] = 0x18;
            arr[20] = 0x1c;
            arr[21] = 0x32;
            arr[17] = 0xa4;
            arr[22] = 0x90;
            arr[2] = 0xcb;
            arr[10] = 0xd7;
            arr[9] = 0xf5;
            arr[23] = 0xfe;
            arr[27] = 0xf6;
            arr[24] = 0x22;
            arr[29] = 0xd9;
            arr[4] = 0x6a;
            arr[6] = 0xec;
            arr[28] = 0xf5;
            arr[3] = 0xd0;
            arr[18] = 0x90;
            arr[14] = 0xe6;
            arr[1] = 0xb3;
            arr[26] = 0x6a;
            arr[30] = 0x7b;
            arr[0] = 0x62;
            return arr;
        }
    }
}

namespace DNGuard
{
    internal sealed class SecureMethodAttribute : Attribute
    {

    }

    internal sealed class SecureCallbackMethodAttribute : Attribute
    {

    }
}
