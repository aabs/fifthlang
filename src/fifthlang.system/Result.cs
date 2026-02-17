namespace Fifth.System;

[Union]
public abstract partial record Result
{
    public partial record TabularResult(SparqlResultSet ResultSet) : Result;
    public partial record GraphResult(Store GraphStore) : Result;
    public partial record BooleanResult(bool Value) : Result;
}
