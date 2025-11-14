using System;
using System.Text;

namespace Fifth.System;

/// <summary>
/// Helper utilities for RDF/Turtle serialization.
/// Provides proper escaping for TriG literal interpolations.
/// </summary>
public static class RdfHelpers
{
    /// <summary>
    /// Escapes a string value for use in RDF/Turtle syntax.
    /// Wraps in quotes and escapes special characters according to Turtle spec.
    /// </summary>
    /// <param name="value">The string value to escape</param>
    /// <returns>A quoted and escaped string suitable for RDF/Turtle</returns>
    public static string EscapeForRdf(string value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        var escaped = new StringBuilder();
        escaped.Append('"');

        foreach (char c in value)
        {
            switch (c)
            {
                case '\\':
                    escaped.Append("\\\\");
                    break;
                case '"':
                    escaped.Append("\\\"");
                    break;
                case '\n':
                    escaped.Append("\\n");
                    break;
                case '\r':
                    escaped.Append("\\r");
                    break;
                case '\t':
                    escaped.Append("\\t");
                    break;
                default:
                    escaped.Append(c);
                    break;
            }
        }

        escaped.Append('"');
        return escaped.ToString();
    }
}
