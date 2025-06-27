using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;

namespace NoDev.Infinity.Build.InfinityBuilder.Deployment
{
    internal class ClickOnceBuilder
    {
        private const string InfinityExe = "Infinity.exe";
        private const string ApplicationFileName = "Infinity.application";

        private static readonly HashAlgorithm Hasher = SHA256.Create();

        private readonly string _inputDir;
        private readonly FileVersionInfo _infinityVersionInfo;

        // codebase/target => file version
        private readonly IDictionary<string, Tuple<string, AssemblyReference>> _existingApplicationAssemblies;
        private readonly string _existingApplicationDir;

        internal string DeploymentUrl { get; set; }
        internal Version Version { get; set; }
        internal X509Certificate2 Certificate { get; set; }
        internal Uri CertificateTimestampUri { get; set; }
        internal IDictionary<string, string> FileNameToGroupName { get; set; } 

        internal ClickOnceBuilder(string inputDir, string existingApplicationFile = null)
        {
            var infinityExe = Path.Combine(inputDir, InfinityExe);

            _inputDir = inputDir;
            _infinityVersionInfo = FileVersionInfo.GetVersionInfo(infinityExe);

            if (existingApplicationFile == null) 
                return;

            var deployManifest = (DeployManifest)ManifestReader.ReadManifest("DeployManifest", existingApplicationFile, false);

            if (deployManifest == null)
                throw new Exception(string.Format("Existing deploy manifest is not valid ({0}).", existingApplicationFile));

            DeploymentUrl = deployManifest.DeploymentUrl;

            var deployDir = Path.GetDirectoryName(existingApplicationFile);
            
            if (deployDir == null)
                throw new Exception(string.Format("Invalid deployment application file ({0}).", existingApplicationFile));

            var appManifestPath = Path.Combine(deployDir, deployManifest.AssemblyReferences[0].TargetPath);

            var appManifest = (ApplicationManifest)ManifestReader.ReadManifest("ApplicationManifest", appManifestPath, false);

            if (appManifest == null)
                throw new Exception(string.Format("Existing application manifest path is not valid ({0}).", appManifestPath));

            _existingApplicationDir = Path.GetDirectoryName(appManifestPath);

            if (_existingApplicationDir == null)
                throw new Exception(string.Format("Existing application manifest path is not valid ({0}).", appManifestPath));

            _existingApplicationAssemblies = new Dictionary<string, Tuple<string, AssemblyReference>>(appManifest.AssemblyReferences.Count);
            foreach (AssemblyReference asmRef in appManifest.AssemblyReferences)
            {
                _existingApplicationAssemblies.Add(
                    asmRef.TargetPath,
                    new Tuple<string, AssemblyReference>(
                        FileVersionInfo.GetVersionInfo(Path.Combine(_existingApplicationDir, asmRef.TargetPath)).FileVersion,
                        asmRef
                ));
            }
        }

        internal void Build(string outDirectory)
        {
            var relativeAppDir = @"appfiles\" + Version;
            var appDir = Path.Combine(outDirectory, relativeAppDir);

            if (Directory.Exists(outDirectory))
            {
                Directory.Delete(outDirectory);
                while(Directory.Exists(outDirectory))
                    Thread.Sleep(10);
            }

            Directory.CreateDirectory(appDir);
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(_inputDir, appDir, true);

            var outDir = new DirectoryInfo(outDirectory);
            foreach (var file in outDir.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => f.Extension == ".deploy"))
                file.MoveTo(file.FullName.Remove(file.FullName.Length - ".deploy".Length));

            var relativeAppManifestFile = Path.Combine(relativeAppDir, InfinityExe + ".manifest");

            var appManifestFile = Path.Combine(outDirectory, relativeAppManifestFile);
            CreateApplicationManifest(appDir, InfinityExe, appManifestFile);

            if (Certificate != null)
                SecurityUtilities.SignFile(Certificate, CertificateTimestampUri, appManifestFile);

            var deployManifestFile = Path.Combine(outDirectory, ApplicationFileName);
            CreateDeployManifest(deployManifestFile, relativeAppManifestFile);

            if (Certificate != null)
                SecurityUtilities.SignFile(Certificate, CertificateTimestampUri, deployManifestFile);

            var bootstrapResult = BuildBootstrapper(outDirectory, DeploymentUrl);

            if (bootstrapResult.Messages != null)
                foreach (var message in bootstrapResult.Messages)
                    Console.WriteLine("\r\nBootstrapper: " + message.Message);

            if (!bootstrapResult.Succeeded)
                throw new Exception("Failed to create ClickOnce bootstrapper.");

            if (Certificate != null)
                SecurityUtilities.SignFile(Certificate, CertificateTimestampUri, bootstrapResult.KeyFile);
        }

        private BuildResults BuildBootstrapper(string outDir, string remoteDirectory)
        {
            var settings = new BuildSettings
            {
                ApplicationName = _infinityVersionInfo.ProductName,
                ApplicationRequiresElevation = false,
                ApplicationUrl = remoteDirectory,
                ApplicationFile = ApplicationFileName,
                ComponentsLocation = ComponentsLocation.HomeSite,
                CopyComponents = true,
                Validate = true,
                OutputPath = outDir
            };

            var boot = new BootstrapperBuilder
            {
                Path = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\Bootstrapper"
            };

            settings.ProductBuilders.Add(boot.Products.Product(".NETFramework,Version=v4.5").ProductBuilder);

            return boot.Build(settings);
        }

