namespace ast_model.TypeSystem.Inference
{
    // BEWARE: this is prototype code that has not yet been aligned with the rest of the type system. It
    //         is not usable as is.

    public abstract record BaseType;

    public record Type(string Name) : BaseType;
    public record Arrow(string Name, BaseType InputType, BaseType OutputType) : BaseType;

    public interface ITypeSystem
    {
        List<Arrow> Arrows { get; }
        List<Type> Types { get; }

        BaseType InferResultType(BaseType[] inputTypes, string @operator);

        TypeSystem WithFunction(BaseType[] inputTypes, BaseType outputType, string op);

        TypeSystem WithOperation(BaseType tInLHS, BaseType tInRHS, BaseType tOut, string operation);

        TypeSystem WithType(Type t);
    }

    public class TypeSystem : ITypeSystem
    {
        public List<Arrow> Arrows { get; } = [];

        public List<Type> Types { get; } = [];

        public Arrow Build(BaseType[] inputTypes, BaseType outputType, string op)
        {
            var result = inputTypes switch
            {
            [] => new Arrow(op, new Type("void"), outputType),
            [BaseType t] => new Arrow(op, t, outputType),
            [BaseType t1, ..] => new Arrow(op, t1, Build(inputTypes[1..], outputType, "")),
            };
            return result;
        }

        public BaseType InferResultType(BaseType[] inputTypes, string @operator)
        {
            return InferResultType(inputTypes, @operator, Arrows);
        }

        public TypeSystem WithFunction(BaseType[] inputTypes, BaseType outputType, string op)
        {
            Arrows.Add(Build(inputTypes, outputType, op));
            return this;
        }

        public TypeSystem WithOperation(BaseType tInLHS, BaseType tInRHS, BaseType tOut, string operation)
        {
            Arrows.Add(new Arrow(operation, tInLHS, new Arrow("", tInRHS, tOut)));
            return this;
        }

        public TypeSystem WithType(Type t)
        {
            Types.Add(t);
            return this;
        }

        private BaseType InferResultType(BaseType[] inputTypes, string @operator, List<Arrow> arrows)
        {
            if (inputTypes.Length == 0)
            {
                return InferResultType([new Type("void")], @operator, arrows);
            }

            var matches = from a in arrows where @operator == a.Name && a.InputType == inputTypes[0] select a;
            foreach (var a in matches)
            {
                if (inputTypes.Length == 1)
                {
                    return a.OutputType;
                }

                if (a.OutputType is Arrow a2)
                {
                    var result = InferResultType(inputTypes[1..], "", new List<Arrow> { a2 });
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}

