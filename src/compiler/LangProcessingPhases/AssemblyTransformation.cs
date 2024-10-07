namespace Fifth.LangProcessingPhases
{
    using System.Collections.Generic;
    using AST;
    using AST.Visitors;

    /// <summary>
    /// creates a new root on the AST for the assembly that will contain the program
    /// </summary>
    public class AssemblyTransformation : BaseAstVisitor
    {
        public AssemblyTransformation(string name, string strongNameKey = "", string versionNumber = "")
        {
            Assembly = new Assembly(name, "", "");
            Assembly.References = new List<AssemblyRef>();
        }

        public Assembly Assembly { get; set; }

        public override void EnterFifthProgram(FifthProgram ctx)
        {
            Assembly.Program = ctx;
        }
    }
}
