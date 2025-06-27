using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NoDev.Common.Cryptography;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using NoDev.Infinity.Build.InfinityBuilder.Manifests;
using NoDev.Infinity.Build.InfinityBuilder.Solutions;
using NoDev.Infinity.Build.InfinityBuilder.Tools;
using NoDev.Infinity.Build.InfinityBuilder.Tools.Serializers;
using NoDev.Infinity.Build.InfinityBuilder.DNGuard;
using NoDev.Infinity.Build.InfinityBuilder.Deployment;
using NoDev.InfinityToolLib.Tools;
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder
{
    internal class Builder
    {
        private const string
            RelativeToolProjectsDirectory = "Tools",
            InfinityAssemblyName = "Infinity",
            ChallengeAssemblyName = "_",
            GameManifestFileName = "Games.json",
            ToolManifestFileName = "Manifest.json",
            DnGuardProjectFile = @"\Resources\DNGuardFull.hvmprj";

        private const string DefaultSnExePath =
            @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\sn.exe";

        private static readonly Type LiteralCollectionType = typeof(ILiteralCollection);
        private static readonly object[] IgnoredLiteralValues =
        {
            (byte)0, (sbyte)0, (short)0, (ushort)0, 0, 0U, 0L, 0UL, 0f, 0.0, (decimal)0, "0",
            (byte)1, (sbyte)1, (short)1, (ushort)1, 1, 1U, 1L, 1UL, 1f, 1.0, (decimal)1, "1",
            (sbyte)-1, (short)-1, -1, -1L, -1f, -1.0, (decimal)-1,
            "", "/", "\\", "\n", "\n\n", "\r\n", "\r\n\r\n", "\0", true, false, null
        };

        private readonly BuildArguments _args;
        private readonly Solution _solution;
        private readonly Dictionary<Dir, string> _directories;
        private readonly X509Certificate2 _certificate;
        private readonly Uri _certificateUri;

        private byte[] _literalsKey;

        private enum Dir
        {
            Solution,
            BuiltAssemblies,
            ProtectedAssemblies,
            ClientFiles,
            ClientTools,
            ServerFiles,
            ServerLiterals,
            ClickOnce
        }

        internal Builder(BuildArguments args)
        {
            _args = args;
            _directories = new Dictionary<Dir, string>();

            if (_args.SnContainer == null && _args.SnPublicKeyToken != null)
                throw new Exception("Strong name container must be specified if a public key token is provided.");

            if (_args.SnContainer != null && _args.SnPublicKeyToken == null)
                throw new Exception("Assembly public key token must be specified if a strong name container is provided.");

            if (args.SigningCertificateThumbprint != null)
            {
                var certStore = new X509Store();
                certStore.Open(OpenFlags.ReadOnly);
                var foundCerts = certStore.Certificates.Find(X509FindType.FindByThumbprint, args.SigningCertificateThumbprint.ToUpperInvariant(), true);
                if (foundCerts.Count == 0)
                    throw new Exception("Certificate not found in user store.");
                if (foundCerts.Count != 1)
                    throw new Exception("More than one certificate found with the given thumbprint.");
                _certificate = foundCerts[0];
                certStore.Close();
                if (args.SigningCertificateTimestampUrl != null)
                    _certificateUri = new Uri(args.SigningCertificateTimestampUrl);
            }
            
            SetDir(Dir.Solution, _args.WorkingDirectory, "Solution");
            SetDir(Dir.BuiltAssemblies, _args.WorkingDirectory, "Built");
            SetDir(Dir.ProtectedAssemblies, _args.WorkingDirectory, "Protected");
            SetDir(Dir.ClientFiles, _args.WorkingDirectory, "Client");
            SetDir(Dir.ClientTools, _args.WorkingDirectory, "Client", "tools");
            SetDir(Dir.ServerFiles, _args.WorkingDirectory, "Server");
            SetDir(Dir.ServerLiterals, _args.WorkingDirectory, "Server", "literals");
            SetDir(Dir.ClickOnce, _args.WorkingDirectory, "Server", "build");

            Console.WriteLine("Creating a copy of the solution...");
            _args.SolutionFile = CopySolution(_args.SolutionFile, GetDir(Dir.Solution));

            Console.WriteLine("\r\nAnalyzing the solution...");
            _solution = new Solution(_args.SolutionFile, _args.BuildConfiguration);
        }

        internal void Build()
        {
            Console.WriteLine("\r\nWriting initial server key and URL...");

            _literalsKey = WriteServerConfigClass(_args.InitialServerKey, _args.ServerUrl);

            Console.WriteLine("\r\nBuilding the solution...");

            var builtProjects = new List<CSharpProject>();

            var infinityProject = _solution.Projects.First(p => p.AssemblyName == InfinityAssemblyName);
            EnsureProjectAndProjectReferencesAreBuilt(infinityProject, builtProjects);

            var challengeProject = _solution.Projects.First(p => p.AssemblyName == ChallengeAssemblyName);
            EnsureProjectAndProjectReferencesAreBuilt(challengeProject, builtProjects);

            var gameManifest = ProcessGameManifest(infinityProject.MsbuildProject.DirectoryPath);
            var toolManifest = ProcessToolManifests(gameManifest);

            RemoveGamesWithNoTools(gameManifest, toolManifest);

            var manifestCompiler = new ManifestCompiler();
            File.WriteAllText(GetDir(Dir.ClientFiles, "Manifest.json"), manifestCompiler.CompileForClient(gameManifest, toolManifest));
            File.WriteAllText(GetDir(Dir.ServerFiles, "Manifest.json"), manifestCompiler.CompileForServer(gameManifest, toolManifest));

            // extra project libs to build
            foreach (var tool in toolManifest.Values)
                EnsureProjectReferencesAreBuilt(tool.Project, builtProjects);

            foreach (var tool in builtProjects.SelectMany(p => toolManifest.Values.Where(t => p.MsbuildProject == t.Project.MsbuildProject)))
                throw new Exception(string.Format("A project cannot reference a tool (ref: {0}).", tool.Id));

            // build the tools in parallel
            toolManifest.Values.AsParallel().ForAll(BuildTool);

            var builtAssembliesDir = GetDir(Dir.BuiltAssemblies);

            if (_args.DNGuardCmdPath == null)
            {
                // manually move all the assemblies because DNGuard isn't going to.
                foreach (var referenceProject in builtProjects.Select(p => p.MsbuildProject))
                    File.Copy(referenceProject.GetProperty("TargetPath").EvaluatedValue, builtAssembliesDir);
            }
            else
            {
                Console.WriteLine("\r\nProtecting the assemblies with DNGuard...");

                var dnguard = new HvmProtector(_args.DNGuardCmdPath, Environment.CurrentDirectory + DnGuardProjectFile);

                foreach (var referenceProject in builtProjects.Select(p => p.MsbuildProject))
                    dnguard.Assemblies.Add(referenceProject.GetProperty("TargetPath").EvaluatedValue);

                foreach (var toolInfo in toolManifest.Values)
                    dnguard.Assemblies.Add(Path.Combine(builtAssembliesDir, toolInfo.Id));

                builtAssembliesDir = GetDir(Dir.ProtectedAssemblies);

                if (!dnguard.ProtectAssemblies(builtAssembliesDir))
                    throw new Exception("DNGuard failed.");
            }

            var clientToolsDir = GetDir(Dir.ClientTools);

            foreach (var toolInfo in toolManifest.Values)
                File.Move(Path.Combine(builtAssembliesDir, toolInfo.Id), Path.Combine(clientToolsDir, toolInfo.Id + ".dll"));

            var coreFiles = Directory.GetFiles(builtAssembliesDir);
            var clientDir = GetDir(Dir.ClientFiles);

            foreach (var coreFile in coreFiles)
            {
                var coreFileName = Path.GetFileName(coreFile);

                if (coreFileName == null)
                    throw new Exception(string.Format("Core file path is invalid ({0}).", coreFile));

                File.Move(coreFile, Path.Combine(clientDir, coreFileName));
            }

            CopyCopyLocalReferences(builtProjects, clientDir);

            if (_args.SnPublicKeyToken != null && _args.SnContainer != null)
            {
                Console.WriteLine("\r\nStrong name signing assemblies...");
                StrongNameSignAssemblies(clientDir, _args.SnPublicKeyToken, _args.SnContainer,
                    _args.SnExePath ?? DefaultSnExePath);
            }

            if (_certificate != null)
            {
                Console.WriteLine("\r\nSigning PE files...");
                SignPeFiles(clientDir, _certificate, _certificateUri);
            }

            var infinityExePath = Path.Combine(clientDir, infinityProject.MsbuildProject.GetProperty("TargetFileName").EvaluatedValue);

            byte[] challengeHash = null;

            var challengeSkipped = _args.ServerChallengeKeys == null || _args.ServerChallengeKeys.Length != 2;

            if (challengeSkipped)
                Console.WriteLine("\r\nSkipping server challenge hash generation.");
            else
            {
                Console.WriteLine("\r\nGenerating server challenge hash...");

                var challengeDllPath = GetDir(Dir.ServerFiles, "Challenge.dll");

                File.Move(
                    Path.Combine(clientDir, challengeProject.MsbuildProject.GetProperty("TargetFileName").EvaluatedValue),
                    challengeDllPath
                );

                challengeHash = GenerateChallengeHash(
                    infinityExePath,
                    challengeDllPath,
                    _args.ServerChallengeKeys[0],
                    _args.ServerChallengeKeys[1]
                );
            }

            var clientInfo = JsonConvert.SerializeObject(new
            {
                Version = FileVersionInfo.GetVersionInfo(infinityExePath).FileVersion,
                InitialKey = _args.InitialServerKey,
                BaseHash = challengeHash
            });

            File.WriteAllText(GetDir(Dir.ServerFiles, "Client.json"), clientInfo);

            Console.WriteLine("\r\nBuilding deployment package...");

            var toolToGroup = new Dictionary<string, string>(toolManifest.Values.Count);

            foreach (var tool in toolManifest.Values)
                toolToGroup.Add(string.Format("tools\\{0}.dll", tool.Id), tool.GameId);

            var isServerRemote = _args.ServerUrl.StartsWith("http://") || _args.ServerUrl.StartsWith("https://");

            var clickOnce = new ClickOnceBuilder(clientDir, _args.ExistingApplicationFile)
            {
                Version = _args.NewApplicationVersion,
                DeploymentUrl = isServerRemote ? _args.ServerUrl + "/deployment" : _args.ServerUrl,
                Certificate = _certificate,
                CertificateTimestampUri = _certificateUri,
                FileNameToGroupName = toolToGroup
            };

            clickOnce.Build(GetDir(Dir.ClickOnce));

            var outFile = Path.Combine(_args.WorkingDirectory, "package.zip");
            var serverDir = GetDir(Dir.ServerFiles);

            if (_args.DeploymentKey != null)
            {
                var tmpOutFile = Path.Combine(_args.WorkingDirectory, "package_decrypted.zip");
                ZipFile.CreateFromDirectory(serverDir, tmpOutFile);
                SimpleAES.Encrypt(tmpOutFile, outFile, _args.DeploymentKey);
            }
            else
            {
                ZipFile.CreateFromDirectory(serverDir, outFile);
            }

            if (_args.NoUpload)
            {
                Console.WriteLine("Skipping upload.");
            }
            else if (_args.DeploymentId == null || _args.DeploymentKey == null)
            {
                Console.WriteLine("Skipping upload because deployment ID or key was not specified.");
            }
            else if (!isServerRemote)
            {
                Console.WriteLine("Skipping upload because server URL is local.");
            }
            else if (_args.DNGuardCmdPath == null)
            {
                Console.WriteLine("Skipping upload because assemblies are not protected with DNGuard.");
            }
            else
            {
                Console.WriteLine("Deploying Infinity...");

                Deploy(outFile, _args.ServerUrl);

                Console.WriteLine("Deploy finished.");
            }
        }

        private void Deploy(string package, string serverUrl)
        {
            var req = WebRequest.Create(serverUrl);

            req.Method = "POST";
            req.ContentType = "application/octet-stream";
            req.Headers["X-Inf-Client"] = "Deployment";
            req.Headers["X-Inf-Deploy"] = _args.DeploymentId;

            var reqStream = req.GetRequestStream();
            var packageStream = File.OpenRead(package);
            packageStream.CopyTo(reqStream);
            packageStream.Close();
            reqStream.Close();

            try
            {
                var res = req.GetResponse();

                if (res.Headers["X-Inf-Message"] != null)
                {
                    Console.WriteLine("Server message: " + res.Headers["X-Inf-Message"]);
                }
            }
            catch (WebException ex)
            {
                throw new Exception("Server message: " + ex.Response.Headers["X-Inf-Message"]);
            }
        }

        private byte[] WriteServerConfigClass(byte[] key, string url)
        {
            var serverFile = GetDir(Dir.Solution, @"Infinity\Network\Server.cs");

            var lines = File.ReadAllLines(serverFile);

            for (var x = 0; x < lines.Length; x++)
            {
                if (lines[x].Contains("//KEY"))
                {
                    if (key == null)
                    {
                        key = new byte[32];
                        var provider = new RNGCryptoServiceProvider();
                        provider.GetBytes(key);
                    }

                    lines[x] = ByteArrayTransformer.ToLocalVariable(key, "arr");
                }
                else if (lines[x].Contains("//URL"))
                {
                    lines[x] = string.Format("return \"{0}\";", url);
                }
            }

            File.WriteAllLines(serverFile, lines);

            return key;
        }

        private static byte[] GenerateChallengeHash(string infinityExe, string challengeAsmPath, 
            string b64SessionId, string b64Nonce)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = infinityExe,
                    Arguments = string.Format(
                        "ServerProxy \"{0}\" \"{1}\" \"{2}\"",
                        challengeAsmPath,
                        b64SessionId, 
                        b64Nonce),
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            var output = "";

            proc.OutputDataReceived += (s, e) => output += e.Data;
            proc.ErrorDataReceived += (s, e) => output += e.Data;

            if (!proc.Start())
                throw new Exception("Failed to start Infinity for challenge hash generation.");

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            if (proc.ExitCode != 0 || output.Length == 0)
                throw new Exception("Failed to generate challenge hash.");

            try
            {
                return Convert.FromBase64String(output);
            }
            catch
            {
                throw new Exception("Invalid challenge hash format.");
            }
        }

        private static void SignPeFiles(string dir, X509Certificate2 cert, Uri certUri)
        {
            var filesDir = new DirectoryInfo(dir);

            foreach (var file in filesDir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories))
            {
                var ext = file.Extension;
                if (ext != ".exe" && ext != ".dll")
                    continue;

                SecurityUtilities.SignFile(cert, certUri, file.FullName);
            }
        }

        private static void StrongNameSignAssemblies(string dir, string publicKeyToken, string snContainer, string snExePath)
        {
            var filesDir = new DirectoryInfo(dir);

            foreach (var file in filesDir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories))
            {
                var ext = file.Extension;
                if (ext != ".exe" && ext != ".dll")
                    continue;

                var asm = AssemblyIdentity.FromManagedAssembly(file.FullName);

                if (asm == null || asm.PublicKeyToken != publicKeyToken)
                    continue;

                var proc = new Process
                {
                    StartInfo =
                    {
                        FileName = snExePath,
                        Arguments = string.Format("-Rca {0} {1}", file.FullName, snContainer),
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                proc.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
                proc.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

                if (!proc.Start())
                    throw new Exception("Failed to start sn.exe process.");

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                    throw new Exception("Strong name signing failed.");
            }
        }

        private static void CopyCopyLocalReferences(IEnumerable<CSharpProject> projects, string outDir)
        {
            var sha1 = SHA1.Create();
            var copied = new Dictionary<string, string>();

            foreach (var project in projects)
            {
                foreach (var asm in project.CopyLocalAssemblyReferences)
                {
                    var fs = File.OpenRead(asm);
                    var hash = Convert.ToBase64String(sha1.ComputeHash(fs));
                    fs.Close();

                    var fileName = Path.GetFileName(asm);

                    if (fileName == null)
                        throw  new Exception(string.Format("Invalid assembly file path ({0}).", asm));

                    if (!copied.ContainsKey(fileName))
                    {
                        File.Copy(asm, Path.Combine(outDir, fileName));
                        copied.Add(fileName, hash);
                    }
                    else if (copied[fileName] != hash)
                        throw new Exception(string.Format(
                            "Project '{0}' references an assembly with the same filename '{1}' but different contents as the reference of another project.",
                            project.Title, fileName));
                }
            }
        }

        private void BuildTool(ToolInfo toolInfo)
        {
            var result = new ToolBuilder(toolInfo, LiteralCollectionType, IgnoredLiteralValues).Build();

            if (!result.Success)
                throw new Exception(string.Format("Failed to build tool ({0}).", toolInfo.Id));

            // this keeps the assembly open, so it can't be deleted until this program exits
            var toolAsm = Assembly.ReflectionOnlyLoadFrom(result.OutputAssemblyPath);

            var guidAttrType = typeof(GuidAttribute);
            var guid = (string)toolAsm.GetCustomAttributesData().First(c => c.AttributeType == guidAttrType).ConstructorArguments[0].Value;

            if (guid != toolInfo.Id)
                throw new Exception(string.Format("Tool ID in manifest ({0}) does not match assembly GUID ({1}).", toolInfo.Id, guid));

            /*var toolType = typeof (Tool);
            var numToolTypes = toolAsm.GetTypes().Count(t => t.IsSubclassOf(toolType));

            if (numToolTypes == 0)
                throw new Exception(string.Format("Tool ({0}) must include a class derrived from type Tool.", guid));

            if (numToolTypes > 1)
                throw new Exception(string.Format("Only one class can be derrived from base class Tool ({0}).", guid));*/

            File.Copy(result.OutputAssemblyPath, GetDir(Dir.BuiltAssemblies, guid), true);

            if (result.ReplacedLiterals != null)
                SerializeLiterals(result.ReplacedLiterals, GetDir(Dir.ServerLiterals, guid), _literalsKey);
        }

        private static void SerializeLiterals(IDictionary<int, object> literals, string outFile, byte[] key)
        {
            var literalSerializer = key == null 
                ? new BinaryLiteralSerializer() 
                : new EncryptedBinaryLiteralSerializer(key);

            File.WriteAllBytes(outFile, literalSerializer.Serialize(literals));
        }

        private static void RemoveGamesWithNoTools(GameManifest gameManifest, ToolManifest toolManifest)
        {
            var gamesToRemove = gameManifest.Keys.Where(gameId => toolManifest.Values.All(t => t.GameId != gameId)).ToList();

            foreach (var gameId in gamesToRemove)
                gameManifest.Remove(gameId);
        }

        private ToolManifest ProcessToolManifests(GameManifest gameManifest)
        {
            var toolProjectsDir = Path.Combine(GetDir(Dir.Solution), RelativeToolProjectsDirectory) + "\\";

            var toolManifest = new ToolManifest();

            foreach (var project in _solution.Projects.Where(p => p.FileName.StartsWith(toolProjectsDir)))
            {
                var manifestFile = Path.Combine(project.MsbuildProject.DirectoryPath, ToolManifestFileName);

                if (!File.Exists(manifestFile))
                    throw new Exception("Tool manifest file does not exist at " + manifestFile);

                var toolId = toolManifest.AddFromManifest(project, manifestFile);

                if (toolId == null)
                    continue;

                if (!gameManifest.ContainsKey(toolManifest[toolId].GameId))
                    throw new Exception(string.Format("Unknown game ID '{0}' in tool '{1}'.", toolManifest[toolId].GameId, toolId));

                if (string.IsNullOrWhiteSpace(toolManifest[toolId].Name))
                    throw new Exception(string.Format("Tool name cannot be empty ({0}).", toolId));
            }

            return toolManifest;
        }

        private GameManifest ProcessGameManifest(string infinityProjectDir)
        {
            var manifestFile = Path.Combine(infinityProjectDir, GameManifestFileName);

            if (!File.Exists(manifestFile))
                throw new Exception("Game manifest file does not exist at " + manifestFile);

            var gameManifest = new GameManifest(manifestFile);

            foreach (var gameInfo in gameManifest)
            {
                if (string.IsNullOrWhiteSpace(gameInfo.Key))
                    throw new Exception(string.Format("Game ID cannot be empty ({0}).", gameInfo.Value.Name));

                if (string.IsNullOrWhiteSpace(gameInfo.Value.Name))
                    throw new Exception(string.Format("Game name cannot be empty ({0}).", gameInfo.Key));

                if (!_args.IgnoreInvalidGameThumbnails && (gameInfo.Value.Thumbnail == null || gameInfo.Value.Thumbnail.Length == 0))
                    throw new Exception(string.Format("Game thumbnail must be set ({0}).", gameInfo.Key));
            }

            return gameManifest;
        }

        private void SetDir(Dir dirType, params string[] dirParts)
        {
            if (dirParts == null || dirParts.Length == 0)
                throw new ArgumentNullException("dirParts");

            var dir = Path.Combine(dirParts);

            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, true); }
                catch (IOException e) { throw new Exception("Cannot delete directory. Close all Explorer windows and try again.", e); }
                
                while (Directory.Exists(dir))
                    Thread.Sleep(10);
            }

            Directory.CreateDirectory(dir);

            _directories[dirType] = dir;
        }

        private string GetDir(Dir dirType, params string[] appendPaths)
        {
            if (appendPaths == null || appendPaths.Length == 0)
                return _directories[dirType];

            return _directories[dirType] + "\\" + Path.Combine(appendPaths);
        }

        private static string CopySolution(string slnFile, string dir)
        {
            var solutionDir = Path.GetDirectoryName(slnFile);
            FileSystem.CopyDirectory(solutionDir, dir, true);

            var filename = Path.GetFileName(slnFile);

            if (filename == null)
                throw new Exception(string.Format("Solution file path is invalid ({0}).", slnFile));

            return Path.Combine(dir, filename);
        }

        private void EnsureProjectReferencesAreBuilt(CSharpProject project, ICollection<CSharpProject> alreadyBuilt = null)
        {
            foreach (var item in project.MsbuildProject.GetItems("ProjectReference"))
            {
                var refProjPath = Path.GetFullPath(Path.Combine(project.MsbuildProject.DirectoryPath, item.EvaluatedInclude));
                var refProj = _solution.Projects.FirstOrDefault(p => p.FileName == refProjPath);

                // the referenced project isn't in this solution
                if (refProj == null)
                    continue;

                EnsureProjectAndProjectReferencesAreBuilt(refProj, alreadyBuilt);
            }
        }

        private void EnsureProjectAndProjectReferencesAreBuilt(CSharpProject project, ICollection<CSharpProject> alreadyBuilt)
        {
            foreach (var item in project.MsbuildProject.GetItems("ProjectReference"))
            {
                var refProjPath = Path.GetFullPath(Path.Combine(project.MsbuildProject.DirectoryPath, item.EvaluatedInclude));
                var refProj = _solution.Projects.FirstOrDefault(p => p.FileName == refProjPath);

                // the referenced project isn't in this solution
                if (refProj == null)
                    continue;

                EnsureProjectAndProjectReferencesAreBuilt(refProj, alreadyBuilt);
            }

            if (alreadyBuilt.Contains(project))
                return;

            if (!project.MsbuildProject.Build(new ConsoleLogger(LoggerVerbosity.Minimal)))
                throw new Exception("Failed to build project dependency.");

            alreadyBuilt.Add(project);
        }
    }
}
