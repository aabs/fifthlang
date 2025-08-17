using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ast;
using ast_generated;
using ast_model.TypeSystem;
using Operator = ast.Operator;

namespace compiler.LangProcessingPhases;

public class AstBuilderVisitor : FifthBaseVisitor<IAstThing>
{
    public static readonly FifthType Void = new FifthType.TVoidType() { Name = TypeName.From("void") };

    #region Helper Functions

    private TAstType CreateLiteral<TAstType, TBaseType>(ParserRuleContext ctx, Func<string, TBaseType> x)
        where TAstType : LiteralExpression<TBaseType>, new()
    {
        return new TAstType()
        {
            Annotations = [],
            Location = GetLocationDetails(ctx),
            Parent = null,
            Type = new FifthType.TDotnetType(typeof(TBaseType)) { Name = TypeName.From(typeof(TBaseType).FullName) },
            Value = x(ctx.GetText())
        };
    }

    private SourceLocationMetadata GetLocationDetails(ParserRuleContext context)
    {
        var text = context.GetText();
        var len = Math.Min(text.Length, 10);
        return new SourceLocationMetadata(
            context.Start.Column,
            string.Empty,
            context.Start.Line,
            text[..len]
        );
    }

    private FifthType ResolveTypeFromName(string typeName)
    {
        // Ensure the TypeRegistry is initialized with primitive types
        TypeRegistry.DefaultRegistry.RegisterPrimitiveTypes();
        
        // Create a mapping from Fifth language type names to .NET type names
        var typeNameMapping = new Dictionary<string, string>
        {
            { "int", "Int32" },
            { "float", "Single" },
            { "double", "Double" },
            { "bool", "Boolean" },
            { "string", "String" },
            { "byte", "Byte" },
            { "char", "Char" },
            { "long", "Int64" },
            { "short", "Int16" },
            { "decimal", "Decimal" }
        };
        
        // Try to map the language type name to .NET type name
        string dotnetTypeName = typeNameMapping.TryGetValue(typeName, out string mappedName) ? mappedName : typeName;
        
        // Try to resolve the type name using the TypeRegistry
        if (TypeRegistry.DefaultRegistry.TryGetTypeByName(dotnetTypeName, out FifthType resolvedType) && resolvedType != null)
        {
            return resolvedType;
        }
        
        // Fall back to UnknownType if the type cannot be resolved
        return new FifthType.UnknownType() { Name = TypeName.From(typeName) };
    }

    #endregion Helper Functions

