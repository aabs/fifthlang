namespace Fifth.AST.Builders
{
    using System.Collections.Generic;

    public class FunctionBuilder : IBuilder<FunctionDefinition>
    {
        public FunctionBuilder()
        {
        }

        public ExpressionList Body { get; private set; }
        public string FunctionName { get; private set; }
        public IFifthType ReturnType { get; private set; }
        internal ParameterDeclarationList ParameterDeclarations { get; private set; }

        public static FunctionBuilder Start() => new FunctionBuilder();

        public FunctionDefinition Build()
        {
            if (!this.IsValid())
            {
                throw new System.Exception("Attempted to build an invalid function definition");
            }

            return new FunctionDefinition
            {
                Name = this.FunctionName,
                ReturnType = this.ReturnType,
                Body = this.Body
            };
        }

        public bool IsValid() => !string.IsNullOrWhiteSpace(this.FunctionName)
                && this.ReturnType != null
                && this.Body != null;

        public FunctionBuilder WithBody(List<Expression> expressions)
        {
            this.Body = new ExpressionList { Expressions = expressions };
            return this;
        }

        public FunctionBuilder WithName(string functionName)
        {
            this.FunctionName = functionName;
            return this;
        }

        public FunctionBuilder WithParameters(ParameterDeclarationList parameterDeclarations)
        {
            this.ParameterDeclarations = parameterDeclarations;
            return this;
        }

        public FunctionBuilder WithReturnType(IFifthType returnType)
        {
            this.ReturnType = returnType;
            return this;
        }
    }
}