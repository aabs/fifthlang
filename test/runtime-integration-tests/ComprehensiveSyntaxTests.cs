using FluentAssertions;

namespace runtime_integration_tests;

/// <summary>
/// Comprehensive end-to-end tests for syntax features
/// Each test corresponds to a .5th file in TestPrograms/Syntax/
/// These tests focus on validating that syntax constructs parse and compile correctly
/// </summary>
public class ComprehensiveSyntaxTests : RuntimeTestBase
{
    #region Alias Tests

    [Test]
    public async Task alias_basic_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_basic.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task alias_domain_dots_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_domain_dots.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task alias_fragment_empty_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_fragment_empty.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task alias_fragment_named_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_fragment_named.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task alias_path_segments_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_path_segments.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task alias_trailing_slash_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "alias_trailing_slash.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Block Tests

    [Test]
    public async Task block_empty_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "block_empty.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task block_multiple_statements_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "block_multiple_statements.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task block_nested_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "block_nested.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task blocks_and_statements_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "blocks_and_statements.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Call Tests

    [Test]
    public async Task call_multi_arg_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "call_multi_arg.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task call_nested_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "call_nested.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task call_no_arg_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "call_no_arg.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task call_one_arg_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "call_one_arg.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Class Tests

    [Test]
    public async Task class_empty_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "class_empty.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task class_methods_duplicate_names_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "class_methods_duplicate_names.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task class_methods_only_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "class_methods_only.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task class_mixed_members_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "class_mixed_members.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task class_props_only_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "class_props_only.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Constraint Tests

    [Test]
    public async Task constraint_complex_expr_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "constraint_complex_expr.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(2, "f(2) where f(x: int | (x & 1) == 0) { return x; } should return 2");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task constraint_simple_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "constraint_simple.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(1, "f(1) where f(x: int | x > 0) { return x; } should return 1");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Destructuring Tests

    [Test]
    public async Task destructure_binding_constraint_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "destructure_binding_constraint.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task destructure_nested_with_constraints_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "destructure_nested_with_constraints.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Program and Empty Tests

    [Test]
    public async Task empty_program_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "empty_program.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "empty program should compile and run successfully");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task expressions_precedence_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "expressions_precedence.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Function Tests

    [Test]
    public async Task func_destructure_binding_constraint_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_destructure_binding_constraint.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_multi_params_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_multi_params.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(3, "main() should return 3 indicating successful addition");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_no_params_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_no_params.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_param_constraint_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_param_constraint.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(1, "f(1) where f(x: int | x > 0) { return x; } should return 1");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_param_destructure_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_param_destructure.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_param_nested_destructure_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_param_nested_destructure.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task func_single_param_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "func_single_param.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(2, "main() should return 2 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task function_params_constraints_destructuring_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "function_params_constraints_destructuring.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating constrained case got invoked");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Import Tests

    [Test]
    public async Task import_multiple_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "import_multiple.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task import_single_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "import_single.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task import_with_underscore_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "import_with_underscore.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task imports_aliases_mixed_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "imports_aliases_mixed.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region List and Collection Tests

    [Test]
    public async Task list_comprehension_simple_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_comprehension_simple.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task list_comprehension_with_constraint_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_comprehension_with_constraint.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task list_literal_multiple_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_literal_multiple.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task list_literal_nested_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_literal_nested.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(4, "main() should return x[1][1] which is 4");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task list_literal_single_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_literal_single.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task list_literal_with_exprs_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "list_literal_with_exprs.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task lists_and_comprehensions_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lists_and_comprehensions.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Literal Tests

    [Test]
    public async Task lit_bools_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lit_bools.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task lit_imaginary_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lit_imaginary.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task lit_ints_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lit_ints.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task lit_reals_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lit_reals.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task lit_strings_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "lit_strings.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task literals_all_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "literals_all.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Member Access and Indexing Tests

    [Test]
    public async Task member_and_index_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "member_and_index.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region New Expression Tests

    [Test]
    public async Task new_args_and_property_init_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "new_args_and_property_init.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task new_bare_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "new_bare.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task new_empty_args_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "new_empty_args.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task new_paramdecl_args_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "new_paramdecl_args.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task new_property_init_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "new_property_init.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task object_instantiation_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "object_instantiation.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Operator Tests

    [Test]
    public async Task op_additive_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_additive.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_call_then_access_index_ShouldFailCompilation()
    {
        // This test verifies that invalid member access on primitive types
        // is caught during compilation with meaningful diagnostics.
        // The code f().a[0] attempts to access member 'a' on int (returned by f())
        // which is not valid.
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_call_then_access_index.5th");
        
        // Act & Assert
        var act = async () => await CompileFileAsync(sourceFile);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("Compilation failed") && 
                        (ex.Message.Contains("TYPE_ERROR") || ex.Message.Contains("Cannot access member")));
    }

    [Test]
    public async Task op_indexing_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_indexing.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_logical_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_logical.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_member_access_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_member_access.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_mixed_access_index_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_mixed_access_index.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_multiplicative_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_multiplicative.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_parentheses_alter_precedence_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_parentheses_alter_precedence.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_power_right_assoc_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_power_right_assoc.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_precedence_chain_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_precedence_chain.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task op_relational_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "op_relational.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Program Structure Tests

    [Test]
    public async Task program_mixed_interleaved_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "program_mixed_interleaved.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Statement Tests

    [Test]
    public async Task stmt_array_typed_var_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_array_typed_var.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_assignment_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_assignment.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_else_if_chain_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_else_if_chain.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_empty_expr_stmt_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_empty_expr_stmt.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_expr_stmt_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_expr_stmt.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_if_no_else_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_if_no_else.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_if_with_else_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_if_with_else.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_list_typed_var_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_list_typed_var.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_return_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_return.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_var_decl_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_var_decl.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_var_init_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_var_init.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_while_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_while.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_with_block_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_with_block.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task stmt_with_single_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "stmt_with_single.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Trivia and Type Tests

    [Test]
    public async Task trivia_comments_whitespace_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "trivia_comments_whitespace.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task trivia_semicolons_required_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "trivia_semicolons_required.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task type_array_sized_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "type_array_sized.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task type_array_unsized_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "type_array_unsized.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task type_list_signature_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "type_list_signature.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task type_typename_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "type_typename.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Unary Tests

    [Test]
    public async Task unary_and_calls_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "unary_and_calls.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task unary_postfix_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "unary_postfix.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task unary_prefix_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "unary_prefix.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    #endregion

    #region Miscellaneous Tests

    [Test]
    public async Task with_and_comments_ShouldCompileAndReturnZero()
    {
        var sourceFile = Path.Combine("TestPrograms", "Syntax", "with_and_comments.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "main() should return 0 indicating successful execution");
        result.StandardError.Should().BeEmpty("No errors should occur during execution");
    }

    [Test]
    public async Task knowledge_management_declare_graph_ShouldCompileAndReturnZero()
    {
        // Validates augmented assignment 'store += graph;' desugars to KG.SaveGraph(store, graph)
        var sourceFile = Path.Combine("TestPrograms", "KnowledgeManagement", "declare-a-graph.5th");
        var executablePath = await CompileFileAsync(sourceFile);
        var result = await ExecuteAsync(executablePath);

        result.ExitCode.Should().Be(0, "program should compile and run successfully with graph save");
        result.StandardError.Should().BeEmpty("No runtime errors expected for graph declaration example");
    }

    #endregion
}