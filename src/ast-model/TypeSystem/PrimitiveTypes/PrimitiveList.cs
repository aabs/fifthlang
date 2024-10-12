namespace ast_model.TypeSystem.PrimitiveTypes;

public class PrimitiveList : PrimitiveAny
{
    public PrimitiveList(FifthType typeParameter)
    {
        Type = Type with {TypeArguments = [typeParameter]} ;
    }

    public List<object> List { get; private set; }

    public IValueObject Head()
    {
        return GetItemAt(0);
    }

    public PrimitiveList Tail()
    {
        return new(Type.TypeArguments[0]) { List = List.GetRange(1, List.Count - 1), TypeId = TypeId };
    }

    private IValueObject GetItemAt(int i)
    {
        if (List.Count > i)
        {
            return new ValueObject(Type.TypeArguments[0].Id, string.Empty, List[i]);
        }

        return default;
    }
}