    public override IAstThing VisitAssignment_statement([NotNull] FifthParser.Assignment_statementContext context)
    {
        var b = new AssignmentStatementBuilder()
            .WithAnnotations([])
            .WithLValue((Expression)Visit(context.lvalue))
            .WithRValue((Expression)Visit(context.rvalue));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitBlock(FifthParser.BlockContext context)
    {
        var b = new BlockStatementBuilder()
            .WithAnnotations([]);

        foreach (var stmt in context.statement())
        {
            b.AddingItemToStatements((Statement)Visit(stmt));
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitClass_definition(FifthParser.Class_definitionContext context)
    {
        var b = new ClassDefBuilder();
        foreach (var fctx in context._functions)
        {
            var f = Visit(fctx);
            b.AddingItemToMemberDefs((MemberDef)f);
        }

        foreach (var pctx in context._properties)
        {
            var prop = Visit(pctx);
            b.AddingItemToMemberDefs((MemberDef)prop);
        }

        b.WithVisibility(Visibility.Public);
        b.WithName(TypeName.From(context.name.Text));
        b.WithAnnotations([]);
        var result = b.Build() with
        {
            Location = GetLocationDetails(context),
            Type = new FifthType.TType() { Name = TypeName.From(context.name.Text) }
        };
        return result;
    }

    public override IAstThing VisitDeclaration([NotNull] FifthParser.DeclarationContext context)
    {
        var b = new VarDeclStatementBuilder()
                        .WithAnnotations([]);
        b.WithVariableDecl((VariableDecl)VisitVar_decl(context.var_decl()));
        if (context.expression() is not null)
        {
            var exp = context.expression();
            var e = base.Visit(exp);
            b.WithInitialValue((Expression)e);
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitDestructure_binding([NotNull] FifthParser.Destructure_bindingContext context)
    {
        var b = new PropertyBindingDefBuilder()
            .WithAnnotations([])
            .WithVisibility(Visibility.Public)
            .WithIntroducedVariable(MemberName.From(context.name.Text))
            .WithReferencedPropertyName(MemberName.From(context.propname.Text));
        if (context.destructuring_decl() is not null)
        {
            b.WithDestructureDef((ParamDestructureDef)VisitDestructuring_decl(context.destructuring_decl()));
        }
        
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        
        // Set constraint if present (WithConstraint method not generated for nullable properties)
        if (context.variable_constraint() is not null)
        {
            result = result with { Constraint = (Expression)Visit(context.variable_constraint()) };
        }
        
        return result;
    }

    public override IAstThing VisitDestructuring_decl([NotNull] FifthParser.Destructuring_declContext context)
    {
        var b = new ParamDestructureDefBuilder()
            .WithAnnotations([])
            .WithVisibility(Visibility.Public);
        foreach (var pb in context._bindings)
        {
            b.AddingItemToBindings((PropertyBindingDef)VisitDestructure_binding(pb));
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_add(FifthParser.Exp_addContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.add_op.Type switch
        {
            FifthParser.PLUS => Operator.ArithmeticAdd,
            FifthParser.MINUS => Operator.ArithmeticSubtract,
            FifthParser.OR => Operator.BitwiseOr,
            FifthParser.LOGICAL_XOR => Operator.LogicalXor
        };
        b.WithOperator(op)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_and(FifthParser.Exp_andContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        b.WithOperator(Operator.LogicalAnd)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_exp(FifthParser.Exp_expContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);

        b.WithOperator(Operator.ArithmeticPow)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_member_access([NotNull] FifthParser.Exp_member_accessContext context)
    {
        var b = new MemberAccessExpBuilder()
            .WithAnnotations([]);
        b.WithLHS((Expression)Visit(context.lhs));
        if (context.rhs is not null)
            b.WithRHS((Expression)Visit(context.rhs));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_mul(FifthParser.Exp_mulContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.mul_op.Type switch
        {
            FifthParser.LSHIFT => Operator.BitwiseLeftShift,
            FifthParser.RSHIFT => Operator.BitwiseRightShift,
            FifthParser.AMPERSAND => Operator.BitwiseAnd,
            FifthParser.STAR => Operator.ArithmeticMultiply,
            FifthParser.DIV => Operator.ArithmeticDivide,
            FifthParser.MOD => Operator.ArithmeticMod,
            FifthParser.STAR_STAR => Operator.ArithmeticPow
        };
        b.WithOperator(op)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_or(FifthParser.Exp_orContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        b.WithOperator(Operator.LogicalOr)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_rel(FifthParser.Exp_relContext context)
    {
        var b = new BinaryExpBuilder()
            .WithAnnotations([]);
        var op = context.rel_op.Type switch
        {
            FifthParser.EQUALS => Operator.Equal,
            FifthParser.NOT_EQUALS => Operator.NotEqual,
            FifthParser.LESS => Operator.LessThan,
            FifthParser.LESS_OR_EQUALS => Operator.LessThanOrEqual,
            FifthParser.GREATER => Operator.GreaterThan,
            FifthParser.GREATER_OR_EQUALS => Operator.GreaterThanOrEqual
        };
        b.WithOperator(op)
            .WithLHS((Expression)Visit(context.lhs))
            .WithRHS((Expression)Visit(context.rhs))
            ;
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitExp_unary(FifthParser.Exp_unaryContext context)
    {
        var b = new UnaryExpBuilder()
            .WithAnnotations([]);
        var op = context.unary_op.Type switch
        {
            FifthParser.PLUS => Operator.ArithmeticAdd,
            FifthParser.MINUS => Operator.ArithmeticNegative,
            FifthParser.LOGICAL_NOT => Operator.LogicalNot
        };
        b.WithOperator(op)
            .WithOperand((Expression)Visit(context.expression()));

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitFifth([NotNull] FifthParser.FifthContext context)
    {
        var b = new AssemblyDefBuilder();
        b.WithVisibility(Visibility.Public)
            .WithPublicKeyToken("abc123") // TODO: need ways to define this
            .WithAnnotations([])
            .WithAssemblyRefs([])
            .WithName(AssemblyName.anonymous)
            .WithVersion("0.0.0.0")
            ;
        var mb = new ModuleDefBuilder();
        if (context._classes.Count == 0)
        {
            mb.WithClasses([]);
        }
        else
        {
            foreach (var @class in context._classes)
            {
                mb.AddingItemToClasses((ClassDef)Visit(@class));
            }
        }

        if (context._functions.Count == 0)
        {
            mb.WithFunctions([]);
        }
        else
        {
            foreach (var @func in context._functions)
            {
                mb.AddingItemToFunctions((FunctionDef)Visit(@func));
            }
        }
        b.AddingItemToModules(mb.Build());

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitFunction_declaration(FifthParser.Function_declarationContext context)
    {
        var returnType = ResolveTypeFromName(context.result_type.GetText());

        var b = new FunctionDefBuilder();
        b.WithName(MemberName.From(context.name.GetText()))
            .WithBody((BlockStatement)VisitBlock(context.body.block()))
            .WithReturnType(returnType)
            .WithAnnotations([])
            .WithVisibility(Visibility.Public) // todo: grammar needs support for member visibility
            ;
        foreach (var paramdeclContext in context.paramdecl())
        {
            b.AddingItemToParams((ParamDef)VisitParamdecl(paramdeclContext));
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitIf_statement([NotNull] FifthParser.If_statementContext context)
    {
        var b = new IfElseStatementBuilder();
        b.WithAnnotations([])
            .WithCondition((Expression)Visit(context.condition))
            .WithThenBlock((BlockStatement)Visit(context.ifpart));
        if (context.elsepart is not null)
            b.WithElseBlock((BlockStatement)Visit(context.elsepart));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitList([NotNull] FifthParser.ListContext context)
    {
        return base.VisitList_body(context.list_body());
    }

    public override IAstThing VisitList_body([NotNull] FifthParser.List_bodyContext context)
    {
        if (context.list_literal() is not null)
        {
            return VisitList_literal(context.list_literal());
        }
        else if (context.list_comprehension() is not null)
        {
            return VisitList_comprehension(context.list_comprehension());
        }
        return base.VisitList_body(context);
    }

    public override IAstThing VisitList_comprehension([NotNull] FifthParser.List_comprehensionContext context)
    {
        var b = new ListComprehensionBuilder()
            .WithAnnotations([]);
        b.WithMembershipConstraint((Expression)Visit(context.constraint))
            .WithVarName(context.var_name().GetText())
            .WithSourceName(context.source.GetText());
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitList_literal([NotNull] FifthParser.List_literalContext context)
    {
        var b = new ListLiteralBuilder()
            .WithAnnotations([]);
        foreach (var exp in context.expressionList()._expressions)
        {
            b.AddingItemToElementExpressions((Expression)Visit(exp));
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitLit_bool(FifthParser.Lit_boolContext context)
    {
        return CreateLiteral<BooleanLiteralExp, bool>(context, bool.Parse);
    }

    public override IAstThing VisitLit_float(FifthParser.Lit_floatContext context)
    {
        if (context.GetText().EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
        {
            return CreateLiteral<Float8LiteralExp, double>(context, s => double.Parse(s.Substring(0, s.Length - 1)));
        }

        if (context.GetText().EndsWith("c", StringComparison.InvariantCultureIgnoreCase))
        {
            return CreateLiteral<Float16LiteralExp, decimal>(context, decimal.Parse);
        }

        return CreateLiteral<Float4LiteralExp, float>(context, float.Parse);
    }

    public override IAstThing VisitLit_int(FifthParser.Lit_intContext context)
    {
        return CreateLiteral<Int32LiteralExp, int>(context, int.Parse);
    }

    public override IAstThing VisitLit_string(FifthParser.Lit_stringContext context)
    {
        return CreateLiteral<StringLiteralExp, string>(context, x => x);
    }

    public override IAstThing VisitNum_decimal(FifthParser.Num_decimalContext context)
    {
        return context.suffix?.Type switch
        {
            FifthParser.SUF_SHORT => CreateLiteral<Int16LiteralExp, short>(context, short.Parse),
            FifthParser.SUF_LONG => CreateLiteral<Int64LiteralExp, long>(context, long.Parse),
            _ => CreateLiteral<Int32LiteralExp, int>(context, int.Parse)
        };
    }

    public override IAstThing VisitParamdecl(FifthParser.ParamdeclContext context)
    {
        var b = new ParamDefBuilder()
                .WithVisibility(Visibility.Public)
                .WithAnnotations([])
                .WithName(context.var_name().GetText())
                .WithTypeName(TypeName.From(context.type_name().GetText()))
            ;
        if (context.destructuring_decl() is not null)
        {
            var destructuringDef = VisitDestructuring_decl(context.destructuring_decl());
            b.WithDestructureDef((ParamDestructureDef)destructuringDef);
        }

        if (context.variable_constraint() is not null)
        {
            var constraint = Visit(context.variable_constraint()) as Expression;
            if (constraint is not null)
            {
                b.WithParameterConstraint(constraint);
            }
        }

        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitProperty_declaration(FifthParser.Property_declarationContext context)
    {
        var b = new PropertyDefBuilder();
        b.WithName(MemberName.From(context.name.Text))
         .WithTypeName(TypeName.From(context.type.Text))
         .WithVisibility(Visibility.Public)
         .WithAccessConstraints([AccessConstraint.None])
         .WithIsReadOnly(false)
         .WithIsWriteOnly(false);
        // todo:  There's a lot more detail that could be filled in here, and a lot more
        // sophistication needed in the grammar of the decl
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitReturn_statement([NotNull] FifthParser.Return_statementContext context)
    {
        var b = new ReturnStatementBuilder()
            .WithAnnotations([])
            .WithReturnValue((Expression)Visit(context.expression()));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitVar_decl(FifthParser.Var_declContext context)
    {
        var b = new VariableDeclBuilder()
            .WithAnnotations([]);
        b.WithName(context.var_name().GetText());

        if (context.type_name() is not null)
        {
            b.WithTypeName(TypeName.From(context.type_name().GetText()));
            b.WithCollectionType(CollectionType.SingleInstance);
        }
        else if (context.list_type_signature() is not null)
        {
            b.WithTypeName(TypeName.From(context.list_type_signature().type_name().GetText()));
            b.WithCollectionType(CollectionType.List);
        }
        else if (context.array_type_signature() is not null)
        {
            b.WithTypeName(TypeName.From(context.array_type_signature().type_name().GetText()));
            b.WithCollectionType(CollectionType.Array);
        }
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitVar_name(FifthParser.Var_nameContext context)
    {
        var b = new VarRefExpBuilder()
            .WithVarName(context.GetText())
            .WithAnnotations([]);
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    public override IAstThing VisitVariable_constraint([NotNull] FifthParser.Variable_constraintContext context)
    {
        return base.Visit(context.constraint);
    }

    public override IAstThing VisitWhile_statement([NotNull] FifthParser.While_statementContext context)
    {
        var b = new WhileStatementBuilder();
        b.WithAnnotations([])
            .WithCondition((Expression)Visit(context.condition))
            .WithBody((BlockStatement)Visit(context.looppart));
        var result = b.Build() with { Location = GetLocationDetails(context), Type = Void };
        return result;
    }

    protected override IAstThing DefaultResult { get; }
    /*

public override IAstThing VisitExp_memberaccess(FifthParser.Exp_memberaccessContext context)
{
// the member access '.' operator is left associative...

var builder = MemberAccessExpressionBuilder.CreateMemberAccessExpression();
var segments = from s in context.member_access_expression()._segments select Visit(s) as Expression;
if (segments.Count() < 2)
{
  return segments.First();
}
var e = segments.Reverse().GetEnumerator();
e.MoveNext(); // safe, since we know there is at least 2
var firstElement = e.Current;
Expression result = firstElement;
while (e.MoveNext())
{
  result = new MemberAccessExpression(e.Current, result);
}
return result!;
}

public override IAstThing VisitExp_typecreateinst(FifthParser.Exp_typecreateinstContext context)
{
var typeInitialiser = context.type_initialiser();
var propertyInits = from x in typeInitialiser._properties
                  select VisitType_property_init(x);

var result = new TypeInitialiser(context.type_initialiser().typename.GetText(),
  propertyInits.Cast<TypePropertyInit>().ToList());
return result;
}

public override IAstThing VisitExp_list(FifthParser.Exp_listContext context)
{
throw new NotImplementedException();
//return base.VisitExp_list(context);
}

public override IAstThing VisitExp_paren(FifthParser.Exp_parenContext context)
{
return Visit(context.innerexp).CaptureLocation(context.Start);
}

public override IAstThing VisitExp_varname(FifthParser.Exp_varnameContext context)
{
var id = context.var_name().GetText();
return
  new VariableReference(id)
      .CaptureLocation(context.Start);
}

public override IAstThing VisitExp_typecast(FifthParser.Exp_typecastContext context)
{
var sexp = Visit(context.subexp) as Expression;
if (TypeRegistry.DefaultRegistry.TryGetTypeByName(context.type.GetText(), out var type))
{
  return new TypeCast(sexp, type.TypeId).CaptureLocation(context.Start);
}

throw new TypeCheckingException("Unable to find target type for cast");
}

public override IAstThing VisitExp_int(FifthParser.Exp_intContext context)
{
return new IntValueExpression(int.Parse(context.value.Text)).CaptureLocation(context.Start);
}

public override IAstThing VisitExp_funccall(FifthParser.Exp_funccallContext context)
{
var name = context.funcname.GetText();
var actualParams = (ExpressionList)VisitExplist(context.args);
return new FuncCallExpression(actualParams, name)
  .CaptureLocation(context.Start);
}

public override IAstThing VisitExp_logicnegation(FifthParser.Exp_logicnegationContext context)
{
return UnExp(context.operand, Operator.Not).CaptureLocation(context.Start);
}

public override IAstThing VisitExp_div(FifthParser.Exp_divContext context)
{
return BinExp(context.left, context.right, Operator.Divide).CaptureLocation(context.Start);
}

public override IAstThing VisitAbsoluteIri(FifthParser.AbsoluteIriContext context)
{
throw new NotImplementedException();
//return base.VisitAbsoluteIri(context);
}

public override IAstThing VisitQNameIri(FifthParser.QNameIriContext context)
{
throw new NotImplementedException();
//return base.VisitQNameIri(context);
}

public override IAstThing VisitList(FifthParser.ListContext context)
{
throw new NotImplementedException();
//return base.VisitList(context);
}

public override IAstThing VisitEListLiteral(FifthParser.EListLiteralContext context)
{
throw new NotImplementedException();
//return base.VisitEListLiteral(context);
}

public override IAstThing VisitEListComprehension(FifthParser.EListComprehensionContext context)
{
throw new NotImplementedException();
//return base.VisitEListComprehension(context);
}

public override IAstThing VisitList_comp_constraint(FifthParser.List_comp_constraintContext context)
{
throw new NotImplementedException();
//return base.VisitList_comp_constraint(context);
}

public override IAstThing VisitList_comp_generator(FifthParser.List_comp_generatorContext context)
{
throw new NotImplementedException();
//return base.VisitList_comp_generator(context);
}

public override IAstThing VisitList_literal(FifthParser.List_literalContext context)
{
throw new NotImplementedException();
//return base.VisitList_literal(context);
}

public override IAstThing VisitList_comprehension(FifthParser.List_comprehensionContext context)
{
throw new NotImplementedException();
//return base.VisitList_comprehension(context);
}

public override IAstThing VisitList_type_signature(FifthParser.List_type_signatureContext context)
{
throw new NotImplementedException();
//return base.VisitList_type_signature(context);
}

public override bool Equals(object obj)
{
return base.Equals(obj);
}

public override int GetHashCode()
{
return base.GetHashCode();
}

public override string ToString()
{
return base.ToString();
}

public override IAstThing VisitErrorNode(IErrorNode node)
{
throw new NotImplementedException();
//return base.VisitErrorNode(node);
}

public override IAstThing VisitExplist([NotNull] FifthParser.ExplistContext context)
{
if (context == null)
{
  return null;
}

var exps = new List<Expression>();
foreach (var e in context.exp())
{
  exps.Add((Expression)base.Visit(e));
}

return new ExpressionList(exps).CaptureLocation(context.Start);
}

public override IAstThing VisitFifth([NotNull] FifthParser.FifthContext context)
{
var b = FifthProgramBuilder.CreateFifthProgram();
foreach (var @class in context._classes)
{
  b.AddingItemToClasses((ClassDefinition)Visit(@class));
}

foreach (var function in context._functions)
{
  b.AddingItemToFunctions((IFunctionDefinition)Visit(function));
}

foreach (var aliasContext in context.alias())
{
  b.AddingItemToAliases((AliasDeclaration)Visit(aliasContext));
}

var result = b.Build().CaptureLocation(context.Start);
result.TargetAssemblyFileName = Path.GetFileName(Path.ChangeExtension(result.Filename, "exe"));
return result;
}

public override IAstThing VisitFunction_body([NotNull] FifthParser.Function_bodyContext context)
{
return VisitBlock(context.block()).CaptureLocation(context.Start);
}

public override IAstThing VisitFunction_call([NotNull] FifthParser.Function_callContext context)
{
throw new NotImplementedException();
//return base.VisitFunction_call(context).CaptureLocation(context.Start);
}

public override IAstThing VisitFunction_declaration([NotNull] FifthParser.Function_declarationContext context)
{
var fb = FunctionDefinitionBuilder.CreateFunctionDefinition();
var segments = from seg in context.name.identifier_chain()._segments
             select seg.Text;
var name = string.Join('.', segments);
fb.WithName(name)
.WithFunctionKind(FunctionKind.Normal)
.WithBody((Block)Visit(context.function_body()))
.WithTypename(context.result_type.GetText());

var pdlb = ParameterDeclarationListBuilder.CreateParameterDeclarationList();
foreach (var arg in context._args)
{
  pdlb.AddingItemToParameterDeclarations((IParameterListItem)Visit(arg));
}

fb.WithParameterDeclarations(pdlb.Build());

return fb.Build().CaptureLocation(context.Start);
}

public override IAstThing VisitIri([NotNull] FifthParser.IriContext context)
{
throw new NotImplementedException();
//return base.VisitIri(context).CaptureLocation(context.Start);
}

public override IAstThing VisitIri_query_param([NotNull] FifthParser.Iri_query_paramContext context)
{
throw new NotImplementedException();
//return base.VisitIri_query_param(context).CaptureLocation(context.Start);
}

public override IAstThing VisitModule_import([NotNull] FifthParser.Module_importContext context)
{
throw new NotImplementedException();
//return base.VisitModule_import(context).CaptureLocation(context.Start);
}

public override IAstThing VisitModule_name([NotNull] FifthParser.Module_nameContext context)
{
throw new NotImplementedException();
//return base.VisitModule_name(context).CaptureLocation(context.Start);
}

public override IAstThing VisitPackagename([NotNull] FifthParser.PackagenameContext context)
{
throw new NotImplementedException();
//return base.VisitPackagename(context).CaptureLocation(context.Start);
}

public override IAstThing VisitParamdecl([NotNull] FifthParser.ParamdeclContext context)
{
var builder = ParameterDeclarationBuilder.CreateParameterDeclaration()
                                       .WithParameterName(new Identifier(context.param_name().IDENTIFIER()
                                           .GetText()))
                                       .WithTypeName(context.param_type().GetText());

if (context.variable_constraint() != null)
{
  builder.WithConstraint((Expression)Visit(context.variable_constraint()));
}

if (context.destructuring_decl() != null)
{
  builder.WithDestructuringDecl((DestructuringDeclaration)Visit(context.destructuring_decl()));
}

return builder.Build().CaptureLocation(context.Start);
}

public override IAstThing VisitDestructure_binding([NotNull] FifthParser.Destructure_bindingContext context)
{
var builder = DestructuringBindingBuilder.CreateDestructuringBinding().WithVarname(context.name.Text)
                                       .WithPropname(context.propname.Text);
if (context.destructuring_decl() != null)
{
  builder.WithDestructuringDecl((DestructuringDeclaration)Visit(context.destructuring_decl()));
}

return builder.Build().CaptureLocation(context.Start);
}

public override IAstThing VisitDestructuring_decl([NotNull] FifthParser.Destructuring_declContext context)
{
var builder = DestructuringDeclarationBuilder.CreateDestructuringDeclaration();
foreach (var binding in context._bindings)
{
  builder.AddingItemToBindings((DestructuringBinding)Visit(binding));
}

return builder.Build().CaptureLocation(context.Start);
}

public override IAstThing VisitProperty_declaration(FifthParser.Property_declarationContext context)
{
return PropertyDefinitionBuilder.CreatePropertyDefinition()
                              .WithName(context.name.Text)
                              .WithTypeName(context.type.Text)
                              .Build();
}

public override IAstThing VisitTerminal(ITerminalNode node)
{
throw new NotImplementedException();
//return base.VisitTerminal(node);
}

public override IAstThing VisitTruth_value(FifthParser.Truth_valueContext context)
{
return new BoolValueExpression(bool.Parse(context.value.Text)).CaptureLocation(context.Start);
}

public override IAstThing VisitType_initialiser([NotNull] FifthParser.Type_initialiserContext context)
{
throw new NotImplementedException();
//return base.VisitType_initialiser(context).CaptureLocation(context.Start);
}

public override IAstThing VisitType_name([NotNull] FifthParser.Type_nameContext context)
{
return new Identifier(context.IDENTIFIER().GetText()).CaptureLocation(context.Start);
}

public override IAstThing VisitType_property_init([NotNull] FifthParser.Type_property_initContext context)
{
var exp = Visit(context.exp()) as Expression;
return new TypePropertyInit(context.var_name().GetText(), exp);
}

public override IAstThing VisitVar_decl([NotNull] FifthParser.Var_declContext context)
{
var builder = VariableDeclarationStatementBuilder.CreateVariableDeclarationStatement()
                                               .WithName(context.var_name().GetText())
                                               .WithUnresolvedTypeName(context.type_name().GetText());
var result = builder.Build();
result.TypeName = result.UnresolvedTypeName;
return result.CaptureLocation(context.Start);
}

public override IAstThing VisitVar_name([NotNull] FifthParser.Var_nameContext context)
{
return new Identifier(context.GetText()).CaptureLocation(context.Start);
}

protected override IAstThing AggregateResult(IAstThing aggregate, IAstThing nextResult)
{
return base.AggregateResult(aggregate, nextResult);
}

protected override bool ShouldVisitNextChild(IRuleNode node, IAstThing currentResult)
{
return base.ShouldVisitNextChild(node, currentResult);
}

private BinaryExpression BinExp(FifthParser.ExpContext left, FifthParser.ExpContext right, Operator op)
{
var astLeft = (Expression)Visit(left).CaptureLocation(left.Start);
var astRight = (Expression)Visit(right).CaptureLocation(right.Start);
return new BinaryExpression(astLeft, op, astRight);
}

private UnaryExpression UnExp(FifthParser.ExpContext operand, Operator op)
{
var astOperand = (Expression)Visit(operand);
var result = new UnaryExpression(astOperand, op);
// var resultType = TypeChecker.Infer(result.NearestScope(), result); result.TypeId = resultType;
return result.CaptureLocation(operand.Start);
}

public override IAstThing VisitExp_callsite_varname(FifthParser.Exp_callsite_varnameContext context)
{
throw new NotImplementedException();
//return base.VisitExp_callsite_varname(context);
}

public override IAstThing VisitExp_callsite_func_call(FifthParser.Exp_callsite_func_callContext context)
{
throw new NotImplementedException();
//return base.VisitExp_callsite_func_call(context);
}

public override IAstThing VisitExp_callsite_parenthesised(FifthParser.Exp_callsite_parenthesisedContext context)
{
throw new NotImplementedException();
//return base.VisitExp_callsite_parenthesised(context);
}

public override IAstThing VisitMember_access_expression(
[NotNull] FifthParser.Member_access_expressionContext context)
{
// the member access '.' operator is left associative...

var builder = MemberAccessExpressionBuilder.CreateMemberAccessExpression();
var segments = from s in context._segments select Visit(s) as Expression;
if (segments.Count() < 2)
{
  return segments.First();
}

var e = segments.Reverse().GetEnumerator();
e.MoveNext(); // safe, since we know there is at least 2
var firstElement = e.Current;
var result = firstElement;
while (e.MoveNext())
{
  result = new MemberAccessExpression(e.Current, result);
}

return result!;
}
*/
}
