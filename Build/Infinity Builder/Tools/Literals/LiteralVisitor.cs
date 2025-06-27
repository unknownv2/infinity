using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Literals
{
    public class LiteralVisitor : DepthFirstAstVisitor
    {
        private readonly CSharpAstResolver _resolver;
        private readonly List<Literal> _literals;

        public IEnumerable<Literal> Literals
        {
            get { return _literals.AsReadOnly(); }
        }

        public LiteralVisitor(CSharpAstResolver resolver)
        {
            _resolver = resolver;
            _literals = new List<Literal>();
        }

        private static object ResolveValue(object value, string typeName)
        {
            if (typeName == value.GetType().ToString())
                return value;

            var valType = Type.GetType(typeName);

            if (valType == null)
                throw new Exception(string.Format("Unknown value type {0}.", typeName));

            return Convert.ChangeType(value, valType);
        }

        public override void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            var resolved = _resolver.Resolve(switchStatement.Expression) as ConstantResolveResult;

            if (resolved != null)
            {
                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(resolved.ConstantValue, resolved.Type.FullName),
                    StartLocation = new Location(switchStatement.Expression.StartLocation.Line, switchStatement.Expression.StartLocation.Column),
                    EndLocation = new Location(switchStatement.Expression.EndLocation.Line, switchStatement.Expression.EndLocation.Column)
                });
            }

            base.VisitSwitchStatement(switchStatement);
        }

        public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            var operatorResolveResult = _resolver.Resolve(conditionalExpression) as OperatorResolveResult;

            if (operatorResolveResult == null)
            {
                base.VisitConditionalExpression(conditionalExpression);
                return;
            }

            var op1 = operatorResolveResult.Operands[1] as ConstantResolveResult;

            if (op1 != null)
            {
                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(op1.ConstantValue, op1.Type.FullName),
                    StartLocation = new Location(conditionalExpression.TrueExpression.StartLocation.Line, conditionalExpression.TrueExpression.StartLocation.Column),
                    EndLocation = new Location(conditionalExpression.TrueExpression.EndLocation.Line, conditionalExpression.TrueExpression.EndLocation.Column)
                });
            }

            var op2 = operatorResolveResult.Operands[2] as ConstantResolveResult;

            if (op2 != null)
            {
                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(op2.ConstantValue, op2.Type.FullName),
                    StartLocation = new Location(conditionalExpression.FalseExpression.StartLocation.Line, conditionalExpression.FalseExpression.StartLocation.Column),
                    EndLocation = new Location(conditionalExpression.FalseExpression.EndLocation.Line, conditionalExpression.FalseExpression.EndLocation.Column)
                });
            }

            base.VisitConditionalExpression(conditionalExpression);
        }

        public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            var operatorResolveResult = _resolver.Resolve(binaryOperatorExpression) as OperatorResolveResult;

            if (operatorResolveResult == null)
            {
                base.VisitBinaryOperatorExpression(binaryOperatorExpression);
                return;
            }
            
            var op1 = operatorResolveResult.Operands[0] as ConstantResolveResult;
            var op2 = operatorResolveResult.Operands[1] as ConstantResolveResult;

            if (op1 != null)
            {
                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(op1.ConstantValue, op1.Type.FullName),
                    StartLocation = new Location(binaryOperatorExpression.Left.StartLocation.Line, binaryOperatorExpression.Left.StartLocation.Column),
                    EndLocation = new Location(binaryOperatorExpression.Left.EndLocation.Line, binaryOperatorExpression.Left.EndLocation.Column)
                });

                if (op2 == null)
                {
                    var conversionOp = operatorResolveResult.Operands[1] as ConversionResolveResult;

                    if (conversionOp != null && conversionOp.Conversion.IsImplicit && conversionOp.Input.IsCompileTimeConstant)
                    {
                        _literals.Add(new Literal
                        {
                            File = _resolver.UnresolvedFile.FileName,
                            Value = ResolveValue(conversionOp.Input.ConstantValue, op1.Type.FullName),
                            StartLocation = new Location(binaryOperatorExpression.Right.StartLocation.Line, binaryOperatorExpression.Right.StartLocation.Column),
                            EndLocation = new Location(binaryOperatorExpression.Right.EndLocation.Line, binaryOperatorExpression.Right.EndLocation.Column)
                        });
                    }
                }
            }

            if (op2 != null)
            {
                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(op2.ConstantValue, op2.Type.FullName),
                    StartLocation = new Location(binaryOperatorExpression.Right.StartLocation.Line, binaryOperatorExpression.Right.StartLocation.Column),
                    EndLocation = new Location(binaryOperatorExpression.Right.EndLocation.Line, binaryOperatorExpression.Right.EndLocation.Column)
                });

                if (op1 == null)
                {
                    var conversionOp = operatorResolveResult.Operands[0] as ConversionResolveResult;

                    if (conversionOp != null && conversionOp.Conversion.IsImplicit && conversionOp.Input.IsCompileTimeConstant)
                    {
                        _literals.Add(new Literal
                        {
                            File = _resolver.UnresolvedFile.FileName,
                            Value = ResolveValue(conversionOp.Input.ConstantValue, op2.Type.FullName),
                            StartLocation = new Location(binaryOperatorExpression.Left.StartLocation.Line, binaryOperatorExpression.Left.StartLocation.Column),
                            EndLocation = new Location(binaryOperatorExpression.Left.EndLocation.Line, binaryOperatorExpression.Left.EndLocation.Column)
                        });
                    }
                }
            }

            base.VisitBinaryOperatorExpression(binaryOperatorExpression);
        }

        public override void VisitReturnStatement(ReturnStatement returnStatement)
        {
            var resolvedExpression = _resolver.Resolve(returnStatement.Expression) as ConstantResolveResult;

            if (resolvedExpression == null || resolvedExpression.ConstantValue == null)
            {
                base.VisitReturnStatement(returnStatement);
                return;
            }

            var lastNode = (AstNode)returnStatement;

            while (!(lastNode.Parent is MethodDeclaration) && !(lastNode.Parent is PropertyDeclaration) && !(lastNode.Parent is IndexerDeclaration) && !(lastNode.Parent is LambdaExpression))
                lastNode = lastNode.Parent;

            var resolved = _resolver.Resolve(lastNode.Parent);
            var resolvedLambda = resolved as LambdaResolveResult;
            var typeName = (resolvedLambda == null ? resolved.Type : resolvedLambda.ReturnType).FullName;

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(resolvedExpression.ConstantValue, typeName),
                StartLocation = new Location(returnStatement.Expression.StartLocation.Line, returnStatement.Expression.StartLocation.Column),
                EndLocation = new Location(returnStatement.Expression.EndLocation.Line, returnStatement.Expression.EndLocation.Column)
            });

            base.VisitReturnStatement(returnStatement);
        }

        public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        {
            var resolvedObjectCreation = _resolver.Resolve(objectCreateExpression) as InvocationResolveResult;

            if (resolvedObjectCreation == null)
                throw new Exception("Unknown ObjectCreateExpression resolved type.");

            var arguments = objectCreateExpression.Arguments.ToArray();

            for (var x = 0; x < arguments.Length; x++)
            {
                var resolvedArgument = resolvedObjectCreation.Arguments[x] as ConstantResolveResult;

                if (resolvedArgument == null)
                    continue;

                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = resolvedArgument.ConstantValue,
                    StartLocation = new Location(arguments[x].StartLocation.Line, arguments[x].StartLocation.Column),
                    EndLocation = new Location(arguments[x].EndLocation.Line, arguments[x].EndLocation.Column)
                });
            }

            var initializers = objectCreateExpression.Initializer.Elements.ToArray();

            for (var x = 0; x < initializers.Length; x++)
            {
                var arrayInitializer = initializers[x] as ArrayInitializerExpression;

                if (arrayInitializer == null)
                    continue;
                
                var resolvedInitializer = (InvocationResolveResult)resolvedObjectCreation.InitializerStatements[x];

                var elements = arrayInitializer.Elements.ToArray();

                for (var i = 0; i < elements.Length; i++)
                {
                    var resolvedConstant = resolvedInitializer.Arguments[i] as ConstantResolveResult;

                    if (resolvedConstant == null)
                        continue;

                    _literals.Add(new Literal
                    {
                        File = _resolver.UnresolvedFile.FileName,
                        Value = resolvedConstant.ConstantValue,
                        StartLocation = new Location(elements[i].StartLocation.Line, elements[i].StartLocation.Column),
                        EndLocation = new Location(elements[i].EndLocation.Line, elements[i].EndLocation.Column)
                    });
                }
            }

            base.VisitObjectCreateExpression(objectCreateExpression);
        }

        public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        {
            var arrayCreateResolveResult = (ArrayCreateResolveResult)_resolver.Resolve(arrayCreateExpression);

            int elCount;

            /* we can ignore multidimensional array arguments if the initializer is present */
            if (arrayCreateResolveResult.InitializerElements == null || (elCount = arrayCreateResolveResult.InitializerElements.Count) == 0)
            {
                var args = arrayCreateExpression.Arguments.ToArray();

                for (var x = 0; x < args.Length; x++)
                {
                    var resolvedArg = _resolver.Resolve(args[x]) as ConstantResolveResult;

                    if (resolvedArg == null)
                        continue;

                    _literals.Add(new Literal
                    {
                        File = _resolver.UnresolvedFile.FileName,
                        Value = ResolveValue(
                            arrayCreateResolveResult.SizeArguments[x].ConstantValue,
                            arrayCreateResolveResult.SizeArguments[x].Type.FullName),
                        StartLocation = new Location(args[x].StartLocation.Line, args[x].StartLocation.Column),
                        EndLocation = new Location(args[x].EndLocation.Line, args[x].EndLocation.Column)
                    });
                }

                base.VisitArrayCreateExpression(arrayCreateExpression);
                return;
            }

            var resolvedElements = arrayCreateResolveResult.InitializerElements;
            var allConstants = true;

            // honestly, the code would be unreadable
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var el in resolvedElements)
            {
                if (el is ConstantResolveResult)
                    continue;

                var conversionResult = el as ConversionResolveResult;

                if (conversionResult != null && (conversionResult.Input.IsCompileTimeConstant || conversionResult.Input.Type.Kind == TypeKind.Null))
                    continue;

                allConstants = false;

                break;
            }

            if (allConstants)
            {
                var dimensions = arrayCreateResolveResult.SizeArguments.Select(sizeArg => (long)(int)sizeArg.ConstantValue).ToArray();

                var typeName = ((ArrayType)arrayCreateResolveResult.Type).ElementType.FullName;
                var elementType = Type.GetType(typeName);

                if (elementType == null)
                    throw new Exception(string.Format("Unknown array element type {0}.", typeName));

                var arr = Array.CreateInstance(elementType, dimensions);
                
                for (var x = 0; x < elCount; x++)
                {
                    var indexes = GetMultidimensionalIndex(arr, x);

                    var constantElement = resolvedElements[x] as ConstantResolveResult;

                    if (constantElement != null)
                    {
                        arr.SetValue(constantElement.ConstantValue, indexes);
                        continue;
                    }

                    var conversionElement = (ConversionResolveResult)resolvedElements[x];

                    arr.SetValue(conversionElement.Input.ConstantValue, indexes);
                }

                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = arr,
                    StartLocation = new Location(arrayCreateExpression.StartLocation.Line, arrayCreateExpression.StartLocation.Column),
                    EndLocation = new Location(arrayCreateExpression.EndLocation.Line, arrayCreateExpression.EndLocation.Column)
                });
            }
            else
            {
                var argumentNodes = new List<AstNode>();
                RecursiveArrayInitializerChildGetter(arrayCreateExpression.Initializer, argumentNodes);

                for (var x = 0; x < elCount; x++)
                {
                    var resolvedConstant = resolvedElements[x] as ConstantResolveResult;

                    if (resolvedConstant != null)
                    {
                        _literals.Add(new Literal
                        {
                            File = _resolver.UnresolvedFile.FileName,
                            Value = ResolveValue(resolvedConstant.ConstantValue, resolvedConstant.Type.FullName),
                            StartLocation = new Location(argumentNodes[x].StartLocation.Line, argumentNodes[x].StartLocation.Column),
                            EndLocation = new Location(argumentNodes[x].EndLocation.Line, argumentNodes[x].EndLocation.Column)
                        });

                        continue;
                    }

                    var resolvedConversion = resolvedElements[x] as ConversionResolveResult;

                    if (resolvedConversion == null || !resolvedConversion.Input.IsCompileTimeConstant)
                        continue;

                    _literals.Add(new Literal
                    {
                        File = _resolver.UnresolvedFile.FileName,
                        Value = ResolveValue(resolvedConversion.Input.ConstantValue, resolvedConversion.Input.Type.FullName),
                        StartLocation = new Location(argumentNodes[x].StartLocation.Line, argumentNodes[x].StartLocation.Column),
                        EndLocation = new Location(argumentNodes[x].EndLocation.Line, argumentNodes[x].EndLocation.Column)
                    });
                }
            }

            base.VisitArrayCreateExpression(arrayCreateExpression);
        }

        private static long[] GetMultidimensionalIndex(Array arr, long index)
        {
            var indexes = new long[arr.Rank];
            var elementsInDimension = 1L;

            for (var x = arr.Rank - 1; x >= 0; x--)
            {
                var len = arr.GetLongLength(x);
                indexes[x] = (index / elementsInDimension) % len;
                elementsInDimension *= len;
            }

            return indexes;
        }

        private static void RecursiveArrayInitializerChildGetter(ArrayInitializerExpression arrayInitializerExpression, List<AstNode> children)
        {
            if (arrayInitializerExpression.Elements.Count == 0)
                return;

            var elements = arrayInitializerExpression.Elements.ToArray();

            if (!(elements[0] is ArrayInitializerExpression))
                children.AddRange(elements);
            else
            {
                foreach (var arr in elements.Cast<ArrayInitializerExpression>())
                    RecursiveArrayInitializerChildGetter(arr, children);
            }
        }

        public override void VisitQuerySelectClause(QuerySelectClause querySelectClause)
        {
            var resolvedQuery = _resolver.Resolve(querySelectClause) as InvocationResolveResult;

            if (resolvedQuery == null)
            {
                base.VisitQuerySelectClause(querySelectClause);
                return;
            }

            var conversionResult = resolvedQuery.Arguments[1] as ConversionResolveResult;
            Debug.Assert(conversionResult != null, "conversionResult != null");

            var lambdaResult = conversionResult.Input as LambdaResolveResult;
            Debug.Assert(lambdaResult != null, "lambdaResult != null");

            var constantResult = lambdaResult.Body as ConstantResolveResult;

            if (constantResult == null)
            {
                base.VisitQuerySelectClause(querySelectClause);
                return;
            }

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(constantResult.ConstantValue, constantResult.Type.FullName),
                StartLocation = new Location(querySelectClause.Expression.StartLocation.Line, querySelectClause.Expression.StartLocation.Column),
                EndLocation = new Location(querySelectClause.Expression.EndLocation.Line, querySelectClause.Expression.EndLocation.Column)
            });

            base.VisitQuerySelectClause(querySelectClause);
        }

        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            var resolved = ((OperatorResolveResult)_resolver.Resolve(assignmentExpression)).Operands[1] as ConstantResolveResult;

            if (resolved == null)
            {
                base.VisitAssignmentExpression(assignmentExpression);
                return;
            }

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(resolved.ConstantValue, resolved.Type.FullName),
                StartLocation = new Location(assignmentExpression.Right.StartLocation.Line, assignmentExpression.Right.StartLocation.Column),
                EndLocation = new Location(assignmentExpression.Right.EndLocation.Line, assignmentExpression.Right.EndLocation.Column)
            });

            base.VisitAssignmentExpression(assignmentExpression);
        }

        public override void VisitIndexerExpression(IndexerExpression indexerExpression)
        {
            var args = indexerExpression.Arguments.ToArray();

            ResolveResult resolved = null;

            for (var x = 0; x < args.Length; x++)
            {
                var resolvedArgEx = _resolver.Resolve(args[x]) as ConstantResolveResult;

                if (resolvedArgEx == null)
                    continue;

                if (resolved == null)
                    resolved = _resolver.Resolve(indexerExpression);

                IList<ResolveResult> resolvedArgs;

                var invocationResolveResult = resolved as CSharpInvocationResolveResult;

                if (invocationResolveResult != null)
                    resolvedArgs = invocationResolveResult.Arguments;
                else
                {
                    var arrayAccessResolveResult = resolved as ArrayAccessResolveResult;

                    if (arrayAccessResolveResult == null)
                        continue;

                    resolvedArgs = arrayAccessResolveResult.Indexes;
                }

                var resolvedArg = resolvedArgs[x];

                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(resolvedArg.ConstantValue, resolvedArg.Type.FullName),
                    StartLocation = new Location(args[x].StartLocation.Line, args[x].StartLocation.Column),
                    EndLocation = new Location(args[x].EndLocation.Line, args[x].EndLocation.Column)
                });
            }

            base.VisitIndexerExpression(indexerExpression);
        }

        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            var args = invocationExpression.Arguments.ToArray();

            var resolved = (CSharpInvocationResolveResult)_resolver.Resolve(invocationExpression);

            for (var x = 0; x < args.Length; x++)
            {
                var resolvedArg = resolved.Arguments[x] as ConstantResolveResult;

                if (resolvedArg == null)
                    continue;

                _literals.Add(new Literal
                {
                    File = _resolver.UnresolvedFile.FileName,
                    Value = ResolveValue(resolvedArg.ConstantValue, resolvedArg.Type.FullName),
                    StartLocation = new Location(args[x].StartLocation.Line, args[x].StartLocation.Column),
                    EndLocation = new Location(args[x].EndLocation.Line, args[x].EndLocation.Column)
                });
            }

            base.VisitInvocationExpression(invocationExpression);
        }

        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            var resolved = _resolver.Resolve(memberReferenceExpression.Target) as ConstantResolveResult;

            if (resolved == null)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);
                return;
            }

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(resolved.ConstantValue, resolved.Type.FullName),
                StartLocation = new Location(memberReferenceExpression.Target.StartLocation.Line, memberReferenceExpression.Target.StartLocation.Column),
                EndLocation = new Location(memberReferenceExpression.Target.EndLocation.Line, memberReferenceExpression.Target.EndLocation.Column)
            });

            base.VisitMemberReferenceExpression(memberReferenceExpression);
        }

        public override void VisitCastExpression(CastExpression castExpression)
        {
            var resolved = _resolver.Resolve(castExpression) as ConversionResolveResult;

            if (resolved == null || !resolved.Input.IsCompileTimeConstant)
            {
                base.VisitCastExpression(castExpression);
                return;
            }

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(resolved.Input.ConstantValue, resolved.Input.Type.FullName),
                StartLocation = new Location(castExpression.Expression.StartLocation.Line, castExpression.Expression.StartLocation.Column),
                EndLocation = new Location(castExpression.Expression.EndLocation.Line, castExpression.Expression.EndLocation.Column)
            });

            base.VisitCastExpression(castExpression);
        }

        public override void VisitVariableInitializer(VariableInitializer variableInitializer)
        {
            var isConstant = false;

            var fieldParent = variableInitializer.Parent as FieldDeclaration;
            if (fieldParent != null && fieldParent.Modifiers.HasFlag(Modifiers.Const))
                isConstant = true;

            var variableParent = variableInitializer.Parent as VariableDeclarationStatement;
            if (variableParent != null && variableParent.Modifiers.HasFlag(Modifiers.Const))
                isConstant = true;

            var resolvedInitializer = _resolver.Resolve(variableInitializer.Initializer);

            if (!(resolvedInitializer is ConstantResolveResult) && !(resolvedInitializer is LocalResolveResult))
            {
                base.VisitVariableInitializer(variableInitializer);
                return;
            }

            var resolved = _resolver.Resolve(variableInitializer);

            _literals.Add(new Literal
            {
                File = _resolver.UnresolvedFile.FileName,
                Value = ResolveValue(resolvedInitializer.ConstantValue, resolved.Type.FullName),
                IsConstant = isConstant,
                StartLocation = new Location(variableInitializer.Initializer.StartLocation.Line, variableInitializer.Initializer.StartLocation.Column),
                EndLocation = new Location(variableInitializer.Initializer.EndLocation.Line, variableInitializer.Initializer.EndLocation.Column)
            });

            base.VisitVariableInitializer(variableInitializer);
        }
    }
}
