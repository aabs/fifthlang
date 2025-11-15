using VDS.RDF.Query;

namespace Fifth.System;

/// <summary>
/// Result is a discriminated union representing the outcome of SPARQL query application.
/// It provides type-safe access to tabular data (SELECT), graph data (CONSTRUCT/DESCRIBE),
/// or boolean values (ASK) depending on the query form executed.
/// </summary>
public abstract partial record Result
{
    private Result() { } // Private constructor ensures only nested types can inherit
    
    /// <summary>
    /// Represents SELECT query results as tabular data with named SPARQL variables and rows.
    /// </summary>
    public sealed record TabularResult(SparqlResultSet ResultSet) : Result
    {
        /// <summary>
        /// Lazy enumeration of result rows for streaming access.
        /// </summary>
        public IEnumerable<ISparqlResult> Rows => ResultSet.Results;
        
        /// <summary>
        /// List of SPARQL variable names in the result (without '?' prefix).
        /// </summary>
        public IReadOnlyList<string> Variables => ResultSet.Variables.ToList();
        
        /// <summary>
        /// Total count of result rows. May force materialization of lazy results.
        /// </summary>
        public int RowCount => ResultSet.Count;
        
        /// <summary>
        /// Get value from result row by index and variable name.
        /// </summary>
        public T GetValue<T>(int rowIndex, string varName)
        {
            var row = ResultSet[rowIndex];
            var node = row[varName];
            // Convert INode to T (simplified for MVP)
            return (T)Convert.ChangeType(node?.ToString() ?? "", typeof(T));
        }
        
        /// <summary>
        /// Get all values for a specific variable across all rows.
        /// </summary>
        public IEnumerable<T> GetColumn<T>(string varName)
        {
            foreach (var row in ResultSet.Results)
            {
                if (row.HasValue(varName))
                {
                    var node = row[varName];
                    yield return (T)Convert.ChangeType(node?.ToString() ?? "", typeof(T));
                }
            }
        }
        
        /// <summary>
        /// Try to get value from result row, returns false if not present.
        /// </summary>
        public bool TryGetValue<T>(int rowIndex, string varName, out T value)
        {
            try
            {
                value = GetValue<T>(rowIndex, varName);
                return true;
            }
            catch
            {
                value = default!;
                return false;
            }
        }
    }
    
    /// <summary>
    /// Represents CONSTRUCT/DESCRIBE query results as an RDF graph wrapped in a Store.
    /// </summary>
    public sealed record GraphResult(Store GraphStore) : Result
    {
        /// <summary>
        /// Count of triples in the constructed graph (across all named graphs).
        /// </summary>
        public int TripleCount => GraphStore.TripleCount;
        
        /// <summary>
        /// Serialize the graph as TriG format.
        /// </summary>
        public string ToTriG()
        {
            return GraphStore.ToTrig();
        }
        
        /// <summary>
        /// Enumerate triples from the graph, optionally filtered by graph name.
        /// </summary>
        public IEnumerable<Triple> GetTriples(string? graphName = null)
        {
            return GraphStore.Triples(graphName);
        }
    }
    
    /// <summary>
    /// Represents ASK query results as a boolean truth value.
    /// </summary>
    public sealed record BooleanResult(bool Value) : Result;
}
