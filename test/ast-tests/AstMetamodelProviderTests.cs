using ast;
using ast_model;
using ast_model.TypeSystem;
using FluentAssertions;

namespace ast_tests;

public class AstTypeProviderTests
{
    [Test]
    public void some_types_should_be_exported()
    {
        var sut = new TypeProvider<ast.AstThing>();
        sut.AllTypes.Should().NotBeEmpty();
    }

    [Test]
    public void some_ast_types_should_be_found()
    {
        var sut = new TypeProvider<ast.AstThing>();
        sut.AllAstTypes.Should().NotBeEmpty();
    }

    [Test]
    public void non_ignored_types_should_not_include_ignore_attribute()
    {
        var sut = new TypeProvider<ast.AstThing>();
        sut.NonIgnoredTypes.Should().NotContain(type => type.Name.Contains("IgnoreAttribute"));
    }
    [Test]
    public void concrete_types_should_not_include_ignore_attribute()
    {
        var sut = new TypeProvider<ast.AstThing>();
        sut.ConcreteTypes.Should().NotContain(type => type.Name.StartsWith("IgnoreAttribute"));
    }

    [Test]
    public void having_attribute_should_return_true_when_attribute_is_present_and_false_otherwise()
    {
        var t = GetType()
            .GetMember("having_attribute_should_return_true_when_attribute_is_present_and_false_otherwise").FirstOrDefault();
        t!.HavingAttribute<TestAttribute>().Should().BeTrue();
        t = GetType()
            .GetMember("dummy_method_with_no_attributes").FirstOrDefault();
        t!.HavingAttribute<TestAttribute>().Should().BeFalse();
    }

    void dummy_method_with_no_attributes() { }

    [Test]
    [Arguments(typeof(string), false)]
    [Arguments(typeof(int), false)]
    [Arguments(typeof(AstThing), true)]
    [Arguments(typeof(ast.AssemblyName), false)]
    [Arguments(typeof(TypeName), false)]
    public void IsAnAstThing_should_only_return_true_on_genuine_ast_types(Type t, bool shouldSucceed)
    {
        t.IsAnAstThing(typeof(ast.AstThing)).Should().Be(shouldSucceed);
    }

}

