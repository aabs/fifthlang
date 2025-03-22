using ast;
using ast_generated; // Added namespace for Builder classes
using ast_model.TypeSystem;
using compiler.LanguageTransformations;

namespace ast_tests
{
    public class PropertyToFieldExpanderTests
    {
        [Fact]
        public void VisitPropertyDef_ShouldAddBackingField()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("TestProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Public)
                .Build();
            var classDef1 = new ClassDefBuilder().WithMemberDefs(new List<MemberDef>()).Build();
            propertyDef.Parent = classDef1;

            var expander = new PropertyToFieldExpander();

            // Act
            expander.VisitPropertyDef(propertyDef);

            // Assert
            var classDef = propertyDef.Parent as ClassDef;
            Assert.NotNull(classDef);
            Assert.Contains(classDef.MemberDefs, m => m is FieldDef && ((FieldDef)m).Name == "TestProperty__BackingField");
        }

        [Fact]
        public void VisitPropertyDef_ShouldAddGetterMethod()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("TestProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Public)
                .Build();
            var classDef1 = new ClassDefBuilder().WithMemberDefs(new List<MemberDef>()).Build();
            propertyDef.Parent = classDef1;

            var expander = new PropertyToFieldExpander();

            // Act
            expander.VisitPropertyDef(propertyDef);

            // Assert
            var classDef = propertyDef.Parent as ClassDef;
            Assert.NotNull(classDef);
            Assert.Contains(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "get_TestProperty");
        }

        [Fact]
        public void VisitPropertyDef_ShouldAddSetterMethod()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("TestProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Public)
                .Build();
            var classDef1 = new ClassDefBuilder().WithMemberDefs(new List<MemberDef>()).Build();
            propertyDef.Parent = classDef1;

            var expander = new PropertyToFieldExpander();

            // Act
            expander.VisitPropertyDef(propertyDef);

            // Assert
            var classDef = propertyDef.Parent as ClassDef;
            Assert.NotNull(classDef);
            Assert.Contains(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "set_TestProperty");
        }

        [Fact]
        public void VisitPropertyDef_ShouldHandleNullPropertyDef()
        {
            // Arrange
            var expander = new PropertyToFieldExpander();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => expander.VisitPropertyDef(null));
        }

        [Fact]
        public void VisitPropertyDef_ShouldHandlePrivateProperty()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("PrivateProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Private)
                .Build();
            var classDef1 = new ClassDefBuilder().WithMemberDefs(new List<MemberDef>()).Build();
            propertyDef.Parent = classDef1;

            var expander = new PropertyToFieldExpander();

            // Act
            expander.VisitPropertyDef(propertyDef);

            // Assert
            var classDef = propertyDef.Parent as ClassDef;
            Assert.NotNull(classDef);
            Assert.Contains(classDef.MemberDefs, m => m is FieldDef && ((FieldDef)m).Name == "PrivateProperty__BackingField");
            Assert.Contains(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "get_PrivateProperty");
            Assert.Contains(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "set_PrivateProperty");
        }

        [Fact]
        public void VisitPropertyDef_ShouldHandlePropertyWithoutParentClass()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("TestProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Public)
                .Build();

            var expander = new PropertyToFieldExpander();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => expander.VisitPropertyDef(propertyDef));
        }

        [Fact]
        public void VisitPropertyDef_ShouldHandleReadOnlyProperty()
        {
            // Arrange
            var propertyDef = new PropertyDefBuilder()
                .WithName(MemberName.From("ReadOnlyProperty"))
                .WithTypeName(TypeName.From("int"))
                .WithVisibility(Visibility.Public)
                .WithIsReadOnly(true)
                .Build();
            var classDef1 = new ClassDefBuilder().WithMemberDefs(new List<MemberDef>()).Build();
            propertyDef.Parent = classDef1;

            var expander = new PropertyToFieldExpander();

            // Act
            expander.VisitPropertyDef(propertyDef);

            // Assert
            var classDef = propertyDef.Parent as ClassDef;
            Assert.NotNull(classDef);
            Assert.Contains(classDef.MemberDefs, m => m is FieldDef && ((FieldDef)m).Name == "ReadOnlyProperty__BackingField");
            Assert.Contains(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "get_ReadOnlyProperty");
            Assert.DoesNotContain(classDef.MemberDefs, m => m is MethodDef && ((MethodDef)m).Name == "set_ReadOnlyProperty");
        }
    }
}
