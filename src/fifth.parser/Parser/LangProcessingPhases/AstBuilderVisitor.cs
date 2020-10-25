namespace Fifth.Parser.LangProcessingPhases
{
    using System.Collections.Generic;
    using System.Linq;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using Fifth.AST;
    using Fifth.VirtualMachine;

    public class AstBuilderVisitor : FifthBaseVisitor<IAstNode>
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected override IAstNode DefaultResult => base.DefaultResult;

        public override bool Equals(object obj) => base.Equals(obj);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => base.ToString();

        public override IAstNode Visit(IParseTree tree)
        {
            Log.Debug("Visit");
            return base.Visit(tree);
        }

        public override IAstNode VisitAlias([NotNull] FifthParser.AliasContext context)
        {
            Log.Debug("VisitAlias");
            return base.VisitAlias(context);
        }

        public override IAstNode VisitAssignmentStmt([NotNull] FifthParser.AssignmentStmtContext context)
        {
            Log.Debug("VisitAssignmentStmt");
            return base.VisitAssignmentStmt(context);
        }

        public override IAstNode VisitBlock([NotNull] FifthParser.BlockContext context)
        {
            Log.Debug("VisitBlock");
            return base.VisitBlock(context);
        }

        public override IAstNode VisitChildren(IRuleNode node) => base.VisitChildren(node);

        public override IAstNode VisitEAdd([NotNull] FifthParser.EAddContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.Plus
        };

        public override IAstNode VisitEAnd([NotNull] FifthParser.EAndContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.And
        };

        public override IAstNode VisitEDiv([NotNull] FifthParser.EDivContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.Divide
        };

        public override IAstNode VisitEDouble([NotNull] FifthParser.EDoubleContext context) => new FloatValueExpression(float.Parse(context.value.Text));

        public override IAstNode VisitEFuncCall([NotNull] FifthParser.EFuncCallContext context)
        {
            Log.Debug("VisitEFuncCall");
            return base.VisitEFuncCall(context);
        }

        public override IAstNode VisitEGEQ([NotNull] FifthParser.EGEQContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.GreaterThanOrEqual
        };

        public override IAstNode VisitEGT([NotNull] FifthParser.EGTContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.GreaterThan
        };

        public override IAstNode VisitEInt([NotNull] FifthParser.EIntContext context) => new IntValueExpression(int.Parse(context.value.Text));

        public override IAstNode VisitELEQ([NotNull] FifthParser.ELEQContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.LessThanOrEqual
        };

        public override IAstNode VisitELT([NotNull] FifthParser.ELTContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.LessThan
        };

        public override IAstNode VisitEMul([NotNull] FifthParser.EMulContext context) => new BinaryExpression
        {
            Left = (Expression)this.Visit(context.left),
            Right = (Expression)this.Visit(context.right),
            Op = Operator.Times
        };

        public override IAstNode VisitENegation([NotNull] FifthParser.ENegationContext context) => new UnaryExpression
        {
            Operand = (Expression)this.Visit(context.operand),
            Op = Operator.Not
        };

        public override IAstNode VisitEParen([NotNull] FifthParser.EParenContext context) => this.Visit(context.innerexp);

        public override IAstNode VisitErrorNode(IErrorNode node) => base.VisitErrorNode(node);

        public override IAstNode VisitEStatement([NotNull] FifthParser.EStatementContext context)
        {
            Log.Debug("VisitEStatement");
            return base.VisitEStatement(context);
        }

        public override IAstNode VisitEString([NotNull] FifthParser.EStringContext context) => new StringValueExpression(context.value.Text);

        public override IAstNode VisitESub([NotNull] FifthParser.ESubContext context)
        {
            Log.Debug("VisitESub");
            return base.VisitESub(context);
        }

        public override IAstNode VisitETypeCreate([NotNull] FifthParser.ETypeCreateContext context)
        {
            Log.Debug("VisitETypeCreate");
            return base.VisitETypeCreate(context);
        }

        public override IAstNode VisitEVarname([NotNull] FifthParser.EVarnameContext context)
            => new IdentifierExpression { Identifier = (Identifier)this.Visit(context.var_name()) };

        public override IAstNode VisitFifth([NotNull] FifthParser.FifthContext context)
        {
            Log.Debug("VisitFifth");
            return base.VisitFifth(context);
        }

        public override IAstNode VisitFormal_parameters([NotNull] FifthParser.Formal_parametersContext context)
        {
            Log.Debug("VisitFormal_parameters");
            var parameters = new List<ParameterDeclaration>();
            parameters.AddRange(
                context
                    .children
                    .Where(c => c is FifthParser.Parameter_declarationContext)
                    .Select(c => this.VisitParameter_declaration((FifthParser.Parameter_declarationContext)c))
                    .Cast<ParameterDeclaration>());
            return new ParameterDeclarationList
            {
                ParameterDeclarations = parameters
            };
        }

        public override IAstNode VisitFunction_args([NotNull] FifthParser.Function_argsContext context)
        {
            Log.Debug("VisitFunction_args");
            return base.VisitFunction_args(context);
        }

        public override IAstNode VisitFunction_body([NotNull] FifthParser.Function_bodyContext context)
        {
            Log.Debug("VisitFunction_body");
            return new ExpressionList
            {
                Expressions = context.explist()
                                    .children
                                    .Select(e => this.Visit(e))
                                    .Cast<Expression>()
                                    .ToList()
            };
        }

        public override IAstNode VisitFunction_call([NotNull] FifthParser.Function_callContext context)
        {
            Log.Debug("VisitFunction_call");
            return base.VisitFunction_call(context);
        }

        public override IAstNode VisitFunction_declaration([NotNull] FifthParser.Function_declarationContext context)
        {
            Log.Debug("VisitFunction_declaration");
            var formals = context.function_args().formal_parameters();
            var parameterList = this.VisitFormal_parameters(formals);
            var body = this.VisitFunction_body(context.function_body());
            var name = context.function_name().IDENTIFIER().GetText();
            return new FunctionDefinition
            {
                Body = body as ExpressionList,
                ParameterDeclarations = parameterList as ParameterDeclarationList,
                ReturnType = TypeHelpers.LookupBuiltinType("int"),
                Name = name
            };
        }

        public override IAstNode VisitIfElseStmt([NotNull] FifthParser.IfElseStmtContext context)
        {
            Log.Debug("VisitIfElseStmt");
            return base.VisitIfElseStmt(context);
        }

        public override IAstNode VisitIri([NotNull] FifthParser.IriContext context)
        {
            Log.Debug("VisitIri");
            return base.VisitIri(context);
        }

        public override IAstNode VisitIri_query([NotNull] FifthParser.Iri_queryContext context)
        {
            Log.Debug("VisitIri_query");
            return base.VisitIri_query(context);
        }

        public override IAstNode VisitIri_query_param([NotNull] FifthParser.Iri_query_paramContext context)
        {
            Log.Debug("VisitIri_query_param");
            return base.VisitIri_query_param(context);
        }

        public override IAstNode VisitModule_import([NotNull] FifthParser.Module_importContext context)
        {
            Log.Debug("VisitModule_import");
            return base.VisitModule_import(context);
        }

        public override IAstNode VisitModule_name([NotNull] FifthParser.Module_nameContext context)
        {
            Log.Debug("VisitModule_name");
            return base.VisitModule_name(context);
        }

        public override IAstNode VisitPackagename([NotNull] FifthParser.PackagenameContext context)
        {
            Log.Debug("VisitPackagename");
            return base.VisitPackagename(context);
        }

        public override IAstNode VisitParameter_declaration([NotNull] FifthParser.Parameter_declarationContext context)
        {
            Log.Debug("VisitParameter_declaration");
            var type = context.parameter_type().IDENTIFIER().GetText();
            var name = context.parameter_name().IDENTIFIER().GetText();
            return new ParameterDeclaration
            {
                ParameterName = name,
                ParameterType = TypeHelpers.LookupBuiltinType(type)
            };
        }

        public override IAstNode VisitParameter_name([NotNull] FifthParser.Parameter_nameContext context)
        {
            Log.Debug("VisitParameter_name");
            return base.VisitParameter_name(context);
        }

        public override IAstNode VisitParameter_type([NotNull] FifthParser.Parameter_typeContext context)
        {
            Log.Debug("VisitParameter_type");
            return base.VisitParameter_type(context);
        }

        public override IAstNode VisitTerminal(ITerminalNode node) => base.VisitTerminal(node);

        public override IAstNode VisitType_initialiser([NotNull] FifthParser.Type_initialiserContext context)
        {
            Log.Debug("VisitType_initialiser");
            return base.VisitType_initialiser(context);
        }

        public override IAstNode VisitType_name([NotNull] FifthParser.Type_nameContext context)
        {
            Log.Debug("VisitType_name");
            return new Identifier { Value = context.IDENTIFIER().GetText() };
        }

        public override IAstNode VisitType_property_init([NotNull] FifthParser.Type_property_initContext context)
        {
            Log.Debug("VisitType_property_init");
            return base.VisitType_property_init(context);
        }

        public override IAstNode VisitVar_name([NotNull] FifthParser.Var_nameContext context)
        {
            Log.Debug("VisitVar_name");
            return new Identifier { Value = context.IDENTIFIER().GetText() };
        }

        public override IAstNode VisitVarDeclStmt([NotNull] FifthParser.VarDeclStmtContext context)
        {
            Log.Debug("VisitVarDeclStmt");
            return base.VisitVarDeclStmt(context);
        }

        public override IAstNode VisitWithStmt([NotNull] FifthParser.WithStmtContext context)
        {
            Log.Debug("VisitWithStmt");
            return base.VisitWithStmt(context);
        }

        protected override IAstNode AggregateResult(IAstNode aggregate, IAstNode nextResult) => base.AggregateResult(aggregate, nextResult);

        protected override bool ShouldVisitNextChild(IRuleNode node, IAstNode currentResult) => base.ShouldVisitNextChild(node, currentResult);
    }
}