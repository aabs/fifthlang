using ast_model;
using ast;
using FluentAssertions;

namespace ast_tests;

public class AstTypeProviderTests
{
  [Fact]
  public void some_types_should_be_exported()
  {
    AstTypeProvider.AllTypes.Should().NotBeEmpty();
  }

  [Fact]
  public void some_ast_types_should_be_found()
  {
    AstTypeProvider.AllAstTypes.Should().NotBeEmpty();
  }

  [Fact]
  public void non_ignored_types_should_not_include_ignore_attribute()
  {
    AstTypeProvider.NonIgnoredTypes.Should().NotContain(type => type.Name.Contains("IgnoreAttribute"));
  }
  [Fact]
  public void concrete_types_should_not_include_ignore_attribute()
  {
    AstTypeProvider.ConcreteTypes.Should().NotContain(type => type.Name.StartsWith("IgnoreAttribute"));
  }

  [Fact]
  public void having_attribute_should_return_true_when_attribute_is_present_and_false_otherwise()
  {
      var t = GetType()
          .GetMember("having_attribute_should_return_true_when_attribute_is_present_and_false_otherwise").FirstOrDefault();
      t!.HavingAttribute<FactAttribute>().Should().BeTrue();
      t = GetType()
          .GetMember("dummy_method_with_no_attributes").FirstOrDefault();
      t!.HavingAttribute<FactAttribute>().Should().BeFalse();
  }

    void dummy_method_with_no_attributes(){}

    [Theory]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(AstThing), true)]
    [InlineData(typeof(ast.AssemblyName), false)]
    [InlineData(typeof(ast.TypeName), false)]
    public void IsAnAstThing_should_only_return_true_on_genuine_ast_types(Type t, bool shouldSucceed)
    {
        t.IsAnAstThing().Should().Be(shouldSucceed);
    }

}

