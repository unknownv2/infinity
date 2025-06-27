using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NoDev.Infinity.Build.InfinityBuilder.DNGuard
{
    internal class HvmProtector
    {
        private const string AssemblyNodeTemplate = "<assembly><path>{0}</path><obfuscate>true</obfuscate></assembly>";

        private readonly string _dnguardExe;
        private readonly string _projectFile;

        internal IList<string> Assemblies { get; private set; }

        internal HvmProtector(string dnguardExe, string hvmprjFileTemplate)
        {
            if (!File.Exists(hvmprjFileTemplate))
                throw new FileNotFoundException("HVM Project file template not found.", hvmprjFileTemplate);

            _projectFile = hvmprjFileTemplate;
            _dnguardExe = dnguardExe;

            Assemblies = new List<string>();
        }

        internal bool ProtectAssemblies(string outputDirectory)
        {
            if (Assemblies.Count == 0)
                throw new Exception("There are no assemblies to protect.");

            var notFound = Assemblies.FirstOrDefault(path => !File.Exists(path));

            if (notFound != null)
                throw new FileNotFoundException("Assembly file not found.", notFound);

            var projectSettings = string.Format(
                File.ReadAllText(_projectFile), 
                BuildAssemblyNodes(Assemblies), 
                outputDirectory
            );

            var projectFile = Path.GetTempFileName();

            File.WriteAllText(projectFile, projectSettings);

            var proc = new Process
            {
                StartInfo =
                {
                    FileName = _dnguardExe,
                    Arguments = string.Format("/hvmprj=\"{0}\"", projectFile),
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
                throw new Exception("Failed to start DNGuard process.");

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            return proc.ExitCode == 0;
        }

        private static string BuildAssemblyNodes(IEnumerable<string> assemblies)
        {
            var sb = new StringBuilder();

            foreach (var asm in assemblies)
            {
                if (!File.Exists(asm))
                    throw new FileNotFoundException("Assembly file not found.", asm);

                sb.AppendFormat(AssemblyNodeTemplate, asm);
            }

            return sb.ToString();
        }
    }
}
