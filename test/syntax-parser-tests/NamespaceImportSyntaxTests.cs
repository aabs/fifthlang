using FluentAssertions;
using syntax_parser_tests.Utils;
using Fifth;

namespace syntax_parser_tests;

/// <summary>
/// Tests for namespace declaration and import directive syntax parsing.
/// Ensures the parser accepts new namespace/import syntax and rejects legacy 'use' syntax.
/// </summary>
public class NamespaceImportSyntaxTests
{
    [Test]
    public void NamespaceDeclaration_ShouldParse()
    {
        var input = "namespace MyLib.Core;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.program(),
            "File-scoped namespace declaration should parse successfully");
    }

    [Test]
    public void NamespaceDeclaration_WithQualifiedName_ShouldParse()
    {
        var input = "namespace System.Collections.Generic;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.program(),
            "Namespace declaration with multi-segment qualified name should parse");
    }

    [Test]
    public void ImportDirective_ShouldParse()
    {
        var input = "import MyLib.Core;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.program(),
            "Import directive should parse successfully");
    }

    [Test]
    public void ImportDirective_WithQualifiedName_ShouldParse()
    {
        var input = "import System.Collections.Generic;";
        ParserTestUtils.AssertNoErrors(input + "\n", p => p.program(),
            "Import directive with multi-segment qualified name should parse");
    }

    [Test]
    public void MultipleImportDirectives_ShouldParse()
    {
        var input = @"
import System.Collections;
import MyLib.Core;
import External.Utilities;
";
        ParserTestUtils.AssertNoErrors(input, p => p.program(),
            "Multiple import directives should parse successfully");
    }

    [Test]
    public void NamespaceAndImport_Together_ShouldParse()
    {
        var input = @"
namespace App.Core;
import System.Collections;
import MyLib.Utilities;

main(): int {
    return 0;
}
";
        ParserTestUtils.AssertNoErrors(input, p => p.program(),
            "Namespace declaration with import directives should parse successfully");
    }

    [Test]
    public void LegacyUseSyntax_ShouldFail()
    {
        var input = "use MyOldModule;";
        ParserTestUtils.AssertHasErrors(input + "\n", p => p.program(),
            "Legacy 'use' syntax should be rejected by the parser");
    }

    [Test]
    public void LegacyUseSyntax_WithMultipleModules_ShouldFail()
    {
        var input = "use Module1, Module2, Module3;";
        ParserTestUtils.AssertHasErrors(input + "\n", p => p.program(),
            "Legacy 'use' syntax with multiple modules should be rejected");
    }

    [Test]
    public void MultipleNamespaceDeclarations_ShouldFail()
    {
        var input = @"
namespace First.Namespace;
namespace Second.Namespace;
";
        ParserTestUtils.AssertHasErrors(input, p => p.program(),
            "Multiple namespace declarations in a single file should be rejected");
    }

    [Test]
    public void NamespaceDeclaration_MustBeFirstStatement()
    {
        var input = @"
x : int = 5;
namespace MyLib.Core;
";
        ParserTestUtils.AssertHasErrors(input, p => p.program(),
            "Namespace declaration after other statements should be rejected");
    }

    [Test]
    public void ImportDirective_AfterNamespace_ShouldParse()
    {
        var input = @"
namespace App.Core;
import MyLib.Utilities;
";
        ParserTestUtils.AssertNoErrors(input, p => p.program(),
            "Import directive immediately following namespace declaration should parse");
    }

    [Test]
    public void ImportDirective_BeforeNamespace_ShouldFail()
    {
        var input = @"
import MyLib.Utilities;
namespace App.Core;
";
        ParserTestUtils.AssertHasErrors(input, p => p.program(),
            "Import directive before namespace declaration should be rejected");
    }

    [Test]
    public void EmptyNamespace_ShouldFail()
    {
        var input = "namespace ;";
        ParserTestUtils.AssertHasErrors(input + "\n", p => p.program(),
            "Namespace declaration without a name should be rejected");
    }

    [Test]
    public void EmptyImport_ShouldFail()
    {
        var input = "import ;";
        ParserTestUtils.AssertHasErrors(input + "\n", p => p.program(),
            "Import directive without a namespace name should be rejected");
    }
}
