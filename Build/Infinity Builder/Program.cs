using System;
using System.IO;

namespace NoDev.Infinity.Build.InfinityBuilder
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var success = false;

            try
            {
#if DEBUG
                if (args == null || args.Length == 0)
                    Build(GetDefaultArguments());
                else
#endif
                Build(new BuildArguments(args));

                Console.WriteLine("\r\nBuild complete.");

                success = true;
            }
            catch (Exception e)
            {
                Console.Write("\r\nException Thrown: " + e.Message);
            }

#if DEBUG
            Console.In.ReadToEnd();
            Console.ReadKey();
#endif

            return success ? 0 : 1;
        }

        private static void Build(BuildArguments args)
        {
            var buildProcessor = new Builder(args);

            buildProcessor.Build();
        }

#if DEBUG
        private static BuildArguments GetDefaultArguments()
        {
            var workingDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Build";

            var dngCmd = @"C:\Program Files (x86)\DNGuard Enterprise\DNGuardCMD.exe";
            if (!File.Exists(dngCmd))
                dngCmd = null;

            return new BuildArguments
            {
                SolutionFile = Path.GetFullPath(Environment.CurrentDirectory + @"\..\..\..\..\Infinity.sln"),
                WorkingDirectory = workingDir,
                SigningCertificateThumbprint = "4bf5a25ddb35f9092566148fbb935d69172b3b59",
                SigningCertificateTimestampUrl = null,
                ExistingApplicationFile = null,
                NewApplicationVersion = Version.Parse("1.0.0.0"),
                IgnoreInvalidGameThumbnails = true,
                BuildConfiguration = "Release",
                DNGuardCmdPath = dngCmd,
                SnContainer = "nodev",
                SnPublicKeyToken = "C6E1A43C64DC7910",
                ServerChallengeKeys = null //removed
            };
        }
#endif
    }
}
