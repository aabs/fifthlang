using ast;
using ast_generated;
using ast_model.Symbols;
using Antlr4.Runtime;
using System.Text.RegularExpressions;

namespace Fifth.LangProcessingPhases;

/// <summary>
/// SPARQL variable binding visitor that resolves Fifth variable references within SPARQL literals.
/// This visitor scans the SPARQL text for identifiers that match in-scope Fifth variables
/// and populates the Bindings list for safe parameterization.
/// </summary>
/// <remarks>
/// This implements User Story 2: Variable Binding via Parameters.
/// Variables referenced in SPARQL text (e.g., "age" in "?s ex:age age") are bound as
/// typed parameters using dotNetRDF's SparqlParameterizedString, preventing injection attacks.
/// 
/// The visitor uses the SPARQL lexer to identify bare identifiers (not SPARQL keywords or variables)
/// that could be Fifth variable references, then attempts to resolve them in the symbol table.
/// </remarks>
public class SparqlVariableBindingVisitor : DefaultRecursiveDescentVisitor
{
    private readonly List<Diagnostic> diagnostics = new();
    
    // SPARQL keywords that should not be treated as Fifth variable references
    private static readonly HashSet<string> SparqlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "CONSTRUCT", "DESCRIBE", "ASK", "FROM", "NAMED", "WHERE", "GRAPH",
        "OPTIONAL", "UNION", "FILTER", "MINUS", "BIND", "SERVICE", "SILENT", "VALUES",
        "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "LIMIT", "OFFSET",
        "DISTINCT", "REDUCED", "AS", "BASE", "PREFIX", "STR", "LANG", "LANGMATCHES",
        "DATATYPE", "BOUND", "IRI", "URI", "BNODE", "RAND", "ABS", "CEIL", "FLOOR",
        "ROUND", "CONCAT", "STRLEN", "UCASE", "LCASE", "CONTAINS", "STRSTARTS", "STRENDS",
        "STRBEFORE", "STRAFTER", "YEAR", "MONTH", "DAY", "HOURS", "MINUTES", "SECONDS",
        "TIMEZONE", "TZ", "NOW", "UUID", "STRUUID", "MD5", "SHA1", "SHA256", "SHA384",
        "SHA512", "COALESCE", "IF", "STRLANG", "STRDT", "SAMETERM", "ISIRI", "ISURI",
        "ISBLANK", "ISLITERAL", "ISNUMERIC", "REGEX", "SUBSTR", "REPLACE", "EXISTS",
        "NOT", "IN", "TRUE", "FALSE", "UNDEF", "INSERT", "DELETE", "LOAD", "CLEAR",
        "DROP", "CREATE", "ADD", "MOVE", "COPY", "WITH", "DATA", "TO", "DEFAULT",
        "ALL", "USING", "INTO", "a"
    };

    /// <summary>
    /// Gets the list of diagnostics generated during variable binding resolution.
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => diagnostics.AsReadOnly();

    /// <summary>
    /// Visits a SparqlLiteralExpression and resolves variable references within the SPARQL text.
    /// </summary>
    public override SparqlLiteralExpression VisitSparqlLiteralExpression(SparqlLiteralExpression ctx)
    {
        // First visit children using base implementation
        var result = base.VisitSparqlLiteralExpression(ctx);

        // Find the nearest scope for symbol table lookups
        var nearestScope = ctx.NearestScope();
        if (nearestScope == null)
        {
            return result;
        }

        // Extract potential Fifth variable references from the SPARQL text
        var identifiers = ExtractIdentifiers(result.SparqlText);
        var bindings = new List<VariableBinding>();
        
        foreach (var identifier in identifiers)
        {
            // Skip SPARQL keywords
            if (SparqlKeywords.Contains(identifier.Name))
            {
                continue;
            }
            
            // Try to resolve against symbol table
            if (TryResolveVariable(identifier.Name, nearestScope, out var varDecl))
            {
                // Create a binding for this Fifth variable
                bindings.Add(new VariableBinding
                {
                    Name = identifier.Name,
                    ResolvedExpression = new VarRefExp 
                    { 
                        VarName = identifier.Name, 
                        VariableDecl = varDecl,
                        Location = result.Location,
                        Parent = result,
                        Annotations = []
                    },
                    PositionInLiteral = identifier.Position,
                    Length = identifier.Length,
                    Location = result.Location,
                    Parent = result,
                    Annotations = []
                });
            }
            // Note: We don't emit diagnostics for unresolved identifiers because they might be
            // SPARQL prefixes, property names, or other valid SPARQL constructs.
            // Only emit error if it looks like it should be a Fifth variable but isn't found.
        }
        
        return result with { Bindings = bindings };
    }

    /// <summary>
    /// Extracts potential Fifth variable identifiers from SPARQL text.
    /// Returns identifiers that are not SPARQL variables (?var, $var), IRIs (<...>), 
    /// or string literals.
    /// </summary>
    private List<IdentifierInfo> ExtractIdentifiers(string sparqlText)
    {
        var identifiers = new List<IdentifierInfo>();
        
        try
        {
            // Use SPARQL lexer to tokenize the text
            var input = new AntlrInputStream(sparqlText);
            var lexer = new SparqlLexer(input);
            var tokens = lexer.GetAllTokens();
            
            // Reset lexer for potential reuse
            lexer.Reset();
            lexer.InputStream.Seek(0);
            
            foreach (var token in tokens)
            {
                // Look for PNAME_LN tokens (prefixed names like ex:age)
                // The local part could be a Fifth variable
                if (token.Type == SparqlLexer.PNAME_LN)
                {
                    var text = token.Text;
                    var colonIndex = text.IndexOf(':');
                    if (colonIndex >= 0 && colonIndex < text.Length - 1)
                    {
                        var localPart = text.Substring(colonIndex + 1);
                        // Only consider as potential variable if it's a simple identifier
                        if (IsSimpleIdentifier(localPart))
                        {
                            identifiers.Add(new IdentifierInfo
                            {
                                Name = localPart,
                                Position = token.StartIndex + colonIndex + 1,
                                Length = localPart.Length
                            });
                        }
                    }
                }
                // Look for bare PNAME_NS (could be a simple identifier used as object)
                else if (token.Type == SparqlLexer.PNAME_NS)
                {
                    var text = token.Text;
                    if (text.EndsWith(":"))
                    {
                        text = text.Substring(0, text.Length - 1);
                    }
                    if (!string.IsNullOrEmpty(text) && IsSimpleIdentifier(text))
                    {
                        identifiers.Add(new IdentifierInfo
                        {
                            Name = text,
                            Position = token.StartIndex,
                            Length = text.Length
                        });
                    }
                }
            }
        }
        catch
        {
            // If lexing fails, fall back to simple regex-based extraction
            // This handles cases where the SPARQL might not be fully valid yet
            var regex = new Regex(@"\b([a-zA-Z_][a-zA-Z0-9_]*)\b");
            var matches = regex.Matches(sparqlText);
            
            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                if (!SparqlKeywords.Contains(name))
                {
                    identifiers.Add(new IdentifierInfo
                    {
                        Name = name,
                        Position = match.Index,
                        Length = match.Length
                    });
                }
            }
        }
        
        // Remove duplicates (same name)
        return identifiers
            .GroupBy(i => i.Name)
            .Select(g => g.First())
            .ToList();
    }
    
    /// <summary>
    /// Checks if a string is a simple identifier (alphanumeric + underscore).
    /// </summary>
    private bool IsSimpleIdentifier(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
            
        if (!char.IsLetter(text[0]) && text[0] != '_')
            return false;
            
        return text.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Attempts to resolve a variable reference by name within the given scope.
    /// </summary>
    private bool TryResolveVariable(string varName, ScopeAstThing scope, out VariableDecl? resolvedDecl)
    {
        resolvedDecl = null;
        
        if (string.IsNullOrEmpty(varName) || scope == null)
        {
            return false;
        }

        var symbol = new Symbol(varName, SymbolKind.VarDeclStatement);
        
        if (scope.TryResolve(symbol, out var symbolTableEntry))
        {
            if (symbolTableEntry?.OriginatingAstThing is VariableDecl variableDecl)
            {
                resolvedDecl = variableDecl;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Emits a diagnostic for an unknown variable reference in SPARQL text.
    /// </summary>
    private void EmitUnknownVariableDiagnostic(string varName, SparqlLiteralExpression context)
    {
        var diagnostic = new Diagnostic
        {
            Code = SparqlDiagnostics.UnknownVariable,
            Message = SparqlDiagnostics.FormatUnknownVariable(varName),
            Severity = DiagnosticSeverity.Error,
            Filename = context.Location?.Filename ?? "",
            Line = context.Location?.Line ?? 0,
            Column = context.Location?.Column ?? 0
        };

        diagnostics.Add(diagnostic);
    }
}

/// <summary>
/// Information about an identifier found in SPARQL text.
/// </summary>
internal record IdentifierInfo
{
    public required string Name { get; init; }
    public required int Position { get; init; }
    public required int Length { get; init; }
}

/// <summary>
/// Represents a diagnostic message from the SPARQL variable binding process.
/// </summary>
public class Diagnostic
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required string Filename { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
}

/// <summary>
/// Severity levels for diagnostics.
/// </summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}
