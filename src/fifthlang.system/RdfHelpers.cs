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
    /// Escapes a value for use in RDF/Turtle syntax.
    /// Strings are wrapped in quotes and escaped. Numbers and booleans are converted to RDF literals.
    /// </summary>
    /// <param name="value">The value to escape (can be string, number, boolean, or other)</param>
    /// <returns>A properly formatted value for RDF/Turtle</returns>
    public static string EscapeForRdf(object? value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        // Handle strings with quoting and escaping
        if (value is string str)
        {
            var escaped = new StringBuilder();
            escaped.Append('"');

            foreach (char c in str)
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

        // Handle booleans - lowercase for RDF
        if (value is bool b)
        {
            return b ? "true" : "false";
        }

        // Handle numbers - convert to string without quotes
        if (value is int || value is long || value is short || value is byte ||
            value is uint || value is ulong || value is ushort || value is sbyte ||
            value is float || value is double || value is decimal)
        {
            return global::System.Convert.ToString(value, global::System.Globalization.CultureInfo.InvariantCulture) ?? "";
        }

        // Default: convert to string and quote
        return EscapeForRdf(value.ToString() ?? "");
    }
}
