using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoDev.Infinity.Build.InfinityBuilder
{
    public class BuildArguments
    {
        /// <summary>
        /// The path to the Infinity solution (.sln) file.
        /// </summary>
        public string SolutionFile { get; set; }

        /// <summary>
        /// The solution configuration to use when building Infinity.
        /// Default value is "Release".
        /// </summary>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// A path to the existing ClickOnce application file.
        /// Setting this makes sure only to release assemblies that have new version numbers.
        /// </summary>
        public string ExistingApplicationFile { get; set; }

        /// <summary>
        /// The new version of the ClickOnce application.
        /// </summary>
        public Version NewApplicationVersion { get; set; }

        /// <summary>
        /// The thumbprint of the certificate to use in the X509 store.
        /// </summary>
        public string SigningCertificateThumbprint { get; set; }

        /// <summary>
        /// Timestamp URL to use when signing the assemblies and ClickOnce manifests.
        /// </summary>
        public string SigningCertificateTimestampUrl { get; set; }

        /// <summary>
        /// The directory where all temporary build files go.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The path to the DNGuard command line executable.
        /// </summary>
        public string DNGuardCmdPath { get; set; }

        /// <summary>
        /// Override the internal sn.exe path.
        /// Default is "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\sn.exe"
        /// </summary>
        public string SnExePath { get; set; }

        /// <summary>
        /// The strong name container used to sign the solution assemblies.
        /// </summary>
        public string SnContainer { get; set; }

        /// <summary>
        /// The public key token of assemblies to resign.
        /// </summary>
        public string SnPublicKeyToken { get; set; }

        /// <summary>
        /// Three base64-encoded values to generate the server challenge hash.
        /// </summary>
        public string[] ServerChallengeKeys { get; set; }

        /// <summary>
        /// Specifies whether or not null or invalid game thumbnails will throw an error.
        /// </summary>
        public bool IgnoreInvalidGameThumbnails { get; set; }

        /// <summary>
        /// The key the server will initially use to sign requests.
        /// A value of null indicates that the key will be randomly generated.
        /// </summary>
        public byte[] InitialServerKey { get; set; }

        /// <summary>
        /// The URL of the Infinity server that this build will connect to and be uploaded to.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Set to true if we are skipping the package upload.
        /// </summary>
        public bool NoUpload { get; set; }

        /// <summary>
        /// The key used to encrypt the deployment package.
        /// </summary>
        public byte[] DeploymentKey { get; set; }

        /// <summary>
        /// An ID used for remote verification of the deployment.
        /// </summary>
        public string DeploymentId { get; set; }

        internal BuildArguments()
        {
            
        }

        internal BuildArguments(IReadOnlyList<string> args)
        {
            if (args.Count < 3)
                throw new ArgumentException(Usage);

            SolutionFile = args[0];
            NewApplicationVersion = Version.Parse(args[1]);
            ServerUrl = args[2];

            if (!File.Exists(SolutionFile))
                throw new FileNotFoundException("Specified solution file does not exist.", SolutionFile);

            for (var x = 3; x < args.Count;)
            {
                if (args[x][0] != '-' || args[x][1] != '-')
                    throw new ArgumentException(Usage);

                var argKey = args[x++].Substring(2).ToLowerInvariant();
                var lastX = x;

                try
                {
                    switch (argKey)
                    {
                        case "working-dir":
                            WorkingDirectory = args[x];
                            RecreateDirectory(WorkingDirectory);
                            break;
                        case "dnguard-cmd":
                            DNGuardCmdPath = args[x];
                            if (!File.Exists(DNGuardCmdPath))
                                throw new FileNotFoundException("Specified DNGuard executable not found.",
                                    DNGuardCmdPath);
                            break;
                        case "ignore-invalid-game-thumbnails":
                            IgnoreInvalidGameThumbnails = true;
                            break;
                        case "config":
                            BuildConfiguration = args[x];
                            break;
                        case "existing-app-file":
                            ExistingApplicationFile = args[x];
                            if (!File.Exists(ExistingApplicationFile))
                                throw new FileNotFoundException("Existing application file does not exist.",
                                    ExistingApplicationFile);
                            break;
                        case "cert-thumbprint":
                            SigningCertificateThumbprint = args[x];
                            break;
                        case "cert-timestamp-url":
                            SigningCertificateTimestampUrl = args[x];
                            break;
                        case "sn-exe":
                            SnExePath = args[x];
                            break;
                        case "sn-container":
                            SnContainer = args[x];
                            break;
                        case "sn-token":
                            SnPublicKeyToken = args[x];
                            break;
                        case "challenge-keys":
                            ServerChallengeKeys = new[] {args[x], args[++x]};
                            break;
                        case "server-key":
                            InitialServerKey = Convert.FromBase64String(args[x]);
                            break;
                        case "server-url":
                            ServerUrl = args[x];
                            break;
                        case "no-upload":
                            NoUpload = true;
                            break;
                        case "deployment-id":
                            DeploymentId = args[x];
                            break;
                        case "deployment-key":
                            DeploymentKey = Convert.FromBase64String(args[x]);
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException(Usage);
                }

                if (x == lastX)
                    x++;
            }

            if (WorkingDirectory == null)
            {
                WorkingDirectory = Path.Combine(Path.GetTempPath(), "InfinityBuild");
                Directory.CreateDirectory(WorkingDirectory);
            }

            if (BuildConfiguration == null)
                BuildConfiguration = "Release";
        }

        private static void RecreateDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }

        internal static string Usage
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("Usage: build.exe <solution_file> <version> <server_url> [options...]");
                sb.AppendLine("Options:");
                sb.AppendLine("\t--working-dir <dir>\t\tSets the working directory used for temporary build files. Default: %TEMP%\\InfinityBuild");
                sb.AppendLine("\t--config <confg>\t\tSets the solution configuration. Default: Release");
                sb.AppendLine("\t--dnguard-cmd <exe_path>\t\tThe path to the DNGuard executable. Assemblies will not be protected if this isn't set.");
                sb.AppendLine("\t--ignore-invalid-game-thumbnails\t\tSuppress errors caused by non-existing or invalid game thumbnails.");
                sb.AppendLine("\t--existing-app-file <file>\tA path to the existing ClickOnce application file.");
                sb.AppendLine("\t--cert-thumbprint <thumbprint>\tThe thumbprint of the certificate to use in the X509 store.");
                sb.AppendLine("\t--cert-timestamp-url <url>\tTimestamp URL to use when signing the assemblies and ClickOnce manifests.");
                sb.AppendLine("\t--sn-exe <exe_path>\t\tThe path to the sn.exe strongname executable.");
                sb.AppendLine("\t--sn-container <name>\t\tThe container to search for the strongname token.");
                sb.AppendLine("\t--sn-token <token>\t\tThe public key token for the strongname key.");
                sb.AppendLine("\t--challenge-keys <key1> <key2>\tThe keys to use to generate the base challenge hash.");
                sb.AppendLine("\t--server-url <key1> <key2>\tThe URL of the Infinity server that this build will connect to and be uploaded to.");
                sb.AppendLine("\t--server-key <base64_encoded_key>\tThe initial 32-byte server key used to sign and verify API requests. Omit to generate a random key.");
                sb.AppendLine("\t--no-upload\t\tSkip the build upload.");
                sb.AppendLine("\t--deployment-id\t\tAn ID used for remote deployment verification.");
                sb.AppendLine("\t--deployment-key <base64_encoded_key>\t\tThe 32-byte key used to encrypt the deployment payload. Omit to disable encryption.");
                return sb.ToString();
            }
        }
    }
}
