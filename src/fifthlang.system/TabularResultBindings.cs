using VDS.RDF.Query;

namespace Fifth.System;

/// <summary>
/// Runtime helper for accessing variable bindings from SPARQL tabular results.
/// Used by lowered list comprehensions to extract values from SPARQL SELECT result rows.
/// </summary>
public static class TabularResultBindings
{
    /// <summary>
    /// Gets a variable binding from a SPARQL result row.
    /// Throws a runtime exception if the variable is not bound in the row.
    /// </summary>
    /// <param name="row">SPARQL result row (ISparqlResult)</param>
    /// <param name="variableName">Variable name (with or without ? prefix)</param>
    /// <returns>The bound value as an object (INode from dotNetRDF)</returns>
    /// <exception cref="ArgumentNullException">If row is null</exception>
    /// <exception cref="ArgumentException">If variableName is null or empty</exception>
    /// <exception cref="InvalidOperationException">If variable is not bound in the row</exception>
    public static object GetBinding(ISparqlResult row, string variableName)
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }
        
        if (string.IsNullOrEmpty(variableName))
        {
            throw new ArgumentException("Variable name cannot be null or empty", nameof(variableName));
        }
        
        // Normalize variable name (remove ? prefix if present)
        var cleanVarName = variableName.TrimStart('?', '$');
        
        // Check if variable is bound
        if (!row.HasBoundValue(cleanVarName))
        {
            throw new InvalidOperationException(
                $"SPARQL variable '?{cleanVarName}' is not bound in result row. " +
                $"Available variables: {string.Join(", ", row.Variables.Select(v => "?" + v))}");
        }
        
        // Get the bound node
        var node = row[cleanVarName];
        
        if (node == null)
        {
            throw new InvalidOperationException(
                $"SPARQL variable '?{cleanVarName}' is bound but value is null");
        }
        
        return node;
    }
    
    /// <summary>
    /// Gets a variable binding as a string value.
    /// Converts the RDF node to its lexical form.
    /// </summary>
    /// <param name="row">SPARQL result row</param>
    /// <param name="variableName">Variable name (with or without ? prefix)</param>
    /// <returns>The bound value as a string</returns>
    public static string GetBindingAsString(ISparqlResult row, string variableName)
    {
        var binding = GetBinding(row, variableName);
        
        if (binding is VDS.RDF.INode node)
        {
            return node switch
            {
                VDS.RDF.ILiteralNode literal => literal.Value,
                VDS.RDF.IUriNode uri => uri.Uri.ToString(),
                VDS.RDF.IBlankNode blank => blank.InternalID,
                _ => node.ToString() ?? ""
            };
        }
        
        return binding?.ToString() ?? "";
    }
    
    /// <summary>
    /// Gets a variable binding as an integer value.
    /// Attempts to parse the RDF node as an integer.
    /// </summary>
    /// <param name="row">SPARQL result row</param>
    /// <param name="variableName">Variable name (with or without ? prefix)</param>
    /// <returns>The bound value as an integer</returns>
    /// <exception cref="FormatException">If the value cannot be parsed as an integer</exception>
    public static int GetBindingAsInt(ISparqlResult row, string variableName)
    {
        var stringValue = GetBindingAsString(row, variableName);
        
        if (!int.TryParse(stringValue, out var result))
        {
            throw new FormatException(
                $"Cannot convert SPARQL variable '?{variableName}' value '{stringValue}' to integer");
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets a variable binding as a float value.
    /// Attempts to parse the RDF node as a float.
    /// </summary>
    /// <param name="row">SPARQL result row</param>
    /// <param name="variableName">Variable name (with or without ? prefix)</param>
    /// <returns>The bound value as a float</returns>
    /// <exception cref="FormatException">If the value cannot be parsed as a float</exception>
    public static float GetBindingAsFloat(ISparqlResult row, string variableName)
    {
        var stringValue = GetBindingAsString(row, variableName);
        
        if (!float.TryParse(stringValue, out var result))
        {
            throw new FormatException(
                $"Cannot convert SPARQL variable '?{variableName}' value '{stringValue}' to float");
        }
        
        return result;
    }
    
    /// <summary>
    /// Checks if a variable is bound in a result row.
    /// </summary>
    /// <param name="row">SPARQL result row</param>
    /// <param name="variableName">Variable name (with or without ? prefix)</param>
    /// <returns>True if the variable is bound, false otherwise</returns>
    public static bool HasBinding(ISparqlResult row, string variableName)
    {
        if (row == null || string.IsNullOrEmpty(variableName))
        {
            return false;
        }
        
        var cleanVarName = variableName.TrimStart('?', '$');
        return row.HasBoundValue(cleanVarName);
    }
    
    /// <summary>
    /// Enumerates all rows from a tabular SPARQL result.
    /// Helper for lowered comprehensions to iterate over SELECT results.
    /// </summary>
    /// <param name="result">SPARQL result (must be TabularResult)</param>
    /// <returns>Enumerable of result rows</returns>
    /// <exception cref="ArgumentNullException">If result is null</exception>
    /// <exception cref="ArgumentException">If result is not a TabularResult</exception>
    public static IEnumerable<ISparqlResult> EnumerateRows(Result result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }
        
        if (result is not Result.TabularResult tabular)
        {
            throw new ArgumentException(
                $"Expected TabularResult but got {result.GetType().Name}. " +
                "SPARQL comprehensions require SELECT queries that produce tabular results.",
                nameof(result));
        }
        
        return tabular.ResultSet;
    }
}
