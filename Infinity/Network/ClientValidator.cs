using NoDev.Infinity.Network.Api;
using NoDev.Infinity.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoDev.Infinity.Network
{
    internal static class ClientValidator
    {
        internal static async Task ValidateAsync()
        {
            var req = ApiRequestFactory.CreateUnvalidated(Method.POST, "/challenge");

            var response = await req.SendUrlEncodedFormAsync(new Dictionary<string, string>
            {
                { "machine_id", Convert.ToBase64String(GetMachineID()) }
            });

            var authKey = ExecuteChallenge(
                await response.ToByteArrayAsync(),
                Encoding.UTF8.GetBytes(req.State.SessionId),
                Convert.FromBase64String(response.Headers["X-Inf-Nonce"])
            );

            if (authKey != null)
            {
                req.State.AuthKey = authKey;
            }
        }

        internal static byte[] ExecuteChallenge(byte[] assemblyBytes, byte[] sessionId, byte[] nonce)
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return null;

            if (!AssemblyValidator.ValidateSignature(assemblyBytes))
                return null;

            byte[] result;

            var appDomain = AppDomain.CreateDomain(Rand.Next().ToString());

            try
            {
                var mediatorType = typeof(ChallengeMediator);
                var mediator = (ChallengeMediator)appDomain.CreateInstanceAndUnwrap(mediatorType.Assembly.FullName, mediatorType.FullName);
                result = (byte[])mediator.ExecuteChallenge(assemblyBytes, new object[] { sessionId, nonce });
            }
            catch
            {
                result = null;
            }

            AppDomain.Unload(appDomain);

            return result;
        }

        private sealed class ChallengeMediator : MarshalByRefObject
        {
            public ChallengeMediator()
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, e) => Assembly.ReflectionOnlyLoad(e.Name);
            }

            // ReSharper disable once MemberCanBeMadeStatic.Local
            internal object ExecuteChallenge(byte[] assemblyBytes, object[] args)
            {
                var callingAsm = Assembly.GetCallingAssembly();
                var executingAsm = Assembly.GetExecutingAssembly();

                // separate instances because they are from different app domains
                if (callingAsm == executingAsm)
                    return null;

                if (callingAsm.Location != executingAsm.Location || callingAsm.FullName != executingAsm.FullName)
                    return null;

                try
                {
                    var challengeAssembly = Assembly.Load(assemblyBytes);

                    var token = BitConverter.ToUInt64(challengeAssembly.GetName().GetPublicKeyToken(), 0);
                    if (!AssemblyValidator.IsNoDevPublicKeyToken(token))
                        return null;

                    var challengeClass = challengeAssembly.GetExportedTypes().FirstOrDefault(t => t.IsClass && t.IsSealed && t.IsAbstract);

                    if (challengeClass == null)
                        return null;

                    var method = challengeClass
                        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .FirstOrDefault();

                    return method == null ? null : method.Invoke(null, args);
                }
                catch
                {
                    return null;
                }
            }
        }

        internal static byte[] GetMachineID()
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

        private unsafe static byte[] GetBytes(string s)
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
    }
}
