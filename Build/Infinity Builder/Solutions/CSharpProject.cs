using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace NoDev.Infinity.Build.InfinityBuilder.Solutions
{
    /// <summary>
    /// Represents a C# project (.csproj file)
    /// </summary>
    public class CSharpProject
    {
        /// <summary>
        /// Parent solution.
        /// </summary>
        public Solution Solution { get; private set; }

        /// <summary>
        /// MSBuild project instance.
        /// </summary>
        public Project MsbuildProject { get; private set; }

        /// <summary>
        /// Title is the project name as specified in the .sln file.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Name of the output assembly.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Full path to the .csproj file.
        /// </summary>
        public string FileName { get; private set; }

        public List<CSharpFile> Files { get; private set; }

        public CompilerSettings CompilerSettings { get; private set; }

        /// <summary>
        /// The unresolved type system for this project.
        /// </summary>
        public IProjectContent ProjectContent { get; private set; }

        public string[] CopyLocalAssemblyReferences { get; private set; }

        /// <summary>
        /// The resolved type system for this project.
        /// </summary>
        public ICompilation Compilation
        {
            get
            {
                if (_compilation == null)
                    Solution.Analyze();

                return _compilation;;
            }
            set { _compilation = value; }
        }

        private ICompilation _compilation;

        public CSharpProject(Solution solution, string title, string fileName, string configuration = null)
        {
            fileName = Path.GetFullPath(fileName);

            Solution = solution;
            Title = title;
            FileName = fileName;

            Project msbuildProject;

            if (configuration == null)
                msbuildProject = new Project(fileName);
            else
            {
                var globalProperties = new Dictionary<string, string>();
                globalProperties["Configuration"] = configuration;
                msbuildProject = new Project(fileName, globalProperties, null);
            }

            AssemblyName = msbuildProject.GetPropertyValue("AssemblyName");
            CompilerSettings = new CompilerSettings
            {
                AllowUnsafeBlocks = GetBoolProperty(msbuildProject, "AllowUnsafeBlocks") ?? false,
                CheckForOverflow = GetBoolProperty(msbuildProject, "CheckForOverflowUnderflow") ?? false
            };

            var defineConstants = msbuildProject.GetPropertyValue("DefineConstants");

            foreach (var symbol in defineConstants.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                CompilerSettings.ConditionalSymbols.Add(symbol.Trim());

            IProjectContent pc = new CSharpProjectContent();
            pc = pc.SetAssemblyName(AssemblyName);
            pc = pc.SetProjectFileName(fileName);
            pc = pc.SetCompilerSettings(CompilerSettings);

            Files = new List<CSharpFile>();

            // Parse the C# code files
            foreach (var item in msbuildProject.GetItems("Compile"))
                Files.Add(new CSharpFile(this, Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude)));

            // Add parsed files to the type system
            pc = pc.AddOrUpdateFiles(Files.Select(f => f.UnresolvedTypeSystemForFile));

            // Add referenced assemblies:
            foreach (var assemblyFile in ResolveAssemblyReferences(msbuildProject))
                pc = pc.AddAssemblyReferences(solution.LoadAssembly(assemblyFile));

            // Add project references:
            foreach (var item in msbuildProject.GetItems("ProjectReference"))
            {
                pc = pc.AddAssemblyReferences(new ProjectReference(
                    Path.GetFullPath(Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude))
                ));
            }

            MsbuildProject = msbuildProject;
            ProjectContent = pc;
        }

        private IEnumerable<string> ResolveAssemblyReferences(Project project)
        {
            var projectInstance = project.CreateProjectInstance();

            projectInstance.SetProperty("BuildingProject", "false");
            project.SetProperty("DesignTimeBuild", "true");

            if (!projectInstance.Build("ResolveAssemblyReferences", new[] { new ConsoleLogger(LoggerVerbosity.Minimal) }))
                throw new Exception("Failed to build project.");

            var items = projectInstance.GetItems("_ResolveAssemblyReferenceResolvedFiles");

            var baseDirectory = Path.GetDirectoryName(FileName);
            Debug.Assert(baseDirectory != null, "baseDirectory != null");

            CopyLocalAssemblyReferences = items
                .Where(i => i.GetMetadataValue("CopyLocal") == "true")
                .Select(i => Path.Combine(baseDirectory, i.GetMetadataValue("Identity"))).ToArray();

            var result = items.Select(i => Path.Combine(baseDirectory, i.GetMetadataValue("Identity"))).ToList();

            if (!result.Any(t => t.Contains("mscorlib") || t.Contains("System.Runtime")))
                result.Add(typeof(object).Assembly.Location);

            return result;
        }

        private static bool? GetBoolProperty(Project p, string propertyName)
        {
            var val = p.GetPropertyValue(propertyName);
            bool result;

            if (bool.TryParse(val, out result))
                return result;

            return null;
        }

        public override string ToString()
        {
            return string.Format("[CSharpProject AssemblyName={0}]", AssemblyName);
        }
    }
}
