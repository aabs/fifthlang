# Parser Project

This project is responsible for the low-level parsing of fifth source code. It is built using the ANTLR 4 Parser generator framework. 

## generating the parser code

The csproj file contains a target called `GenerateParser`. That target invokes the ANTLR 4.8 Java jar file tooling to generate the parser source files into the `grammar` folder. The output folder for the generated source files for the low level parser is `grammar/grammar` and the target programming language is `Csharp`.

The key piece of non-generated to code is AstBuilderVisitor.cs which is responsible for passing a source file and using it to generate an AST based on the AST model defined in the AST-model project.

## Testing

Be very careful, when introduing changes, that the build does not produce any ANTLR error messages.