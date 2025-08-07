using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CodeGenerator <astmetamodel-path> <output-dir>");
                return;
            }

            var astMetamodelPath = args[0];
            var outputDir = args[1];
            
            Console.WriteLine($"Reading from: {astMetamodelPath}");
            Console.WriteLine($"Output to: {outputDir}");

            try
            {
                var astContent = File.ReadAllText(astMetamodelPath);
                var ilContent = "";
                
                var ilMetamodelPath = astMetamodelPath.Replace("AstMetamodel.cs", "ILMetamodel.cs");
                if (File.Exists(ilMetamodelPath))
                {
                    ilContent = File.ReadAllText(ilMetamodelPath);
                    Console.WriteLine($"Found IL metamodel: {ilMetamodelPath}");
                }

                // Extract AST types
                var astTypes = ExtractTypes(astContent, "ast");
                var ilTypes = ExtractTypes(ilContent, "il_ast");

                Console.WriteLine($"Found {astTypes.Count} AST types and {ilTypes.Count} IL types");

                // Generate files
                Directory.CreateDirectory(outputDir);
                
                if (astTypes.Count > 0)
                {
                    File.WriteAllText(Path.Combine(outputDir, "builders.generated.cs"), 
                        GenerateBuilders(astTypes, "ast"));
                    File.WriteAllText(Path.Combine(outputDir, "visitors.generated.cs"), 
                        GenerateVisitors(astTypes, "ast"));
                    File.WriteAllText(Path.Combine(outputDir, "typeinference.generated.cs"), 
                        GenerateTypeInference(astTypes, "ast"));
                    Console.WriteLine("Generated AST files");
                }

                if (ilTypes.Count > 0)
                {
                    File.WriteAllText(Path.Combine(outputDir, "il.builders.generated.cs"), 
                        GenerateBuilders(ilTypes, "il_ast"));
                    File.WriteAllText(Path.Combine(outputDir, "il.visitors.generated.cs"), 
                        GenerateVisitors(ilTypes, "il_ast"));
                    Console.WriteLine("Generated IL files");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static List<TypeInfo> ExtractTypes(string content, string namespaceName)
        {
            var types = new List<TypeInfo>();
            if (string.IsNullOrEmpty(content)) return types;

            // Match public record declarations but exclude record structs and commented lines
            var recordRegex = new Regex(@"^\s*public\s+record\s+(?!struct\s+)(\w+)\s*(?::\s*\w+)?", RegexOptions.Multiline);
            var matches = recordRegex.Matches(content);

            var csharpKeywords = new HashSet<string> 
            { 
                "struct", "class", "interface", "abstract", "sealed", "static", 
                "public", "private", "protected", "internal", "virtual", "override"
            };

            var seenTypes = new HashSet<string>(); // Track seen types to avoid duplicates

            foreach (Match match in matches)
            {
                var typeName = match.Groups[1].Value;
                
                // Skip C# keywords, abstract records, and duplicates
                if (csharpKeywords.Contains(typeName.ToLower()) || 
                    match.Value.Contains("abstract") ||
                    seenTypes.Contains(typeName)) 
                    continue;

                // Skip lines that are commented out
                var lineStart = content.LastIndexOf('\n', match.Index);
                if (lineStart == -1) lineStart = 0;
                else lineStart += 1;
                
                // Get the line content before the match to check for comments
                var lineText = lineStart <= match.Index ? content.Substring(lineStart, match.Index - lineStart) : "";
                if (lineText.TrimStart().StartsWith("//"))
                    continue;

                seenTypes.Add(typeName);

                var typeInfo = new TypeInfo
                {
                    Name = typeName,
                    FullName = $"{namespaceName}.{typeName}",
                    Namespace = namespaceName,
                    Properties = ExtractProperties(content, typeName)
                };
                
                Console.WriteLine($"  Type: {typeName}, Properties: {typeInfo.Properties.Count}");
                types.Add(typeInfo);
            }

            return types;
        }

        static List<PropertyInfo> ExtractProperties(string content, string typeName)
        {
            var properties = new List<PropertyInfo>();
            
            // Find the record definition - use word boundaries to avoid partial matches
            var recordPattern = $@"\bpublic\s+record\s+{Regex.Escape(typeName)}\b";
            var match = Regex.Match(content, recordPattern);
            if (!match.Success)
                return properties;

            var startIndex = match.Index;

            // Extract base type if any
            string baseType = null;
            var lineEnd = content.IndexOf('\n', startIndex);
            var lineText = content.Substring(startIndex, lineEnd - startIndex);
            var colonIndex = lineText.IndexOf(':');
            if (colonIndex != -1)
            {
                var baseTypePart = lineText.Substring(colonIndex + 1).Trim();
                var spaceIndex = baseTypePart.IndexOf(' ');
                var braceIndex = baseTypePart.IndexOf('{');
                var semicolonIndex = baseTypePart.IndexOf(';');
                
                var endIndex = baseTypePart.Length;
                if (spaceIndex != -1) endIndex = Math.Min(endIndex, spaceIndex);
                if (braceIndex != -1) endIndex = Math.Min(endIndex, braceIndex);
                if (semicolonIndex != -1) endIndex = Math.Min(endIndex, semicolonIndex);
                
                baseType = baseTypePart.Substring(0, endIndex).Trim();
            }

            // Add required properties from base types
            if (!string.IsNullOrEmpty(baseType))
            {
                var baseProperties = GetRequiredPropertiesFromBaseType(baseType);
                properties.AddRange(baseProperties);
            }

            // Check if this is a single-line inheritance record (like: public record TypeName : BaseType;)
            if (lineText.Contains(';') && !lineText.Contains('{'))
            {
                // This is a simple inheritance record without additional properties
                return properties;
            }

            // Find the opening brace
            var openBraceIndex = content.IndexOf('{', startIndex);
            if (openBraceIndex == -1)
            {
                // This might be a simple record like: public record TypeName;
                return properties;
            }

            // Make sure the opening brace is for this record and not a later one
            var firstNewlineAfterRecord = content.IndexOf('\n', startIndex);
            if (openBraceIndex > firstNewlineAfterRecord + 100) // Allow some reasonable distance
            {
                // The brace is probably for a different record
                return properties;
            }

            // Find the matching closing brace by counting braces
            var braceCount = 1;
            var currentIndex = openBraceIndex + 1;
            var closeBraceIndex = -1;
            
            while (currentIndex < content.Length && braceCount > 0)
            {
                if (content[currentIndex] == '{')
                    braceCount++;
                else if (content[currentIndex] == '}')
                    braceCount--;
                
                if (braceCount == 0)
                {
                    closeBraceIndex = currentIndex;
                    break;
                }
                currentIndex++;
            }

            if (closeBraceIndex == -1)
                return properties;

            // Extract the body between the braces
            var body = content.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);
            
            // Extract properties - handle complex types including generics and arrays
            var propRegex = new Regex(@"public\s+(?:required\s+)?([^{}\r\n]+?)\s+(\w+)\s*\{\s*[^}]*\}(?:\s*=\s*[^;]+)?", RegexOptions.Multiline);
            var propMatches = propRegex.Matches(body);
            
            foreach (Match propMatch in propMatches)
            {
                var propType = propMatch.Groups[1].Value.Trim();
                var propName = propMatch.Groups[2].Value;
                
                // Skip invalid property types or attributes
                if (string.IsNullOrEmpty(propType) || string.IsNullOrEmpty(propName) || 
                    propType.StartsWith("[") || propName.StartsWith("["))
                    continue;
                
                // Clean up the type name (remove extra whitespace)
                propType = Regex.Replace(propType, @"\s+", " ").Trim();
                
                properties.Add(new PropertyInfo
                {
                    Name = propName,
                    TypeName = propType,
                    IsCollection = propType.Contains("List<") || propType.Contains("LinkedList<") || propType.Contains("[]")
                });
            }

            return properties;
        }
        
        static List<PropertyInfo> GetRequiredPropertiesFromBaseType(string baseType)
        {
            var properties = new List<PropertyInfo>();
            
            switch (baseType)
            {
                case "Definition":
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "Visibility", 
                        TypeName = "Visibility",
                        IsRequired = true,
                        DefaultValue = "Visibility.Public"
                    });
                    break;
                    
                case "ScopedDefinition":
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "Visibility", 
                        TypeName = "Visibility",
                        IsRequired = true,
                        DefaultValue = "Visibility.Public"
                    });
                    break;
                    
                case "MemberDef":
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "Visibility", 
                        TypeName = "Visibility",
                        IsRequired = true,
                        DefaultValue = "Visibility.Public"
                    });
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "Name", 
                        TypeName = "MemberName",
                        IsRequired = true,
                        DefaultValue = "MemberName.From(\"DefaultName\")"
                    });
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "TypeName", 
                        TypeName = "TypeName",
                        IsRequired = true,
                        DefaultValue = "TypeName.From(\"DefaultType\")"
                    });
                    properties.Add(new PropertyInfo 
                    { 
                        Name = "IsReadOnly", 
                        TypeName = "bool",
                        IsRequired = true,
                        DefaultValue = "false"
                    });
                    break;
            }
            
            return properties;
        }

        static string GenerateBuilders(List<TypeInfo> types, string namespaceName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName}_generated;");
            sb.AppendLine($"using ast_generated;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine("using ast_model.TypeSystem;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("#nullable disable");
            sb.AppendLine();

            foreach (var type in types)
            {
                // Skip generic types for now - they need special handling
                if (type.Name.Contains("<") || type.Name == "Literal")
                    continue;

                sb.AppendLine($"public class {type.Name}Builder : IBuilder<{type.FullName}>");
                sb.AppendLine("{");

                // Generate private fields
                foreach (var prop in type.Properties)
                {
                    if (prop.IsRequired && !string.IsNullOrEmpty(prop.DefaultValue))
                    {
                        sb.AppendLine($"    private {prop.TypeName} _{prop.Name} = {prop.DefaultValue};");
                    }
                    else
                    {
                        sb.AppendLine($"    private {prop.TypeName} _{prop.Name};");
                    }
                }

                // Add Annotations field only for AST types (which inherit from AnnotatedThing)
                if (namespaceName == "ast")
                {
                    sb.AppendLine("    private Dictionary<string, object> _Annotations = new();");
                }
                
                sb.AppendLine();
                sb.AppendLine($"    public {type.FullName} Build()");
                sb.AppendLine("    {");
                sb.AppendLine($"        return new {type.FullName}()");
                sb.AppendLine("        {");

                var allProperties = new List<string>();
                
                // Add regular properties
                foreach (var prop in type.Properties)
                {
                    allProperties.Add($"            {prop.Name} = this._{prop.Name}");
                }
                
                // Add Annotations property only for AST types (which inherit from AnnotatedThing)
                if (namespaceName == "ast")
                {
                    allProperties.Add("            Annotations = this._Annotations");
                }
                
                // Write all properties with commas, except the last one
                for (int i = 0; i < allProperties.Count; i++)
                {
                    var suffix = i == allProperties.Count - 1 ? "" : ",";
                    sb.AppendLine($"{allProperties[i]}{suffix}");
                }

                sb.AppendLine("        };");
                sb.AppendLine("    }");

                // Generate With methods
                foreach (var prop in type.Properties)
                {
                    sb.AppendLine($"    public {type.Name}Builder With{prop.Name}({prop.TypeName} value)");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        _{prop.Name} = value;");
                    sb.AppendLine("        return this;");
                    sb.AppendLine("    }");
                    sb.AppendLine();

                    // Generate AddingItemTo methods for collections
                    if (prop.IsCollection)
                    {
                        var elementType = ExtractElementType(prop.TypeName);
                        if (!string.IsNullOrEmpty(elementType))
                        {
                            sb.AppendLine($"    public {type.Name}Builder AddingItemTo{prop.Name}({elementType} value)");
                            sb.AppendLine("    {");
                            sb.AppendLine($"        _{prop.Name} ??= new {prop.TypeName}();");
                            sb.AppendLine($"        _{prop.Name}.Add(value);");
                            sb.AppendLine("        return this;");
                            sb.AppendLine("    }");
                            sb.AppendLine();
                        }
                    }
                }

                // Add WithAnnotations method only for AST types (which inherit from AnnotatedThing)
                if (namespaceName == "ast")
                {
                    sb.AppendLine($"    public {type.Name}Builder WithAnnotations(Dictionary<string, object> value)");
                    sb.AppendLine("    {");
                    sb.AppendLine("        _Annotations = value;");
                    sb.AppendLine("        return this;");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

            sb.AppendLine("#nullable restore");
            return sb.ToString();
        }

        static string GenerateVisitors(List<TypeInfo> types, string namespaceName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName}_generated;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();

            // Filter out generic types
            var filteredTypes = types.Where(t => !t.Name.Contains("<") && t.Name != "Literal").ToList();

            // Generate IAstVisitor interface
            sb.AppendLine("public interface IAstVisitor");
            sb.AppendLine("{");
            foreach (var type in filteredTypes)
            {
                sb.AppendLine($"    public void Enter{type.Name}({type.Name} ctx);");
                sb.AppendLine($"    public void Leave{type.Name}({type.Name} ctx);");
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // Generate BaseAstVisitor
            sb.AppendLine("public partial class BaseAstVisitor : IAstVisitor");
            sb.AppendLine("{");
            foreach (var type in filteredTypes)
            {
                sb.AppendLine($"    public virtual void Enter{type.Name}({type.Name} ctx) {{ }}");
                sb.AppendLine($"    public virtual void Leave{type.Name}({type.Name} ctx) {{ }}");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        static string GenerateTypeInference(List<TypeInfo> types, string namespaceName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName}_generated;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine("using ast_model.Symbols;");
            sb.AppendLine("using ast_model.TypeSystem;");
            sb.AppendLine();

            // Filter out generic types
            var filteredTypes = types.Where(t => !t.Name.Contains("<") && t.Name != "Literal").ToList();

            // Generate ITypeChecker interface
            sb.AppendLine("public interface ITypeChecker");
            sb.AppendLine("{");
            foreach (var type in filteredTypes)
            {
                sb.AppendLine($"    public FifthType Infer(ScopeAstThing scope, {type.Name} node);");
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // Generate abstract base class
            sb.AppendLine("public abstract class FunctionalTypeChecker : ITypeChecker");
            sb.AppendLine("{");
            sb.AppendLine("    public FifthType Infer(AstThing exp)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (exp == null) return default;");
            sb.AppendLine("        var scope = exp.NearestScope();");
            sb.AppendLine("        return exp switch");
            sb.AppendLine("        {");
            foreach (var type in filteredTypes)
            {
                sb.AppendLine($"            {type.Name} node => Infer(scope, node),");
            }
            sb.AppendLine("            { } node => throw new ast_model.TypeCheckingException(\"Unrecognised type\")");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            foreach (var type in filteredTypes)
            {
                sb.AppendLine($"    public abstract FifthType Infer(ScopeAstThing scope, {type.Name} node);");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        static string ExtractElementType(string typeName)
        {
            var match = Regex.Match(typeName, @"List<(.+)>");
            if (match.Success)
                return match.Groups[1].Value;

            match = Regex.Match(typeName, @"LinkedList<(.+)>");
            if (match.Success)
                return match.Groups[1].Value;

            return "";
        }
    }

    public class TypeInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public List<PropertyInfo> Properties { get; set; } = new();
    }

    public class PropertyInfo
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
        public bool IsCollection { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; } = "";
    }
}