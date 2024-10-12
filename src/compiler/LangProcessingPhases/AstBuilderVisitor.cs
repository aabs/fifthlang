using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ast_model.TypeSystem;

namespace compiler.LangProcessingPhases;

public static class ParserHelperExtensions
{
    public static SourceLocationMetadata CaptureMetadata(this ParserRuleContext ctx)
    {
        return new SourceLocationMetadata
        {
            Column = ctx.Start.Column,
            Line = ctx.Start.Line,
            Filename = ctx.Start.TokenSource.SourceName,
            OriginalText = ctx.GetText()
        };
    }
}
public class AstBuilderVisitor : FifthBaseVisitor<IAstThing>
{
    protected override IAstThing DefaultResult { get; }

    /*public override IAstThing Visit(IParseTree tree)
    {
        throw new NotImplementedException();
        //return base.Visit(tree);
    }*/

    public override IAstThing VisitAlias([NotNull] FifthParser.AliasContext context)
    {
        throw new NotImplementedException();
        //return base.VisitAlias(context);
    }

    public override IAstThing VisitBlock([NotNull] FifthParser.BlockContext context)
    {
        var statements = new List<Statement>();
        foreach (var e in context.statement())
        {
            var node = base.Visit(e);
            if (node is Expression exp)
            {
                node = new ExpStatement
                {
                    RHS = exp,
                    Location = e.CaptureMetadata(),
                    Parent = null,
                    Type = FifthType.VoidType,
                    Annotations = []
                };
            }

            statements.Add((Statement)node);
        }

        return new BlockStatement
        {
            Type = FifthType.VoidType,
            Statements = statements,
            Location = context.CaptureMetadata(),
            Parent = null,
            Annotations = []
        };
    }

    public override IAstThing VisitChildren(IRuleNode node)
    {
        throw new NotImplementedException();
        //return base.VisitChildren(node);
    }

    public override IAstThing VisitClass_definition(FifthParser.Class_definitionContext context)
    {
        var b = new ClassDefBuilder();
        b.WithType(TypeRegistry.DefaultRegistry.RegisterType());
        b.WithName(context.name.Text);
        foreach (var property in context._properties)
        {
            b.AddingItemToProperties((PropertyDefinition)Visit(property));
        }

        foreach (var function in context._functions)
        {
            b.AddingItemToFunctions((IFunctionDefinition)Visit(function));
        }

        var result = b.Build().CaptureLocation(context.Start);
        var userDefinedType = new UserDefinedType(result);
        TypeRegistry.DefaultRegistry.RegisterType(userDefinedType);
        result.TypeId = userDefinedType.TypeId;
        return result;
    }

    public override IAstThing VisitVariable_constraint(FifthParser.Variable_constraintContext context)
    {
        throw new NotImplementedException();
        //return base.VisitVariable_constraint(context);
    }

    public override IAstThing VisitParam_name(FifthParser.Param_nameContext context)
    {
        throw new NotImplementedException();
        //return base.VisitParam_name(context);
    }

    public override IAstThing VisitParam_type(FifthParser.Param_typeContext context)
    {
        throw new NotImplementedException();
        //return base.VisitParam_type(context);
    }

    public override IAstThing VisitStmt_ifelse(FifthParser.Stmt_ifelseContext context)
    {
        var condNode = Visit(context.condition) as Expression;
        var ifBlockEL = VisitBlock(context.ifpart) as StatementList;
        var elseBlockEL = VisitBlock(context.elsepart) as StatementList;
        var result = new IfElseStatement(new Block(ifBlockEL), new Block(elseBlockEL), condNode);
        result.TypeOfStatement = StatementType.IfElse;
        return result.CaptureLocation(context.Start);
    }

    public override IAstThing VisitStmt_while(FifthParser.Stmt_whileContext context)
    {
        var condNode = Visit(context.condition) as Expression;
        var expressionList = Visit(context.looppart) as StatementList;
        var loopBlock = new Block(expressionList);
        var result = new WhileExp(condNode, loopBlock);
        result.TypeOfStatement = StatementType.While;
        return result.CaptureLocation(context.Start);
    }

    public override IAstThing VisitStmt_with(FifthParser.Stmt_withContext context)
    {
        throw new NotImplementedException();
        //return base.VisitStmt_with(context);
    }

