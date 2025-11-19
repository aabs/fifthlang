using System;
using System.Collections.Generic;
using System.Linq;
using ast;
using compiler;
using compiler.LanguageTransformations;

namespace compiler.SemanticAnalysis
{
    /// <summary>
    /// Validates base constructor requirements for inheritance scenarios.
    /// Ensures derived class constructors invoke base constructors when required.
    /// Detects inheritance cycles in constructor chains.
    /// </summary>
    public class BaseConstructorValidator : DefaultRecursiveDescentVisitor
    {
        private readonly List<Diagnostic> _diagnostics = new();
        private readonly HashSet<string> _visitedForCycle = new(); // Track classes validated for cycles

        public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

        public BaseConstructorValidator()
        {
        }

        public override ClassDef VisitClassDef(ClassDef ctx)
        {
            // Check each constructor in this class
            var constructors = ctx.MemberDefs.OfType<FunctionDef>()
                .Where(f => f.IsConstructor)
                .ToList();

            foreach (var constructor in constructors)
            {
                ValidateBaseConstructorRequirement(ctx, constructor);
            }

            return base.VisitClassDef(ctx);
        }

        private void ValidateBaseConstructorRequirement(ClassDef classDef, FunctionDef constructor)
        {
            // Skip if class has no base class
            if (classDef.BaseClassName == null)
            {
                return;
            }

            // Look up base class via symbol table through AST node's scope
            var scope = classDef.NearestScopeAbove();
            var baseClassSymbol = scope?.ResolveByName(classDef.BaseClassName);
            if (baseClassSymbol == null || baseClassSymbol.OriginatingAstThing is not ClassDef baseClass)
            {
                // Base class not found - this should be caught by type checker
                return;
            }

            // Check for inheritance cycles
            if (!CheckForInheritanceCycle(classDef, baseClass))
            {
                return; // Cycle detected, diagnostic already emitted
            }

            // Check if base class has a parameterless constructor
            var baseConstructors = baseClass.MemberDefs.OfType<FunctionDef>()
                .Where(f => f.IsConstructor)
                .ToList();

            bool hasParameterlessBaseConstructor = false;
            if (baseConstructors.Count == 0)
            {
                // No explicit constructors - implicit parameterless constructor exists
                hasParameterlessBaseConstructor = true;
            }
            else
            {
                // Check if any explicit constructor is parameterless
                hasParameterlessBaseConstructor = baseConstructors.Any(c => 
                    (c.Params == null || c.Params.Count == 0));
            }

            // If base class has no parameterless constructor, derived constructor must have base call
            if (!hasParameterlessBaseConstructor)
            {
                if (constructor.BaseCall == null)
                {
                    // Emit CTOR004: Missing base constructor call
                    var diagnostic = ConstructorDiagnostics.MissingBaseConstructorCall(
                        classDef.Name.Name,
                        baseClass.Name.Name,
                        constructor
                    );
                    _diagnostics.Add(diagnostic);
                }
            }

            // If constructor has a base call, validate it resolves to a valid base constructor
            if (constructor.BaseCall != null)
            {
                ValidateBaseCall(classDef, baseClass, constructor);
            }
        }

        private bool CheckForInheritanceCycle(ClassDef currentClass, ClassDef baseClass)
        {
            // Track the path for cycle detection
            var visited = new HashSet<string>();
            var path = new List<string>();
            
            return CheckCycleRecursive(currentClass, visited, path);
        }

        private bool CheckCycleRecursive(ClassDef currentClass, HashSet<string> visited, List<string> path)
        {
            var className = currentClass.Name.Name;
            
            // If we've seen this class in the current path, we have a cycle
            if (path.Contains(className))
            {
                // Build cycle path for diagnostic
                var cycleStart = path.IndexOf(className);
                var cyclePath = string.Join(" -> ", path.Skip(cycleStart).Append(className));
                
                var diagnostic = ConstructorDiagnostics.CyclicBaseConstructor(
                    className,
                    cyclePath,
                    currentClass
                );
                _diagnostics.Add(diagnostic);
                return false; // Cycle detected
            }

            // If we've already fully validated this class (in previous branches), it's OK
            if (visited.Contains(className))
            {
                return true;
            }

            // Add to current path
            path.Add(className);

            // Check base class if it exists
            if (currentClass.BaseClassName != null)
            {
                var scope = currentClass.NearestScopeAbove();
                var baseClassSymbol = scope?.ResolveByName(currentClass.BaseClassName);
                if (baseClassSymbol?.OriginatingAstThing is ClassDef baseClass)
                {
                    if (!CheckCycleRecursive(baseClass, visited, path))
                    {
                        return false; // Cycle found in chain
                    }
                }
            }

            // Remove from current path, mark as fully visited
            path.RemoveAt(path.Count - 1);
            visited.Add(className);
            
            return true; // No cycle
        }

        private void ValidateBaseCall(ClassDef derivedClass, ClassDef baseClass, FunctionDef constructor)
        {
            // Get base call arguments
            var baseCallArgs = constructor.BaseCall?.Arguments ?? new List<FifthAstExpression>();
            int argCount = baseCallArgs.Count;

            // Find matching base constructors
            var baseConstructors = baseClass.MemberDefs.OfType<FunctionDef>()
                .Where(f => f.IsConstructor)
                .ToList();

            // Check if any base constructor matches the argument count
            var matchingConstructors = baseConstructors
                .Where(c => (c.Params?.Count ?? 0) == argCount)
                .ToList();

            if (matchingConstructors.Count == 0)
            {
                // No matching base constructor - will be caught by ConstructorResolver
                // which emits CTOR001 for the base call
            }
        }
    }
}
