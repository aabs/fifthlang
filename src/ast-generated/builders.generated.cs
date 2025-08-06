namespace ast_generated;
using ast_generated;
using ast;
using ast_model.TypeSystem;
using System.Collections.Generic;
#nullable disable

public class FunctionDefBuilder : IBuilder<ast.FunctionDef>
{
    private List<ParamDef> _Params;
    private BlockStatement _Body;
    private FifthType _ReturnType;
    private MemberName _Name;
    private bool _IsStatic;
    private bool _IsConstructor;

    public ast.FunctionDef Build()
    {
        return new ast.FunctionDef()
        {
            Params = this._Params,
            Body = this._Body,
            ReturnType = this._ReturnType,
            Name = this._Name,
            IsStatic = this._IsStatic,
            IsConstructor = this._IsConstructor
        };
    }
    public FunctionDefBuilder WithParams(List<ParamDef> value)
    {
        _Params = value;
        return this;
    }

    public FunctionDefBuilder AddingItemToParams(ParamDef value)
    {
        _Params ??= new List<ParamDef>();
        _Params.Add(value);
        return this;
    }

    public FunctionDefBuilder WithBody(BlockStatement value)
    {
        _Body = value;
        return this;
    }

    public FunctionDefBuilder WithReturnType(FifthType value)
    {
        _ReturnType = value;
        return this;
    }

    public FunctionDefBuilder WithName(MemberName value)
    {
        _Name = value;
        return this;
    }

    public FunctionDefBuilder WithIsStatic(bool value)
    {
        _IsStatic = value;
        return this;
    }

    public FunctionDefBuilder WithIsConstructor(bool value)
    {
        _IsConstructor = value;
        return this;
    }

}

public class FunctorDefBuilder : IBuilder<ast.FunctorDef>
{
    private FunctionDef _InvocationFuncDev;

    public ast.FunctorDef Build()
    {
        return new ast.FunctorDef()
        {
            InvocationFuncDev = this._InvocationFuncDev
        };
    }
    public FunctorDefBuilder WithInvocationFuncDev(FunctionDef value)
    {
        _InvocationFuncDev = value;
        return this;
    }

}

public class MethodDefBuilder : IBuilder<ast.MethodDef>
{
    private FunctionDef _FunctionDef;

    public ast.MethodDef Build()
    {
        return new ast.MethodDef()
        {
            FunctionDef = this._FunctionDef
        };
    }
    public MethodDefBuilder WithFunctionDef(FunctionDef value)
    {
        _FunctionDef = value;
        return this;
    }

}

public class InferenceRuleDefBuilder : IBuilder<ast.InferenceRuleDef>
{
    private Expression _Antecedent;
    private KnowledgeManagementBlock _Consequent;

    public ast.InferenceRuleDef Build()
    {
        return new ast.InferenceRuleDef()
        {
            Antecedent = this._Antecedent,
            Consequent = this._Consequent
        };
    }
    public InferenceRuleDefBuilder WithAntecedent(Expression value)
    {
        _Antecedent = value;
        return this;
    }

    public InferenceRuleDefBuilder WithConsequent(KnowledgeManagementBlock value)
    {
        _Consequent = value;
        return this;
    }

}

public class KnowledgeManagementBlockBuilder : IBuilder<ast.KnowledgeManagementBlock>
{
    private List<KnowledgeManagementStatement> _Statements;

    public ast.KnowledgeManagementBlock Build()
    {
        return new ast.KnowledgeManagementBlock()
        {
            Statements = this._Statements
        };
    }
    public KnowledgeManagementBlockBuilder WithStatements(List<KnowledgeManagementStatement> value)
    {
        _Statements = value;
        return this;
    }

    public KnowledgeManagementBlockBuilder AddingItemToStatements(KnowledgeManagementStatement value)
    {
        _Statements ??= new List<KnowledgeManagementStatement>();
        _Statements.Add(value);
        return this;
    }

}

public class ExpStatementBuilder : IBuilder<ast.ExpStatement>
{
    private Expression _RHS;

    public ast.ExpStatement Build()
    {
        return new ast.ExpStatement()
        {
            RHS = this._RHS
        };
    }
    public ExpStatementBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

}

public class GuardStatementBuilder : IBuilder<ast.GuardStatement>
{
    private Expression _Condition;

    public ast.GuardStatement Build()
    {
        return new ast.GuardStatement()
        {
            Condition = this._Condition
        };
    }
    public GuardStatementBuilder WithCondition(Expression value)
    {
        _Condition = value;
        return this;
    }

}

public class AssertionStatementBuilder : IBuilder<ast.AssertionStatement>
{
    private Triple _Assertion;
    private AssertionSubject _AssertionSubject;
    private AssertionPredicate _AssertionPredicate;
    private AssertionObject _AssertionObject;

