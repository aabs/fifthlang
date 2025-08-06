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

            // Match public record declarations but exclude record structs
            var recordRegex = new Regex(@"public\s+record\s+(?!struct\s+)(\w+)\s*(?::\s*\w+)?");
            var matches = recordRegex.Matches(content);

            var csharpKeywords = new HashSet<string> 
            { 
                "struct", "class", "interface", "abstract", "sealed", "static", 
                "public", "private", "protected", "internal", "virtual", "override"
            };

            foreach (Match match in matches)
            {
                var typeName = match.Groups[1].Value;
                
                // Skip C# keywords and abstract records
                if (csharpKeywords.Contains(typeName.ToLower()) || match.Value.Contains("abstract")) 
                    continue;

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
            
            // Find the record definition - handle both single line and multi-line definitions
            var singleLineRegex = new Regex($@"public\s+record\s+{Regex.Escape(typeName)}\s*(?::\s*\w+)?\s*;");
            var multiLineRegex = new Regex($@"public\s+record\s+{Regex.Escape(typeName)}\s*(?::\s*[^{{]*)?\s*\{{(.*?)\}}", RegexOptions.Singleline);
            
            var singleLineMatch = singleLineRegex.Match(content);
            if (singleLineMatch.Success)
            {
                // This is a simple record without properties - no properties to extract
                return properties;
            }
            
            var multiLineMatch = multiLineRegex.Match(content);
            if (multiLineMatch.Success)
            {
                var body = multiLineMatch.Groups[1].Value;
                
                // Extract properties - handle { get; init; } and { get; set; } and defaults
                var propRegex = new Regex(@"public\s+(?:required\s+)?([^{}\s]+(?:<[^>]+>)?)\s+(\w+)\s*\{\s*[^}]*\}(?:\s*=\s*[^;]+)?");
                var propMatches = propRegex.Matches(body);
                
                foreach (Match propMatch in propMatches)
                {
                    var propType = propMatch.Groups[1].Value.Trim();
                    var propName = propMatch.Groups[2].Value;
                    
                    // Skip invalid property types or attributes
                    if (string.IsNullOrEmpty(propType) || string.IsNullOrEmpty(propName) || 
                        propType.StartsWith("[") || propName.StartsWith("["))
                        continue;
                    
                    properties.Add(new PropertyInfo
                    {
                        Name = propName,
                        TypeName = propType,
                        IsCollection = propType.Contains("List<") || propType.Contains("LinkedList<")
                    });
                }
            }

            return properties;
        }

        static string GenerateBuilders(List<TypeInfo> types, string namespaceName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName}_generated;");
            sb.AppendLine($"using ast_generated;");
            sb.AppendLine($"using {namespaceName};");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("#nullable disable");
            sb.AppendLine();

            foreach (var type in types)
            {
                sb.AppendLine($"public class {type.Name}Builder : IBuilder<{type.FullName}>");
                sb.AppendLine("{");

                // Generate private fields
                foreach (var prop in type.Properties)
                {
                    sb.AppendLine($"    private {prop.TypeName} _{prop.Name};");
                }

                sb.AppendLine();
                sb.AppendLine($"    public {type.FullName} Build()");
                sb.AppendLine("    {");
                sb.AppendLine($"        return new {type.FullName}()");
                sb.AppendLine("        {");

                for (int i = 0; i < type.Properties.Count; i++)
                {
                    var prop = type.Properties[i];
                    var separator = i == type.Properties.Count - 1 ? "" : ",";
                    sb.AppendLine($"            {prop.Name} = this._{prop.Name}{separator}");
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

            // Generate IAstVisitor interface
            sb.AppendLine("public interface IAstVisitor");
            sb.AppendLine("{");
            foreach (var type in types)
            {
                sb.AppendLine($"    public void Enter{type.Name}({type.Name} ctx);");
                sb.AppendLine($"    public void Leave{type.Name}({type.Name} ctx);");
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // Generate BaseAstVisitor
            sb.AppendLine("public partial class BaseAstVisitor : IAstVisitor");
            sb.AppendLine("{");
            foreach (var type in types)
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

            // Generate ITypeChecker interface
            sb.AppendLine("public interface ITypeChecker");
            sb.AppendLine("{");
            foreach (var type in types)
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
            foreach (var type in types)
            {
                sb.AppendLine($"            {type.Name} node => Infer(scope, node),");
            }
            sb.AppendLine("            { } node => throw new ast_model.TypeCheckingException(\"Unrecognised type\")");
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();

            foreach (var type in types)
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
    }
}