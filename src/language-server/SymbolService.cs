using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Fifth.LanguageServer;

public sealed class SymbolService
{
    private static readonly Regex IdentifierRegex = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

    public IEnumerable<string> CollectIdentifiers(string text)
    {
        foreach (Match match in IdentifierRegex.Matches(text))
        {
            yield return match.Value;
        }
    }

    public (string word, Range range)? GetWordAtPosition(string text, Position position)
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
        return (word, new Range(new Position(position.Line, start), new Position(position.Line, end)));
    }

    public LocationOrLocationLinks? FindDefinition(string text, Uri uri, string word)
    {
        var match = IdentifierRegex.Match(text);
        while (match.Success)
        {
            if (string.Equals(match.Value, word, StringComparison.Ordinal))
            {
                var (startLine, startCol) = CountLines(text, match.Index);
                var (endLine, endCol) = CountLines(text, match.Index + match.Length);
                var range = new Range(
                    new Position(startLine, startCol),
                    new Position(endLine, endCol));

                return new LocationOrLocationLinks(new LocationOrLocationLink(new Location
                {
                    Uri = uri,
                    Range = range
                }));
            }
            match = match.NextMatch();
        }

        return null;
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
}