    public override IAstThing VisitStmt_vardecl(FifthParser.Stmt_vardeclContext context)
    {
        var decl = VisitVar_decl(context.decl) as VariableDeclarationStatement;
        if (context.exp() != null)
        {
            var exp = base.Visit(context.exp());
            decl.Expression = (Expression)exp;
        }

        decl.TypeOfStatement = StatementType.VarDecl;
        return decl.CaptureLocation(context.Start);    }

    public override IAstThing VisitStmt_assignment(FifthParser.Stmt_assignmentContext context)
    {
        var id = Visit(context.var_name()) as Identifier;
        var expression = Visit(context.exp()) as Expression;
        var varName = id.Value;
        var variableReference = new VariableReference(varName);
        var result = new AssignmentStmt(expression, variableReference);
        return result.CaptureLocation(context.Start);
    }

    public override IAstThing VisitStmt_return(FifthParser.Stmt_returnContext context)
    {
        var visitSReturn =
            new ReturnStatement((Expression)Visit(context.exp()), null).CaptureLocation(context.Start);
        visitSReturn.TypeOfStatement = StatementType.Return;
        return visitSReturn;
    }

    public override IAstThing VisitStmt_bareexpression(FifthParser.Stmt_bareexpressionContext context)
    {
        return ExpStatementBuilder.CreateExpStatement()
                                  .WithExpression((Expression)Visit(context.exp()))
                                  .Build().CaptureLocation(context.Start);

    }

    public override IAstThing VisitIdentifier_chain(FifthParser.Identifier_chainContext context)
    {
        throw new NotImplementedException();
        //return base.VisitIdentifier_chain(context);
    }

    public override IAstThing VisitExp_geq(FifthParser.Exp_geqContext context)
    {
        return BinExp(context.left, context.right, Operator.GreaterThanOrEqual).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_lt(FifthParser.Exp_ltContext context)
    {
        return BinExp(context.left, context.right, Operator.LessThan).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_leq(FifthParser.Exp_leqContext context)
    {
        return BinExp(context.left, context.right, Operator.LessThanOrEqual).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_and(FifthParser.Exp_andContext context)
    {
        return BinExp(context.left, context.right, Operator.And).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_string(FifthParser.Exp_stringContext context)
    {
        var s = context.value.Text;
        if ((s.StartsWith("\'") && s.EndsWith("\'")) || (s.StartsWith("\"") && s.EndsWith("\"")))
        {
            s = s[1..^1];
        }

        return new StringValueExpression(s).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_boolean(FifthParser.Exp_booleanContext context)
    {
        throw new NotImplementedException();
        //return base.VisitExp_boolean(context);
    }

    public override IAstThing VisitExp_bool(FifthParser.Exp_boolContext context)
    {
        throw new NotImplementedException();
        //return base.VisitExp_bool(context);
    }

    public override IAstThing VisitExp_gt(FifthParser.Exp_gtContext context)
    {
        return BinExp(context.left, context.right, Operator.GreaterThan).CaptureLocation(context.Start);
    }

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

    public override IAstThing VisitExp_double(FifthParser.Exp_doubleContext context)
    {
        return new FloatValueExpression(float.Parse(context.value.Text)).CaptureLocation(context.Start);
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

    public override IAstThing VisitExp_sub(FifthParser.Exp_subContext context)
    {
        return BinExp(context.left, context.right, Operator.Subtract).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_list(FifthParser.Exp_listContext context)
    {
        throw new NotImplementedException();
        //return base.VisitExp_list(context);
    }

    public override IAstThing VisitExp_add(FifthParser.Exp_addContext context)
    {
        return BinExp(context.left, context.right, Operator.Add).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_mul(FifthParser.Exp_mulContext context)
    {
        return BinExp(context.left, context.right, Operator.Multiply).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_paren(FifthParser.Exp_parenContext context)
    {
        return Visit(context.innerexp).CaptureLocation(context.Start);
    }

    public override IAstThing VisitExp_arithnegation(FifthParser.Exp_arithnegationContext context)
    {
        return UnExp(context.operand, Operator.Subtract).CaptureLocation(context.Start);
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
        // var resultType = TypeChecker.Infer(result.NearestScope(), result);
        // result.TypeId = resultType;
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
}
