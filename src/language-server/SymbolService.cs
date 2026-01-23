using System.Text.RegularExpressions;
using ast;
using ast_generated;
using ast_model.Symbols;
using ast_model.TypeSystem;
using compiler;
using Fifth.LanguageServer.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Fifth.LanguageServer;

public sealed class SymbolService
{
    private static readonly Regex IdentifierRegex = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
    private static readonly Regex FunctionDefinitionRegex = new(@"(?m)^(?<name>[A-Za-z_][A-Za-z0-9_]*)(?:\s*<[^>]+>)?\s*\((?<params>[^)]*)\)\s*:\s*(?<return>[^\s\{]+)", RegexOptions.Compiled);

    private static readonly string[] WorkspaceIgnoreSegments = [".git", "bin", "obj", ".idea", ".vscode", "artifacts", "dist", "site"];

    public IEnumerable<string> CollectIdentifiers(string text)
    {
        foreach (Match match in IdentifierRegex.Matches(text))
        {
            yield return match.Value;
        }
    }

    public (string word, LspRange range)? GetWordAtPosition(string text, Position position)
    {
        var lines = text.Split('\n');
        if (position.Line < 0 || position.Line >= lines.Length)
        {
            return null;
        }

        var line = lines[position.Line];
        if (position.Character < 0 || position.Character > line.Length)
        {
            return null;
        }

        int start = position.Character;
        while (start > 0 && IsIdentChar(line[start - 1]))
            start--;

        int end = position.Character;
        while (end < line.Length && IsIdentChar(line[end]))
            end++;

        if (start == end)
            return null;

        var word = line[start..end];
        return (word, new LspRange(new Position(position.Line, start), new Position(position.Line, end)));
    }

    public static string ResolveWorkspaceRoot(Uri documentUri)
    {
        var envRoot = Environment.GetEnvironmentVariable("FIFTH_WORKSPACE_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot) && Directory.Exists(envRoot))
        {
            return envRoot;
        }

        if (documentUri.IsFile)
        {
            var dir = Path.GetDirectoryName(documentUri.LocalPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                var current = new DirectoryInfo(dir);
                while (current != null)
                {
                    if (Directory.Exists(Path.Combine(current.FullName, ".git")))
                    {
                        return current.FullName;
                    }
                    current = current.Parent;
                }

                return dir;
            }
        }

        return Directory.GetCurrentDirectory();
    }

    public WorkspaceSymbolIndex BuildWorkspaceIndex(
        string workspaceRoot,
        IReadOnlyDictionary<Uri, string> openDocuments,
        IReadOnlyDictionary<Uri, ParsedDocument> parsedDocuments)
    {
        var definitions = new Dictionary<string, List<SymbolDefinition>>(StringComparer.Ordinal);
        var seenUris = new HashSet<Uri>();

        foreach (var (uri, parsed) in parsedDocuments)
        {
            seenUris.Add(uri);
            AddDefinitionsFromParsed(parsed, uri, definitions);
        }

        foreach (var file in EnumerateWorkspaceFiles(workspaceRoot))
        {
            var uri = new Uri(file);
            if (seenUris.Contains(uri))
            {
                continue;
            }

            var parsed = false;
            try
            {
                var ast = FifthParserManager.ParseFile(file) as AssemblyDef;
                if (ast is not null)
                {
                    var diagnostics = new List<compiler.Diagnostic>();
                    var analyzed = FifthParserManager.ApplyLanguageAnalysisPhases(ast, diagnostics);
                    if (analyzed is AssemblyDef analyzedAssembly)
                    {
                        AddDefinitionsFromAst(analyzedAssembly, uri, definitions);
                        parsed = true;
                    }
                }
            }
            catch
            {
                parsed = false;
            }

            if (!parsed)
            {
                var text = openDocuments.TryGetValue(uri, out var openText)
                    ? openText
                    : File.ReadAllText(file);
                AddDefinitionsFromText(text, uri, definitions);
            }
        }

        return new WorkspaceSymbolIndex(definitions);
    }

    public LocationOrLocationLinks? FindDefinition(WorkspaceSymbolIndex index, string word, Uri requestingUri)
    {
        if (!index.Definitions.TryGetValue(word, out var matches) || matches.Count == 0)
        {
            return null;
        }

        var candidates = matches.Where(m => m.Kind != ast.SymbolKind.VoidSymbol).ToList();
        if (candidates.Count == 0)
        {
            candidates = matches;
        }

        var sameFile = candidates.FirstOrDefault(m => m.Uri == requestingUri);
        var preferred = sameFile ?? candidates.First();
        return new LocationOrLocationLinks(new LocationOrLocationLink(new Location
        {
            Uri = preferred.Uri,
            Range = preferred.Range
        }));
    }

    private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static (int line, int columnStart) CountLines(string text, int index)
    {
        int line = 0;
        int col = 0;
        for (int i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                col = 0;
            }
            else
            {
                col++;
            }
        }
        return (line, col);
    }

    private static void AddDefinitionsFromParsed(ParsedDocument parsed, Uri uri, Dictionary<string, List<SymbolDefinition>> definitions)
    {
        if (parsed.AnalyzedAst is AssemblyDef analyzed)
        {
            AddDefinitionsFromAst(analyzed, uri, definitions);
            return;
        }

        if (!string.IsNullOrWhiteSpace(parsed.Text))
        {
            AddDefinitionsFromText(parsed.Text, uri, definitions);
        }
    }

