using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NoDev.Infinity.Build.InfinityBuilder.Solutions;
using NoDev.Infinity.Build.InfinityBuilder.Tools.Literals;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using NoDev.Infinity.Build.InfinityBuilder.Manifests;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools
{
    internal class ToolBuilder
    {
        private const string LiteralClassNamespace = "NoDev.__Dynamic";
        private const string LiteralClassName = "Literals";
        private const string LiteralClassFilename = "__dynamic.cs";
        private const string LiteralClassFileContentTemplate = @"namespace {0}
{{
    public static class {1}
    {{
        private static {2} _literals;

        public static void SetLiterals({2} literals)
        {{
            if (_literals != null)
                throw new System.Exception(""Cannot set literals more than one time."");

            _literals = literals;
        }}

        internal static T GetValue<T>(int id)
        {{
            return _literals.GetValue<T>(id);
        }}
    }}
}}";

        private readonly ToolInfo _toolInfo;
        private readonly object[] _ignoredLiterals;
        private readonly string _literalCollectionType;

        internal ToolBuilder(ToolInfo toolInfo, Type iLiteralCollectionType, object[] ignoredLiterals = null)
        {
            _toolInfo = toolInfo;
            _ignoredLiterals = ignoredLiterals ?? new object[0];
            _literalCollectionType = iLiteralCollectionType.FullName;
        }

        internal ToolBuildResult Build()
        {
            var result = new ToolBuildResult();

            if (_toolInfo.AccessLevel >= 2)
                result.ReplacedLiterals = ExtractAndReplaceLiterals();

            if (!_toolInfo.Project.MsbuildProject.Build(new ConsoleLogger(LoggerVerbosity.Minimal)))
                return result;

            result.OutputAssemblyPath = _toolInfo.Project.MsbuildProject.GetProperty("TargetPath").EvaluatedValue;

            return result;
        }

        private IDictionary<int, object> _currentLiterals;
        private IDictionary<int, object> ExtractAndReplaceLiterals()
        {
            _currentLiterals = new Dictionary<int, object>();

            foreach (var file in _toolInfo.Project.Files)
                ProcessFile(file);

            var dynamicClassContent = GetDynamicLiteralClassFileContent(_literalCollectionType);

            File.WriteAllText(
                Path.Combine(_toolInfo.Project.MsbuildProject.DirectoryPath, LiteralClassFilename), 
                dynamicClassContent
            );

            _toolInfo.Project.MsbuildProject.AddItem("Compile", LiteralClassFilename);

            return _currentLiterals;
        }

        private static readonly IDictionary<string, string> CachedDynamicsClassFiles = new ConcurrentDictionary<string, string>();
        private static string GetDynamicLiteralClassFileContent(string literalCollectionType)
        {
            string ret;
            if (CachedDynamicsClassFiles.TryGetValue(literalCollectionType, out ret))
                return ret;

            var content = string.Format(
                LiteralClassFileContentTemplate, 
                LiteralClassNamespace, 
                LiteralClassName,
                literalCollectionType
            );

            CachedDynamicsClassFiles[literalCollectionType] = content;

            return content;
        }

        private void ProcessFile(CSharpFile file)
        {
            var resolver = file.CreateResolver();

            var literalVisitor = new LiteralVisitor(resolver);
            file.SyntaxTree.AcceptVisitor(literalVisitor);
            var literals = literalVisitor.Literals;

            var constantVisitor = new ConstantDeclarationVisitor(resolver);
            file.SyntaxTree.AcceptVisitor(constantVisitor);
            var constants = constantVisitor.Declarations;

            var codeLocs = literals.Where(c => !c.IsConstant).Cast<CodeLocation>().Concat(constants).ToList();

            if (codeLocs.Count == 0)
                return;

            codeLocs.Sort((l1, l2) => -CodeLocationComparison(l1, l2));

            var codeLines = File.ReadAllLines(file.FileName);

            var lineToCharIndex = new int[codeLines.Length + 1];

            for (int x = 0, i = 0; x < codeLines.Length; x++)
            {
                lineToCharIndex[x + 1] = i;
                i += codeLines[x].Length + 2;
            }

            var sb = new StringBuilder(File.ReadAllText(file.FileName));

            foreach (var codeLoc in codeLocs)
            {
                string newText;

                var literal = codeLoc as Literal;

                if (literal == null)
                    newText = "";
                else
                {
                    if (_ignoredLiterals.Contains(literal.Value))
                        continue;

                    var type = literal.Value.GetType();

                    newText = string.Format(
                        "global::" + LiteralClassNamespace + "." + LiteralClassName + ".GetValue<global::{0}>({1})",
                        type.FullName,
                        AddToCurrentLiteralDictionary(literal.Value)
                    );
                }

                var startLoc = lineToCharIndex[codeLoc.StartLocation.LineIndex] + codeLoc.StartLocation.ColumnIndex - 1;
                var endLoc = lineToCharIndex[codeLoc.EndLocation.LineIndex] + codeLoc.EndLocation.ColumnIndex - 1;
                var length = endLoc - startLoc;

                sb.Remove(startLoc, length);
                sb.Insert(startLoc, newText);
            }

            File.WriteAllText(file.FileName, sb.ToString());
        }

        private static readonly Random Rand = new Random();
        private int AddToCurrentLiteralDictionary(object value)
        {
            foreach (var existing in _currentLiterals.Where(t => t.Value.Equals(value)))
                return existing.Key;

            int id;
            do { id = Rand.Next(Int32.MinValue, Int32.MaxValue); }
            while (_currentLiterals.ContainsKey(id));

            _currentLiterals.Add(id, value);

            return id;
        }

        private static int CodeLocationComparison(CodeLocation l1, CodeLocation l2)
        {
            if (l1.StartLocation.LineIndex < l2.StartLocation.LineIndex)
                return -1;

            if (l1.StartLocation.LineIndex > l2.StartLocation.LineIndex)
                return 1;

            if (l1.StartLocation.ColumnIndex < l2.StartLocation.ColumnIndex)
                return -1;

            if (l1.StartLocation.ColumnIndex > l2.StartLocation.ColumnIndex)
                return 1;

            return 0;
        }
    }
}
