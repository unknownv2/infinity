using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NoDev.Infinity.Security;


namespace NoDev.Infinity
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
#if !DEBUG
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return 0;
#endif

            var argResult = ProcessArguments(args);

            if (argResult != -1)
                return argResult;

            var asmValidator = new AssemblyValidator(
                0xedaea6b2e64fad30, // Newtonsoft
                0x04de915ba3c3b77e  // DevComponents
            );

            asmValidator.HookAppDomain(AppDomain.CurrentDomain);

    
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());

            return 0;
        }

        private static int ProcessArguments(string[] args)
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return -1;

            if (args == null || args.Length == 0)
                return -1;

            if (args[0] != "ServerProxy" || args.Length != 4 || !File.Exists(args[1]))
                return -1;

            byte[] c1, c2;

            try
            {
                c1 = Convert.FromBase64String(args[2]);
                c2 = Convert.FromBase64String(args[3]);
            }
            catch
            {
                return -1;
            }

            // protect against hooking into CLR or mscorlib to return the correct hash value
            var sha256 = new InternalSHA256Managed();
            sha256.TransformBlock(c1, 0, c1.Length);
            sha256.TransformFinalBlock(c2, 0, c2.Length);
            var clientHash = sha256.Hash;
            sha256.Clear();

            var targetHash = GetInternalChallengeHash();

            if (targetHash.Length != clientHash.Length)
                return 0;

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var x = 0; x < targetHash.Length; x++)
                if (targetHash[x] != clientHash[x])
                    return 0;

            if (!NativeMethods.AttachConsole(-1))
                NativeMethods.AllocConsole();

            var result = Network.ClientValidator.ExecuteChallenge(File.ReadAllBytes(args[1]), c1, c2);

            if (result != null)
                Console.Write(Convert.ToBase64String(result));

            return 0;
        }

        private static Version _version;
        internal static Version Version
        {
            get
            {
                return _version ?? (_version = typeof(Program).Assembly.GetName().Version);
            }
        }

        private static byte[] GetInternalChallengeHash()
        {
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                return null;

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
