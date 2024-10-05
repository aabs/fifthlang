using System.Diagnostics.CodeAnalysis;
using ast;

namespace ast_model.TypeSystem
{
    /// <summary>
    /// A framework for handling tasks related to type checking and type inference
    /// </summary>
    /// <remarks>
    /// The basic process of performing type inference is baked into the class. But what to do with
    /// the information is not. For that purpose it is possible to pass in handler functions that
    /// can be used to act on type information gleaned from the AST. For example, to annotate the
    /// type of the ast node, or to keep track of type mismatch errors etc So, where does the action
    /// take place in the process of inference? A. The task of performing inference on the type
    /// takes place in the typed Infer function for the node type. i.e. The BinaryExpression node,
    /// will own the inference for that kind of node, so it will be the first point where the type
    /// is know, so is the first place where actions can be performed. Similarly, when type
    /// checking, there is a corresponding handler that will be the first to know.
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public partial class FunctionalTypeChecker : ITypeChecker
    {
        public FunctionalTypeChecker(Action<IAstThing, IType> typeInferred,
            Action<IAstThing> typeNotFound,
            Action<IAstThing, IType, IType> typeMismatch,
            Action<IAstThing> typeNotRelevant)
        {
            TypeInferred = typeInferred;
            TypeNotFound = typeNotFound;
            TypeMismatch = typeMismatch;
            TypeNotRelevant = typeNotRelevant;
        }

        public Action<IAstThing, IType> TypeInferred { get; }
        public Action<IAstThing, IType, IType> TypeMismatch { get; }
        public Action<IAstThing> TypeNotFound { get; }
        public Action<IAstThing> TypeNotRelevant { get; }

        #region Type Inference

        /*
        public IType Infer(AstNode exp)
        {
            var scope = exp.NearestScope();
            return exp switch
            {
                TypeCast node => Infer(scope, node),
                ShortValueExpression i => PrimitiveShort.Default,
                IntValueExpression i => PrimitiveInteger.Default,
                LongValueExpression lve => PrimitiveLong.Default,
                FloatValueExpression node => PrimitiveFloat.Default,
                StringValueExpression s => PrimitiveString.Default,
                DoubleValueExpression s => PrimitiveDouble.Default,
                DecimalValueExpression s => PrimitiveDecimal.Default,
                DateValueExpression s => PrimitiveDate.Default,
                BinaryExpression be => Infer(scope, be),
                UnaryExpression ue => Infer(scope, ue),
                IdentifierExpression ie => Infer(scope, ie),
                FuncCallExpression fce => Infer(scope, fce),
                AbsoluteIri node => Infer(scope, node),
                AliasDeclaration node => Infer(scope, node),
                AssignmentStmt node => Infer(scope, node),
                Block node => Infer(scope, node),
                BooleanExpression node => Infer(scope, node),
                ExpressionList node => Infer(scope, node),
                FifthProgram node => Infer(scope, node),
                FunctionDefinition node => Infer(scope, node),
                Identifier node => Infer(scope, node),
                IfElseStatement node => Infer(scope, node),
                ModuleImport node => Infer(scope, node),
                ParameterDeclaration node => Infer(scope, node),
                ParameterDeclarationList node => Infer(scope, node),
                ScopeAstThing node => Infer(scope, node),
                TypeCreateInstExpression node => Infer(scope, node),
                TypeInitialiser node => Infer(scope, node),
                VariableDeclarationStatement node => Infer(scope, node),
                VariableReference node => Infer(scope, node),
                WhileExp node => Infer(scope, node),
                ReturnStatement node => Infer(scope, node),
                Expression node => Infer(scope, node),
                TypedAstNode node => Infer(scope, node),
                { } node => Infer(scope, node),
            };
        }

        public IType Infer(IScope scope, AbsoluteIri node)
        {
            return PrimitiveUri.Default;
        }

        public IType Infer(IScope scope, AliasDeclaration node)
        {
            return default;
        }

        public IType Infer(IScope scope, AnnotatedThing node)
        {
            return default;
        }

        public IType Infer(IScope _, AssignmentStmt node)
        {
            var scope = node.NearestScope();
            IType expType;
            var varType = Infer(scope, node.VariableRef);
            if (node.Expression != null)
            {
                expType = Infer(node.Expression);
                if (expType.DeclaringType != varType.DeclaringType)
                {
                    TypeMismatch(node, varType, expType);
                }
            }

            TypeInferred(node, varType);
            return varType;
        }

        public IType Infer(IScope scope, BinaryExpression be)
        {
            var lhsTid = Infer(be.LHS);
            var rhsTid = Infer(be.RHS);
            var (tid, lhsCoercion, rhsCoercion) =
                OperatorPrecedenceCalculator.GetResultType(be.Operator, lhsTid.DeclaringType, rhsTid.DeclaringType);
            TypeInferred(be, tid.Lookup());
            return tid.Lookup();
        }

        public IType Infer(IScope scope, Block node)
        {
            foreach (var statement in node.Statements)
            {
                Infer(statement);
            }

            TypeNotRelevant(node); // TODO: either type is not relevant, or I should not be returning default
            return default;
        }

        public IType Infer(IScope scope, BoolValueExpression node)
        {
            var result = PrimitiveBool.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, Expression node)
        {
            return Infer(node);
        }

        // throw new NotImplementedException();
        public IType Infer(IScope scope, ExpressionList node)
        {
            var types = new List<IType>();
            foreach (var exp in node.Expressions)
            {
                types.Add(Infer(exp));
                ;
            }

            // var result = types.Last(); I don't think there is a case where we either need, or can
            // use, the type of an expression list.
            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, ExpressionStatement node)
        {
            var expTid = Infer(scope, node.Expression);
            if (expTid == null)
            {
                if (node.Expression is FuncCallExpression)
                {
                    TypeNotRelevant(node);
                }
                else
                {
                    TypeNotFound(node);
                }

                return default;
            }

            TypeInferred(node, expTid);
            return expTid;
        }

        public IType Infer(IScope scope, FifthProgram node)
        {
            foreach (var classDefinition in node.Classes)
            {
                Infer(scope, classDefinition);
            }

            foreach (var functionDefinition in node.Functions)
            {
                if (functionDefinition is FunctionDefinition fd)
                {
                    Infer(scope, fd);
                }

                if (functionDefinition is OverloadedFunctionDefinition ofd)
                {
                    Infer(scope, ofd);
                }
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, FloatValueExpression node)
        {
            var result = PrimitiveFloat.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, FuncCallExpression node)
        {
            if (node.ActualParameters != null)
            {
                Infer(node.ActualParameters);
            }

            var ste = scope.Resolve(node.Name);

            switch (ste.SymbolKind)
            {
                case SymbolKind.FunctionDeclaration:
                case SymbolKind.BuiltinFunctionDeclaration:
                    node[Constants.FunctionImplementation] = ste.Context;
                    var type = Infer(ste.Context as AstNode);
                    if (type != null)
                    {
                        TypeInferred(node, type);
                        return type;
                    }

                    if (ste.Context is IFunctionDefinition fd && fd.Typename == "void")
                    {
                        TypeNotRelevant(node);
                    }
                    else
                    {
                        TypeNotFound(node);
                    }

                    return default;

                default:
                    TypeNotFound(node);
                    return default;
            }
        }

        public IType Infer(IScope _, FunctionDefinition node)
        {
            var returnType = node.ReturnType;
            Infer(node.ParameterDeclarations);
            Infer(node.Body);
            if (node.ReturnType == null || node.Typename == "void")
            {
                TypeNotRelevant(node);
                return default;
            }

            var result = returnType?.Lookup();
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, OverloadedFunctionDefinition node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, Identifier node)
        {
            if (!scope.TryResolve(node.Value, out var ste))
            {
                TypeNotFound(node);
            }

            var result = Infer(ste.Context as AstNode);
            if (result != default)
            {
                TypeInferred(node, result);
                return result;
            }

            TypeNotFound(node);
            return default;
        }

        public IType Infer(IScope _, IdentifierExpression node)
        {
            var t = Infer(node.Identifier);
            TypeInferred(node, t);
            return t;
        }

        public IType Infer(IScope scope, IfElseStatement node)
        {
            Infer(node.Condition);
            Infer(node.IfBlock);
            if (node.ElseBlock != null)
            {
                Infer(node.ElseBlock);
            }

            return default;
        }

        public IType Infer(IScope scope, IntValueExpression node)
        {
            var result = PrimitiveInteger.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, LongValueExpression node)
        {
            var result = PrimitiveLong.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, ShortValueExpression node)
        {
            var result = PrimitiveShort.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, StatementList node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, DoubleValueExpression node)
        {
            var result = PrimitiveDouble.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, DecimalValueExpression node)
        {
            var result = PrimitiveDecimal.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, DateValueExpression node)
        {
            var result = PrimitiveDate.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, StringValueExpression node)
        {
            var result = PrimitiveString.Default;
            TypeInferred(node, result);
            return result;
        }

        public IType Infer(IScope scope, ModuleImport node)
        {
            return default;
        }

        public IType Infer(IScope scope, ParameterDeclaration node)
        {
            if (TypeRegistry.DefaultRegistry.TryGetTypeByName(node.TypeName, out var t))
            {
                TypeInferred(node, t);
                Infer(node.DestructuringDecl);
                return t;
            }

            TypeNotFound(node);
            return default;
        }

        public IType Infer(IScope scope, ParameterDeclarationList node)
        {
            foreach (var e in node.ParameterDeclarations)
            {
                if (e is ParameterDeclaration pd)
                {
                    Infer(pd);
                }
                else if (e is TypeCreateInstExpression tcie)
                {
                    Infer(tcie);
                }
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, ReturnStatement node)
        {
            var result = Infer(node.SubExpression);
            if (result != null)
            {
                TypeInferred(node, result);
                node.TargetTid = result.DeclaringType;
            }
            else
            {
                TypeNotFound(node);
            }

            return result;
        }

        public IType Infer(IScope scope, Statement node)
        {
            return default;
        }

        public IType Infer(IScope scope, TypeCreateInstExpression node)
        {
            return default;
        }

        public IType Infer(IScope scope, TypeInitialiser node)
        {
            return default;
        }

        public IType Infer(IScope scope, Assembly node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, AssemblyRef node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, ClassDefinition node)
        {
            node.Properties.ForEach(propDef => Infer(node, propDef));
            node.Functions.ForEach(propDef => Infer(node, propDef));
            return node.DeclaringType.Lookup();
        }

        public IType Infer(IScope scope, FieldDefinition node)
        {
            if (TypeRegistry.DefaultRegistry.TryGetTypeByName(node.TypeName, out var t))
            {
                TypeInferred(node, t);
                return t;
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, IFunctionDefinition node)
        {
            if (node is FunctionDefinition fd)
            {
                return Infer(scope, fd);
            }

            if (node is OverloadedFunctionDefinition ofd)
            {
                return Infer(scope, ofd);
            }

            TypeNotFound(node);
            return default;
        }

        public IType Infer(IScope scope, PropertyDefinition node)
        {
            if (TypeRegistry.DefaultRegistry.TryGetTypeByName(node.TypeName, out var t))
            {
                TypeInferred(node, t);
                return t;
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, TypeCast node)
        {
            // need to check that the type can be cast to the target type...
            var subExpType = Infer(node.SubExpression);
            var coercionType = node.TargetTid.Lookup();
            // do some check that the coercion is possible
            return coercionType;
        }

        public IType Infer(IScope scope, UnaryExpression node)
        {
            return default;
        }

        public IType Infer(IScope scope, VariableDeclarationStatement node)
        {
            if (node.TypeName == null && node.UnresolvedTypeName != null)
            {
                node.TypeName = node.UnresolvedTypeName;
            }

            if (node.Expression != null)
            {
                Infer(node.Expression);
            }
            if (TypeRegistry.DefaultRegistry.TryGetTypeByName(node.TypeName, out var t))
            {
                TypeInferred(node, t);
                return t;
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, VariableReference node)
        {
            IType result = default;
            if (scope.TryResolve(node.Name, out var ste))
            {
                // if we got here, then it means we were able to resolve the target of the variable
                // reference that's useful information for later, so let's store it with the reference.
                node.SymTabEntry = ste;
                //if (ste.Context is DestructuringBinding pb)
                //{
                //    result = pb.BoundProperty.DeclaringType.Lookup();
                //    TypeInferred(node, result);
                //}

                if (ste.Context is VariableDeclarationStatement vds)
                {
                    result = vds.DeclaringType.Lookup();
                    TypeInferred(node, result);
                }

                if (ste.Context is FieldDefinition fieldDef)
                {
                    if (TypeRegistry.DefaultRegistry.TryGetTypeByName(fieldDef.TypeName, out var type))
                    {
                        fieldDef.DeclaringType = type.DeclaringType;
                    }
                    result = fieldDef.DeclaringType.Lookup();
                    TypeInferred(node, result);
                }

                if (ste.Context is PropertyDefinition propDef)
                {
                    if (TypeRegistry.DefaultRegistry.TryGetTypeByName(propDef.TypeName, out var type))
                    {
                        propDef.DeclaringType = type.DeclaringType;
                    }
                    result = propDef.DeclaringType.Lookup();
                    TypeInferred(node, result);
                }

                if (ste.Context is ParameterDeclaration paramDef)
                {
                    result = paramDef.DeclaringType.Lookup();
                    TypeInferred(node, result);
                }
            }

            return result;
        }

        public IType Infer(IScope scope, MemberAccessExpression node)
        {
            // TODO: Much more to do here...
            Infer(node.LHS);
            return Infer(node.RHS);
        }

        public IType Infer(IScope scope, WhileExp node)
        {
            return default;
        }

        public IType Infer(IScope scope, TypedAstNode node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, ScopeAstThing node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, TypePropertyInit node)
        {
            throw new NotImplementedException();
        }

        public IType Infer(IScope scope, DestructuringDeclaration node)
        {
            foreach (var binding in node.Bindings)
            {
                Infer(binding);
            }

            TypeNotRelevant(node);
            return default;
        }

        public IType Infer(IScope scope, DestructuringBinding node)
        {
            if (node.PropDecl != null)
            {
                var typeOfProp = Infer(scope, node.PropDecl);
                TypeInferred(node, typeOfProp);
                return typeOfProp;
            }
            else
            {
                // TODO:  Is there an alternative way to find the type of the bound property
                TypeNotFound(node);
            }
            return default;
        }
        */

        #endregion Type Inference

        #region Type Checking

        /*
        public void Check(IScope scope, Expression exp, IType type) { }

        public void Check(IScope scope, TypeDefinition typeDef) { }

        public void Check(IScope scope, FifthProgram prog) { }
        */

        #endregion Type Checking

        #region Helper Functions

        /*
        public IScope EmptyEnv() => default;

        public IScope Extend(IScope scope, string identifier, IType type) => default;

        public IScope Extend(IScope scope, TypeDefinition typeDef) => default;

        public IType Lookup(object o) => default;

        public IType LookupFunctionResultType(string identifier, IScope scope) =>
            throw new NotImplementedException();
        */

        #endregion Helper Functions
    }
}
