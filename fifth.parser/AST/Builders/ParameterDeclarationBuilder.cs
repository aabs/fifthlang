namespace Fifth.AST.Builders
{
    using System.Collections.Generic;

    public class ParameterDeclarationBuilder : IBuilder<ParameterDeclaration>
    {
        public List<ParameterDeclaration> ParameterDeclarations { get; private set; }

        public string ParameterName { get; private set; }
        public IFifthType ParameterType { get; private set; }
        public string TypeName { get; private set; }

        public static ParameterDeclarationBuilder Start() => new ParameterDeclarationBuilder();

        public ParameterDeclaration Build() => new ParameterDeclaration
        {
            ParameterType = this.ParameterType,
            ParameterName = this.ParameterName
        };

        public bool IsValid() => throw new System.NotImplementedException();

        public ParameterDeclarationBuilder WithName(string name)
        {
            this.ParameterName = name;
            return this;
        }

        public ParameterDeclarationBuilder WithType(IFifthType type)
        {
            this.ParameterType = type;
            return this;
        }

        public ParameterDeclarationBuilder WithTypeName(string name)
        {
            this.TypeName = name;
            return this;
        }
    }
}