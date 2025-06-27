using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Literals
{
    public class ConstantDeclarationVisitor : DepthFirstAstVisitor
    {
        private readonly CSharpAstResolver _resolver;
        private readonly List<ConstantDeclaration> _constants;

        public IEnumerable<ConstantDeclaration> Declarations
        {
            get { return _constants.AsReadOnly(); }
        }

        public ConstantDeclarationVisitor(CSharpAstResolver resolver)
        {
            _resolver = resolver;
            _constants = new List<ConstantDeclaration>();
        }

        public override void VisitVariableInitializer(VariableInitializer variableInitializer)
        {
            AstNode node = null;

            var fieldParent = variableInitializer.Parent as FieldDeclaration;

            if (fieldParent != null && fieldParent.Modifiers.HasFlag(Modifiers.Const))
                node = fieldParent;
            else
            {
                var variableParent = variableInitializer.Parent as VariableDeclarationStatement;

                if (variableParent != null && variableParent.Modifiers.HasFlag(Modifiers.Const))
                    node = variableParent;
            }

            if (node != null)
            {
                _constants.Add(new ConstantDeclaration
                {
                    File = _resolver.UnresolvedFile.FileName,
                    StartLocation = new Location(node.StartLocation.Line, node.StartLocation.Column),
                    EndLocation = new Location(node.EndLocation.Line, node.EndLocation.Column)
                });
            }

            base.VisitVariableInitializer(variableInitializer);
        }
    }
}
