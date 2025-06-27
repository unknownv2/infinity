using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Utils;

namespace NoDev.Infinity.Build.InfinityBuilder.Solutions
{
    public class Solution
    {
        private readonly ConcurrentDictionary<string, IUnresolvedAssembly> _assemblyDict;

        public readonly string Directory;
        public readonly List<CSharpProject> Projects = new List<CSharpProject>();

        private ISolutionSnapshot _solutionSnapshot;
        public ISolutionSnapshot SolutionSnapshot
        {
            get
            {
                if (_solutionSnapshot == null)
                    Analyze();

                return _solutionSnapshot;
            }
        }

        public IEnumerable<CSharpFile> AllFiles
        {
            get { return Projects.SelectMany(p => p.Files); }
        }

        public Solution(string fileName, string configuration = null)
        {
            _assemblyDict = new ConcurrentDictionary<string, IUnresolvedAssembly>(Platform.FileNameComparer);

            Directory = Path.GetDirectoryName(fileName);
            Debug.Assert(Directory != null, "Directory != null");

            var projectLinePattern = new Regex("Project\\(\"(?<TypeGuid>.*)\"\\)\\s+=\\s+\"(?<Title>.*)\",\\s*\"(?<Location>.*)\",\\s*\"(?<Guid>.*)\"");
            
            foreach (var line in File.ReadLines(fileName))
            {
                var match = projectLinePattern.Match(line);

                if (!match.Success)
                    continue;

                var typeGuid = match.Groups["TypeGuid"].Value;
                var title = match.Groups["Title"].Value;
                var location = match.Groups["Location"].Value;

                switch (typeGuid.ToUpperInvariant())
                {
                    case "{2150E333-8FDC-42A3-9474-1A3956D46DE8}": // Solution Folder
                        // ignore folders
                        break;
                    case "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}": // C# project
                        Projects.Add(new CSharpProject(this, title, Path.Combine(Directory, location), configuration));
                        break;
                    default:
                        Console.WriteLine("Unknown project type {0} in {1}", typeGuid, location);
                        break;
                }
            }
        }

        public void Analyze()
        {
            _solutionSnapshot = new DefaultSolutionSnapshot(Projects.Select(p => p.ProjectContent));

            foreach (var project in Projects)
                project.Compilation = SolutionSnapshot.GetCompilation(project.ProjectContent);
        }

        /// <summary>
        /// Loads a referenced assembly from a .dll.
        /// Returns the existing instance if the assembly was already loaded.
        /// </summary>
        public IUnresolvedAssembly LoadAssembly(string assemblyFileName)
        {
            return _assemblyDict.GetOrAdd(assemblyFileName, file => AssemblyLoader.Create().LoadAssemblyFile(file));
        }
    }
}
