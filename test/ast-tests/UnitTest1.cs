using ast_model;
using FluentAssertions;

namespace ast_tests
{
    public class UnitTest1
    {
        [Fact]
        public void some_types_should_be_exported()
        {
            AstMetamodelProvider.AllTypes.Should().NotBeEmpty();
        }        
        
        [Fact]
        public void some_ast_types_should_be_found()
        {
            AstMetamodelProvider.AllAstTypes.Should().NotBeEmpty();
        }  
        
        [Fact]
        public void non_ignored_types_should_not_include_ignore_attribute()
        {
            AstMetamodelProvider.NonIgnoredTypes.Should().NotContain(type => type.Name.Contains("IgnoreAttribute"));
        }        
        [Fact]
        public void concrete_types_should_not_include_ignore_attribute()
        {
            AstMetamodelProvider.ConcreteTypes.Should().NotContain(type => type.Name.StartsWith("IgnoreAttribute"));
        }
    }
}