    public ast.AssertionStatement Build()
    {
        return new ast.AssertionStatement()
        {
            Assertion = this._Assertion,
            AssertionSubject = this._AssertionSubject,
            AssertionPredicate = this._AssertionPredicate,
            AssertionObject = this._AssertionObject
        };
    }
    public AssertionStatementBuilder WithAssertion(Triple value)
    {
        _Assertion = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionSubject(AssertionSubject value)
    {
        _AssertionSubject = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionPredicate(AssertionPredicate value)
    {
        _AssertionPredicate = value;
        return this;
    }

    public AssertionStatementBuilder WithAssertionObject(AssertionObject value)
    {
        _AssertionObject = value;
        return this;
    }

}

public class AssertionObjectBuilder : IBuilder<ast.AssertionObject>
{

    public ast.AssertionObject Build()
    {
        return new ast.AssertionObject()
        {
        };
    }
}

public class AssertionPredicateBuilder : IBuilder<ast.AssertionPredicate>
{

    public ast.AssertionPredicate Build()
    {
        return new ast.AssertionPredicate()
        {
        };
    }
}

public class AssertionSubjectBuilder : IBuilder<ast.AssertionSubject>
{

    public ast.AssertionSubject Build()
    {
        return new ast.AssertionSubject()
        {
        };
    }
}

public class LambdaExpBuilder : IBuilder<ast.LambdaExp>
{
    private FunctorDef _FunctorDef;

    public ast.LambdaExp Build()
    {
        return new ast.LambdaExp()
        {
            FunctorDef = this._FunctorDef
        };
    }
    public LambdaExpBuilder WithFunctorDef(FunctorDef value)
    {
        _FunctorDef = value;
        return this;
    }

}

public class Int8LiteralExpBuilder : IBuilder<ast.Int8LiteralExp>
{

    public ast.Int8LiteralExp Build()
    {
        return new ast.Int8LiteralExp()
        {
        };
    }
}

public class Int16LiteralExpBuilder : IBuilder<ast.Int16LiteralExp>
{

    public ast.Int16LiteralExp Build()
    {
        return new ast.Int16LiteralExp()
        {
        };
    }
}

public class Int32LiteralExpBuilder : IBuilder<ast.Int32LiteralExp>
{

    public ast.Int32LiteralExp Build()
    {
        return new ast.Int32LiteralExp()
        {
        };
    }
}

public class Int64LiteralExpBuilder : IBuilder<ast.Int64LiteralExp>
{

    public ast.Int64LiteralExp Build()
    {
        return new ast.Int64LiteralExp()
        {
        };
    }
}

public class UnsignedInt8LiteralExpBuilder : IBuilder<ast.UnsignedInt8LiteralExp>
{

    public ast.UnsignedInt8LiteralExp Build()
    {
        return new ast.UnsignedInt8LiteralExp()
        {
        };
    }
}

public class UnsignedInt16LiteralExpBuilder : IBuilder<ast.UnsignedInt16LiteralExp>
{

    public ast.UnsignedInt16LiteralExp Build()
    {
        return new ast.UnsignedInt16LiteralExp()
        {
        };
    }
}

public class UnsignedInt32LiteralExpBuilder : IBuilder<ast.UnsignedInt32LiteralExp>
{

    public ast.UnsignedInt32LiteralExp Build()
    {
        return new ast.UnsignedInt32LiteralExp()
        {
        };
    }
}

public class UnsignedInt64LiteralExpBuilder : IBuilder<ast.UnsignedInt64LiteralExp>
{

    public ast.UnsignedInt64LiteralExp Build()
    {
        return new ast.UnsignedInt64LiteralExp()
        {
        };
    }
}

public class Float4LiteralExpBuilder : IBuilder<ast.Float4LiteralExp>
{

    public ast.Float4LiteralExp Build()
    {
        return new ast.Float4LiteralExp()
        {
        };
    }
}

public class Float8LiteralExpBuilder : IBuilder<ast.Float8LiteralExp>
{

    public ast.Float8LiteralExp Build()
    {
        return new ast.Float8LiteralExp()
        {
        };
    }
}

public class Float16LiteralExpBuilder : IBuilder<ast.Float16LiteralExp>
{

    public ast.Float16LiteralExp Build()
    {
        return new ast.Float16LiteralExp()
        {
        };
    }
}

public class CharLiteralExpBuilder : IBuilder<ast.CharLiteralExp>
{

    public ast.CharLiteralExp Build()
    {
        return new ast.CharLiteralExp()
        {
        };
    }
}

public class StringLiteralExpBuilder : IBuilder<ast.StringLiteralExp>
{

    public ast.StringLiteralExp Build()
    {
        return new ast.StringLiteralExp()
        {
        };
    }
}

public class DateLiteralExpBuilder : IBuilder<ast.DateLiteralExp>
{

    public ast.DateLiteralExp Build()
    {
        return new ast.DateLiteralExp()
        {
        };
    }
}

public class TimeLiteralExpBuilder : IBuilder<ast.TimeLiteralExp>
{

    public ast.TimeLiteralExp Build()
    {
        return new ast.TimeLiteralExp()
        {
        };
    }
}

public class DateTimeLiteralExpBuilder : IBuilder<ast.DateTimeLiteralExp>
{

    public ast.DateTimeLiteralExp Build()
    {
        return new ast.DateTimeLiteralExp()
        {
        };
    }
}

public class DurationLiteralExpBuilder : IBuilder<ast.DurationLiteralExp>
{

    public ast.DurationLiteralExp Build()
    {
        return new ast.DurationLiteralExp()
        {
        };
    }
}

public class UriLiteralExpBuilder : IBuilder<ast.UriLiteralExp>
{

    public ast.UriLiteralExp Build()
    {
        return new ast.UriLiteralExp()
        {
        };
    }
}

public class AtomLiteralExpBuilder : IBuilder<ast.AtomLiteralExp>
{

    public ast.AtomLiteralExp Build()
    {
        return new ast.AtomLiteralExp()
        {
        };
    }
}

public class MemberAccessExpBuilder : IBuilder<ast.MemberAccessExp>
{
    private Expression _LHS;
    private Expression? _RHS;

    public ast.MemberAccessExp Build()
    {
        return new ast.MemberAccessExp()
        {
            LHS = this._LHS,
            RHS = this._RHS
        };
    }
    public MemberAccessExpBuilder WithLHS(Expression value)
    {
        _LHS = value;
        return this;
    }

    public MemberAccessExpBuilder WithRHS(Expression? value)
    {
        _RHS = value;
        return this;
    }

}

public class ObjectInitializerExpBuilder : IBuilder<ast.ObjectInitializerExp>
{
    private FifthType _TypeToInitialize;
    private List<PropertyInitializerExp> _PropertyInitialisers;

    public ast.ObjectInitializerExp Build()
    {
        return new ast.ObjectInitializerExp()
        {
            TypeToInitialize = this._TypeToInitialize,
            PropertyInitialisers = this._PropertyInitialisers
        };
    }
    public ObjectInitializerExpBuilder WithTypeToInitialize(FifthType value)
    {
        _TypeToInitialize = value;
        return this;
    }

    public ObjectInitializerExpBuilder WithPropertyInitialisers(List<PropertyInitializerExp> value)
    {
        _PropertyInitialisers = value;
        return this;
    }

    public ObjectInitializerExpBuilder AddingItemToPropertyInitialisers(PropertyInitializerExp value)
    {
        _PropertyInitialisers ??= new List<PropertyInitializerExp>();
        _PropertyInitialisers.Add(value);
        return this;
    }

}

public class PropertyInitializerExpBuilder : IBuilder<ast.PropertyInitializerExp>
{
    private PropertyRef _PropertyToInitialize;
    private Expression _RHS;

    public ast.PropertyInitializerExp Build()
    {
        return new ast.PropertyInitializerExp()
        {
            PropertyToInitialize = this._PropertyToInitialize,
            RHS = this._RHS
        };
    }
    public PropertyInitializerExpBuilder WithPropertyToInitialize(PropertyRef value)
    {
        _PropertyToInitialize = value;
        return this;
    }

    public PropertyInitializerExpBuilder WithRHS(Expression value)
    {
        _RHS = value;
        return this;
    }

}

public class UnaryExpBuilder : IBuilder<ast.UnaryExp>
{
    private Operator _Operator;
    private Expression _Operand;

    public ast.UnaryExp Build()
    {
        return new ast.UnaryExp()
        {
            Operator = this._Operator,
            Operand = this._Operand
        };
    }
    public UnaryExpBuilder WithOperator(Operator value)
    {
        _Operator = value;
        return this;
    }

    public UnaryExpBuilder WithOperand(Expression value)
    {
        _Operand = value;
        return this;
    }

}

public class VarRefExpBuilder : IBuilder<ast.VarRefExp>
{
    private string _VarName;
    private VariableDecl _VariableDecl;

    public ast.VarRefExp Build()
    {
        return new ast.VarRefExp()
        {
            VarName = this._VarName,
            VariableDecl = this._VariableDecl
        };
    }
    public VarRefExpBuilder WithVarName(string value)
    {
        _VarName = value;
        return this;
    }

    public VarRefExpBuilder WithVariableDecl(VariableDecl value)
    {
        _VariableDecl = value;
        return this;
    }

}

public class ListLiteralBuilder : IBuilder<ast.ListLiteral>
{
    private List<Expression> _ElementExpressions;

    public ast.ListLiteral Build()
    {
        return new ast.ListLiteral()
        {
            ElementExpressions = this._ElementExpressions
        };
    }
    public ListLiteralBuilder WithElementExpressions(List<Expression> value)
    {
        _ElementExpressions = value;
        return this;
    }

    public ListLiteralBuilder AddingItemToElementExpressions(Expression value)
    {
        _ElementExpressions ??= new List<Expression>();
        _ElementExpressions.Add(value);
        return this;
    }

}

#nullable restore