    private static void AddDefinitionsFromAst(AssemblyDef ast, Uri uri, Dictionary<string, List<SymbolDefinition>> definitions)
    {
        var visitor = new SymbolIndexVisitor(entry => AddDefinitionFromEntry(entry, uri, definitions));
        visitor.Visit(ast);
    }

    private static void AddDefinitionFromEntry(ISymbolTableEntry entry, Uri uri, Dictionary<string, List<SymbolDefinition>> definitions)
    {
        var location = entry.OriginatingAstThing?.Location;
        var line = Math.Max((location?.Line ?? 1) - 1, 0);
        var column = Math.Max(location?.Column ?? 0, 0);
        var name = entry.Symbol.Name;
        var signature = entry.OriginatingAstThing is null ? null : BuildSignature(entry.OriginatingAstThing, name);
        var range = new LspRange(
            new Position(line, column),
            new Position(line, column + name.Length));

        var definition = new SymbolDefinition(
            name,
            uri,
            range,
            entry.QualifiedName,
            entry.Symbol.Kind,
            signature);

        if (!definitions.TryGetValue(name, out var list))
        {
            list = new List<SymbolDefinition>();
            definitions[name] = list;
        }

        if (list.All(existing => existing.Uri != uri || existing.Range != range))
        {
            list.Add(definition);
        }
    }

    private static void AddDefinitionsFromText(string text, Uri uri, Dictionary<string, List<SymbolDefinition>> definitions)
    {
        foreach (Match match in FunctionDefinitionRegex.Matches(text))
        {
            var name = match.Groups["name"].Value;
            var parameters = match.Groups["params"].Value.Trim();
            var returnType = match.Groups["return"].Value.Trim();
            var signature = $"{name}({parameters}): {returnType}";

            var (startLine, startCol) = CountLines(text, match.Index);
            var (endLine, endCol) = CountLines(text, match.Index + name.Length);
            var range = new LspRange(
                new Position(startLine, startCol),
                new Position(endLine, endCol));

            var definition = new SymbolDefinition(name, uri, range, null, ast.SymbolKind.FunctionDef, signature);
            if (!definitions.TryGetValue(name, out var functionList))
            {
                functionList = new List<SymbolDefinition>();
                definitions[name] = functionList;
            }
            if (functionList.All(existing => existing.Uri != uri || existing.Range != range))
            {
                functionList.Add(definition);
            }
        }

        var identifierMatch = IdentifierRegex.Match(text);
        while (identifierMatch.Success)
        {
            var (startLine, startCol) = CountLines(text, identifierMatch.Index);
            var (endLine, endCol) = CountLines(text, identifierMatch.Index + identifierMatch.Length);
            var range = new LspRange(
                new Position(startLine, startCol),
                new Position(endLine, endCol));

            var definition = new SymbolDefinition(identifierMatch.Value, uri, range, null, ast.SymbolKind.VoidSymbol, null);
            if (!definitions.TryGetValue(identifierMatch.Value, out var list))
            {
                list = new List<SymbolDefinition>();
                definitions[identifierMatch.Value] = list;
            }
            if (list.All(existing => existing.Uri != uri || existing.Range != range))
            {
                list.Add(definition);
            }

            identifierMatch = identifierMatch.NextMatch();
        }
    }

    private static IEnumerable<string> EnumerateWorkspaceFiles(string workspaceRoot)
    {
        if (!Directory.Exists(workspaceRoot))
        {
            return Enumerable.Empty<string>();
        }

        return Directory.EnumerateFiles(workspaceRoot, "*.5th", SearchOption.AllDirectories)
            .Where(path => !WorkspaceIgnoreSegments.Any(segment => path.Split(Path.DirectorySeparatorChar).Contains(segment)));
    }

    private static string? BuildSignature(IAstThing astThing, string name)
    {
        if (astThing is IOverloadableFunction overload)
        {
            return BuildSignatureFromOverload(name, overload.Params, overload.ReturnType);
        }

        if (astThing is OverloadedFunctionDefinition definition && definition.Signature is not null)
        {
            return $"{name}{definition.Signature}";
        }

        return null;
    }

    private static string BuildSignatureFromOverload(string name, IReadOnlyList<ParamDef> parameters, FifthType returnType)
    {
        var parameterList = parameters
            .Select(p => $"{p.Name}: {p.TypeName.Value ?? p.TypeName.ToString()}")
            .ToList();
        var returnTypeName = returnType.Name.Value ?? returnType.Name.ToString();
        var signature = $"{name}({string.Join(", ", parameterList)})";
        if (!string.IsNullOrWhiteSpace(returnTypeName))
        {
            signature += $": {returnTypeName}";
        }
        return signature;
    }

    private sealed class SymbolIndexVisitor : DefaultRecursiveDescentVisitor
    {
        private readonly Action<ISymbolTableEntry> _record;

        public SymbolIndexVisitor(Action<ISymbolTableEntry> record)
        {
            _record = record;
        }

        public override AstThing Visit(AstThing ctx)
        {
            if (ctx is ScopeAstThing scope)
            {
                foreach (var entry in scope.SymbolTable.All())
                {
                    _record(entry);
                }
            }

            return base.Visit(ctx);
        }
    }
}

public sealed record SymbolDefinition(
    string Name,
    Uri Uri,
    LspRange Range,
    string? QualifiedName,
    ast.SymbolKind Kind,
    string? Signature);

public sealed record WorkspaceSymbolIndex(Dictionary<string, List<SymbolDefinition>> Definitions);