        private void CreateDeployManifest(string outFile, string relativeAppManifestFile)
        {
            var manifest = new DeployManifest
            {
                // assemblyIdentity node
                AssemblyIdentity = new AssemblyIdentity
                {
                    Name = ApplicationFileName,
                    Version = Version.ToString(),
                    Culture = "en",//_infinityVersionInfo.Language,
                    ProcessorArchitecture = "msil"
                },
                
                // description node
                Product = _infinityVersionInfo.ProductName,
                Publisher = _infinityVersionInfo.CompanyName,

                // deployment node
                Install = true,
                MapFileExtensions = true,
                CreateDesktopShortcut = true,
                UpdateEnabled = true,
                UpdateMode = UpdateMode.Background,
                DeploymentUrl = DeploymentUrl,

                // compatibleFrameworks node
                XmlCompatibleFrameworks = new[]
                {
                    new CompatibleFramework
                    {
                        Version = "4.5",
                        Profile = "Full",
                        SupportedRuntime = "4.0.30.319"
                    }
                }
            };

            var outDir = Path.GetDirectoryName(outFile);

            if (outDir == null)
                throw new Exception("Out file path is not valid: " + outFile);

            var appManifestFile = Path.Combine(outDir, relativeAppManifestFile);

            var manifestStream = File.OpenRead(appManifestFile);
            manifest.AssemblyReferences.Add(new AssemblyReference(appManifestFile)
            {
                ReferenceType = AssemblyReferenceType.ClickOnceManifest,
                AssemblyIdentity = AssemblyIdentity.FromManifest(appManifestFile),
                Size = manifestStream.Length,
                Hash = Convert.ToBase64String(Hasher.ComputeHash(manifestStream)),
                TargetPath = relativeAppManifestFile
            });
            manifestStream.Close();

            ManifestWriter.WriteManifest(manifest, outFile);
            ReplaceSha1WithSha256(outFile);
        }

        private void CreateApplicationManifest(string appDirectory, string mainAppFileName, string outFile)
        {
            var mainAppFile = Path.Combine(appDirectory, mainAppFileName);

            var manifest = new ApplicationManifest("4.0")
            {
                AssemblyIdentity = new AssemblyIdentity(Path.GetFileName(mainAppFile), Version.ToString())
                {
                    Culture = "en",//_infinityVersionInfo.Language,
                    ProcessorArchitecture = "msil",
                    Type = "win32"
                },
                EntryPoint = new AssemblyReference(mainAppFile)
                {
                    AssemblyIdentity = AssemblyIdentity.FromManagedAssembly(mainAppFile)
                },
                TrustInfo = new TrustInfo
                {
                    IsFullTrust = true
                },
                XmlEntryPointParameters = "",
                OSVersion = "5.1.2600.0"
            };

            manifest.AssemblyReferences.Add(new AssemblyReference
            {
                AssemblyIdentity = new AssemblyIdentity("Microsoft.Windows.CommonLanguageRuntime", "4.0.30319.0"),
                IsPrerequisite = true
            });

            var appDir = new DirectoryInfo(appDirectory);

            foreach (var file in appDir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var targetPath = file.FullName.Substring(appDirectory.Length + 1);
                var ex = file.Extension;

                var groupName = (FileNameToGroupName != null && FileNameToGroupName.ContainsKey(targetPath))
                    ? FileNameToGroupName[targetPath] : null;

                AssemblyIdentity managedIndentity;

                if ((ex == ".dll" || ex == ".exe") &&
                    (managedIndentity = AssemblyIdentity.FromManagedAssembly(file.FullName)) != null)
                {
                    if (_existingApplicationDir != null && _existingApplicationAssemblies.ContainsKey(targetPath))
                    {
                        var fileVersion = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;

                        if (_existingApplicationAssemblies[targetPath].Item1 == fileVersion)
                        {
                            manifest.AssemblyReferences.Add(_existingApplicationAssemblies[targetPath].Item2);
                            File.Copy(Path.Combine(_existingApplicationDir, targetPath), file.FullName, true);
                            file.Refresh();
                        }
                    }

                    manifest.AssemblyReferences.Add(new AssemblyReference(file.FullName)
                    {
                        AssemblyIdentity = managedIndentity,
                        Size = file.Length,
                        Hash = HashFile(file.FullName),
                        TargetPath = targetPath,
                        Group = groupName,
                        IsOptional = groupName != null
                    });
                }
                else
                {
                    manifest.FileReferences.Add(new FileReference(file.FullName)
                    {
                        Size = file.Length,
                        Hash = HashFile(file.FullName),
                        TargetPath = targetPath,
                        Group = groupName,
                        IsOptional = groupName != null
                    });
                }

                file.MoveTo(file.FullName + ".deploy");
            }

            ManifestWriter.WriteManifest(manifest, outFile);

            ReplaceSha1WithSha256(outFile);
        }

        private static void ReplaceSha1WithSha256(string manifestFile)
        {
            File.WriteAllText(manifestFile, File.ReadAllText(manifestFile).Replace(
                "http://www.w3.org/2000/09/xmldsig#sha1",
                "http://www.w3.org/2000/09/xmldsig#sha256"
            ));
        }

        private static string HashFile(string filename)
        {
            var fs = File.OpenRead(filename);
            var hash = Hasher.ComputeHash(fs);
            fs.Close();

            return Convert.ToBase64String(hash);
        }
    }
}
