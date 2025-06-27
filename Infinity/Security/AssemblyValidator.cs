using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NoDev.Infinity.Security
{
    internal sealed class AssemblyValidator
    {
        private static ulong[] _frameworkTokens;
        private readonly ulong[] _tokens;

        static AssemblyValidator()
        {
            CctorProxy();
        }

        private static void CctorProxy()
        {
            _frameworkTokens = new ulong[]
            {
                0x89e03419565c7ab7, // Microsoft CLR
                0x3a0ad5117f5f3fb0, // Microsoft FX
                0x354e36ad5638bf31, // Microsoft (Unknown) (System.ServiceModel.Internals)
                0x8e79a7bed785ec7c  // Microsoft Silverlight
            };
        }

        internal AssemblyValidator(params ulong[] allowedTokens)
        {
            _tokens = allowedTokens ?? new ulong[0];
        }

        internal void HookAppDomain(AppDomain appDomain)
        {
            appDomain.AssemblyLoad += OnAssemblyLoad;

#if DEBUG
            var assemblies = appDomain.GetAssemblies();

            foreach (var assembly in assemblies.Where(assembly => !IsAssemblyValid(assembly)))
                throw new Exception("Assembly is not marked as trusted: " + assembly.FullName);
#else
            if (Debugger.IsAttached || appDomain.GetAssemblies().Any(asm => !IsAssemblyValid(asm)))
                Kill();
#endif
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
#if DEBUG
            if (!IsAssemblyValid(args.LoadedAssembly))
                Debug.Fail(string.Format("Assembly signature not validated ({0}).", args.LoadedAssembly.FullName));
#else
            if (!IsAssemblyValid(args.LoadedAssembly))
                Kill();
#endif
        }

        internal static bool ValidateSignature(byte[] assemblyImage)
        {
            return StrongNameHelper.ValidateSignature(assemblyImage);
        }

        internal static bool IsNoDevPublicKeyToken(ulong publicKeyToken)
        {
            return publicKeyToken == 0x1079dc643ca4e1c6;
        }

        internal static bool IsNoDevAssembly(string assemblyLocation)
        {
            var token = StrongNameHelper.GetPublicKeyToken(assemblyLocation);

#if DEBUG
            return IsNoDevPublicKeyToken(token);
#else
            return IsNoDevPublicKeyToken(token) && StrongNameHelper.ValidateSignature(assemblyLocation);
#endif
        }

#if !DEBUG
        private static void Kill()
        {
            Process.GetCurrentProcess().Kill();

            throw new Exception();
        }
#endif

        private bool IsAssemblyValid(Assembly asm)
        {
            try
            {
                if (asm.IsDynamic)
                    return false;

                if (asm.Location.Length == 0)
                    return false;

                var token = StrongNameHelper.GetPublicKeyToken(asm.Location);

#if DEBUG
                if (IsNoDevPublicKeyToken(token))
                    return true;
#endif

                if (!IsNoDevPublicKeyToken(token) && !_tokens.Any(t => t == token) && !_frameworkTokens.Any(t => t == token))
                    return false; 

                return StrongNameHelper.ValidateSignature(asm.Location);
            }
            catch
            {
                return false;
            }
        }
    }
}
