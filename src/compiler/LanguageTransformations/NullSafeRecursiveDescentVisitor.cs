using ast;
using ast_generated;

namespace compiler.LanguageTransformations
{
    /// <summary>
    /// A null-safe version of DefaultRecursiveDescentVisitor that prevents null returns from corrupting the AST
    /// </summary>
    public class NullSafeRecursiveDescentVisitor : DefaultRecursiveDescentVisitor
    {
        public override BinaryExp VisitBinaryExp(BinaryExp ctx)
        {
            // Safely visit LHS and RHS, falling back to original if Visit returns null
            var lhs = ctx.LHS;
            var rhs = ctx.RHS;
            
            if (lhs != null)
            {
                var visitedLhs = Visit((AstThing)lhs);
                if (visitedLhs is Expression expr)
                    lhs = expr;
                // If visitedLhs is null or not an Expression, keep the original lhs
            }
            
            if (rhs != null)
            {
                var visitedRhs = Visit((AstThing)rhs);
                if (visitedRhs is Expression expr)
                    rhs = expr;
                // If visitedRhs is null or not an Expression, keep the original rhs
            }
            
            return ctx with {
                LHS = lhs,
                RHS = rhs
            };
        }
        
        public override FuncCallExp VisitFuncCallExp(FuncCallExp ctx)
        {
            // Safely visit InvocationArguments
            List<ast.Expression> tmpInvocationArguments = [];
            foreach (var arg in ctx.InvocationArguments)
            {
                if (arg != null)
                {
                    var visitedArg = Visit(arg);
                    if (visitedArg is Expression expr)
                        tmpInvocationArguments.Add(expr);
                    else
                        tmpInvocationArguments.Add(arg); // Keep original if visit failed
                }
            }
            
            // Safely visit FunctionDef
            var functionDef = ctx.FunctionDef;
            if (functionDef != null)
            {
                var visitedFuncDef = Visit((AstThing)functionDef);
                if (visitedFuncDef is FunctionDef fd)
                    functionDef = fd;
                // If visitedFuncDef is null or not a FunctionDef, keep the original
            }
            
            return ctx with {
                FunctionDef = functionDef,
                InvocationArguments = tmpInvocationArguments
            };
        }
    }
}