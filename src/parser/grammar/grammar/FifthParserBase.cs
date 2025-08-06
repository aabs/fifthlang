using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

public abstract class FifthParserBase : Parser
{
    protected FifthParserBase(ITokenStream input)
        : base(input)
    {
    }

    protected FifthParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
    }

    protected bool IsLocalVariableDeclaration()
    {
        var local_var_decl = this.Context as FifthParser.Var_declContext;
        if (local_var_decl == null) return true;
        var local_variable_type = local_var_decl.type_name();
        if (local_variable_type == null) return true;
        if (local_variable_type.GetText() == "var") return false;
        return true;
    }
}
