using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace compiler.LanguageTransformations;

/// <summary>
/// Compile-time introspection helper for SPARQL SELECT queries.
/// Parses SPARQL query text at compile time to extract:
/// - Query form (SELECT, ASK, CONSTRUCT, DESCRIBE)
/// - Projected variables (for SELECT queries)
/// 
/// Used by comprehension validation to verify:
/// - Generator is a SELECT query (not ASK/CONSTRUCT/DESCRIBE)
/// - Projected variables referenced in object projections exist
/// </summary>
public static class SparqlSelectIntrospection
{
    /// <summary>
    /// Result of introspecting a SPARQL query.
    /// </summary>
    public record IntrospectionResult
    {
        /// <summary>
        /// Whether the query parsed successfully.
        /// </summary>
        public required bool Success { get; init; }
        
        /// <summary>
        /// Query form if successfully parsed: "SELECT", "ASK", "CONSTRUCT", "DESCRIBE", or null.
        /// </summary>
        public string? QueryForm { get; init; }
        
        /// <summary>
        /// List of projected variable names (without ? prefix) for SELECT queries.
        /// Example: SELECT ?name ?age => ["name", "age"]
        /// Empty for non-SELECT queries or if query form is SELECT *.
        /// </summary>
        public List<string> ProjectedVariables { get; init; } = new();
        
        /// <summary>
        /// True if the SELECT uses * (all variables).
        /// </summary>
        public bool IsSelectStar { get; init; }
        
        /// <summary>
        /// Error message if parsing failed.
        /// </summary>
        public string? ErrorMessage { get; init; }
    }
    
    /// <summary>
    /// Introspects a SPARQL query string at compile time.
    /// Parses the query using dotNetRDF's SparqlQueryParser to extract form and projected variables.
    /// </summary>
    /// <param name="queryText">SPARQL query text (must be complete query, not a fragment)</param>
    /// <returns>Introspection result with query form and projected variables</returns>
    public static IntrospectionResult IntrospectQuery(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            return new IntrospectionResult
            {
                Success = false,
                ErrorMessage = "Query text is empty or whitespace"
            };
        }
        
        try
        {
            var parser = new SparqlQueryParser();
            var query = parser.ParseFromString(queryText);
            
            // Determine query form
            var queryForm = query.QueryType switch
            {
                SparqlQueryType.Select => "SELECT",
                SparqlQueryType.Ask => "ASK",
                SparqlQueryType.Construct => "CONSTRUCT",
                SparqlQueryType.Describe => "DESCRIBE",
                SparqlQueryType.DescribeAll => "DESCRIBE",
                SparqlQueryType.SelectAll => "SELECT",
                SparqlQueryType.SelectAllDistinct => "SELECT",
                SparqlQueryType.SelectAllReduced => "SELECT",
                SparqlQueryType.SelectDistinct => "SELECT",
                SparqlQueryType.SelectReduced => "SELECT",
                _ => "UNKNOWN"
            };
            
            // For SELECT queries, extract projected variables
            var projectedVars = new List<string>();
            var isSelectStar = false;
            
            if (queryForm == "SELECT")
            {
                // Check if it's SELECT *
                if (query.QueryType == SparqlQueryType.SelectAll ||
                    query.QueryType == SparqlQueryType.SelectAllDistinct ||
                    query.QueryType == SparqlQueryType.SelectAllReduced)
                {
                    isSelectStar = true;
                }
                else if (query.Variables != null)
                {
                    // Extract variable names (remove ? prefix)
                    // query.Variables contains SparqlVariable objects - convert to strings
                    projectedVars = query.Variables
                        .Select(v => v.ToString().TrimStart(new[] { '?', '$' }))
                        .ToList();
                }
            }
            
            return new IntrospectionResult
            {
                Success = true,
                QueryForm = queryForm,
                ProjectedVariables = projectedVars,
                IsSelectStar = isSelectStar
            };
        }
        catch (RdfParseException ex)
        {
            return new IntrospectionResult
            {
                Success = false,
                ErrorMessage = $"Failed to parse SPARQL query: {ex.Message}"
            };
        }
        catch (System.Exception ex)
        {
            return new IntrospectionResult
            {
                Success = false,
                ErrorMessage = $"Unexpected error introspecting SPARQL query: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Checks if a variable name (without ? prefix) exists in the projected variables list.
    /// For SELECT * queries, this always returns true (we can't validate at compile time).
    /// </summary>
    /// <param name="result">Introspection result from IntrospectQuery</param>
    /// <param name="variableName">Variable name to check (without ? prefix, e.g., "name")</param>
    /// <returns>True if variable exists in projection or query is SELECT *</returns>
    public static bool HasProjectedVariable(IntrospectionResult result, string variableName)
    {
        if (!result.Success || result.QueryForm != "SELECT")
        {
            return false;
        }
        
        // SELECT * accepts any variable reference (runtime will fail if missing)
        if (result.IsSelectStar)
        {
            return true;
        }
        
        // Check if variable is in the projected list
        return result.ProjectedVariables.Contains(variableName, StringComparer.OrdinalIgnoreCase);
    }
}
