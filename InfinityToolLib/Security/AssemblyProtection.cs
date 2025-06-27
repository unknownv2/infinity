using System;
using System.Diagnostics;
using System.Reflection;

namespace NoDev.InfinityToolLib.Security
{
    internal static class AssemblyProtection
    {
        private static bool _protected;

        internal static void EnsureProtected()
        {
            if (_protected)
                return;

            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                if (entryAssembly == null)
                    throw new Exception();

                // ReSharper disable once PossibleNullReferenceException
                if (StrongNameHelper.GetPublicKeyToken(entryAssembly.Location) != 0x1079dc643ca4e1c6)
                    throw new Exception();

#if !DEBUG
                if (!StrongNameHelper.ValidateSignature(entryAssembly.Location))
                    throw new Exception();
#endif

                _protected = true;
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();

                throw new Exception();
            }
        }
    }
}
