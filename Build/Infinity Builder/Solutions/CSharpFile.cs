using System;
using System.IO;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;

namespace NoDev.Infinity.Build.InfinityBuilder.Solutions
{
    public class CSharpFile
    {
        public readonly CSharpProject Project;
        public readonly string FileName;
        public readonly string OriginalText;

        public SyntaxTree SyntaxTree;
        public CSharpUnresolvedFile UnresolvedTypeSystemForFile;

        public CSharpFile(CSharpProject project, string fileName)
        {
            Project = project;
            FileName = fileName;

            var parser = new CSharpParser(project.CompilerSettings);

            OriginalText = File.ReadAllText(fileName);
            SyntaxTree = parser.Parse(OriginalText, fileName);
            SyntaxTree.Freeze();

            if (parser.HasErrors)
            {
                Console.WriteLine("Error parsing {0}:", fileName);

                foreach (var error in parser.ErrorsAndWarnings)
                    Console.WriteLine("  {0} {1}", error.Region, error.Message);
            }

            UnresolvedTypeSystemForFile = SyntaxTree.ToTypeSystem();
        }

        public CSharpAstResolver CreateResolver()
        {
            return new CSharpAstResolver(Project.Compilation, SyntaxTree, UnresolvedTypeSystemForFile);
        }
    }
}
