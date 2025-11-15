using VDS.RDF.Query;
using Dunet;

namespace Fifth.System;

[Union]
public partial record Result
{
    partial record TabularResult(SparqlResultSet ResultSet);
    partial record GraphResult(Store GraphStore);
    partial record BooleanResult(bool Value);
}
