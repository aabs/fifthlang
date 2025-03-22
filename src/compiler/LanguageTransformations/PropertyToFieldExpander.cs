namespace compiler.LanguageTransformations;

/// <summary>
/// Expands a property definition into a C#-style backing field and accessor functions.
/// </summary>
public class PropertyToFieldExpander : DefaultRecursiveDescentVisitor
{
    public override PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        _ = ctx.Parent ?? throw new InvalidOperationException("Cannot expand an orphaned property definition.");
        // Create a backing field using FieldDefBuilder
        var field = new FieldDefBuilder()
            .WithName(MemberName.From($"{ctx.Name}__BackingField"))
            .WithTypeName(ctx.TypeName)
            .WithIsReadOnly(false)
            .WithVisibility(ctx.Visibility)
            .Build();

        // Add the field to the class definition
        var classDef = ctx.Parent as ClassDef;
        classDef.MemberDefs.Add(field);

        // Create getter function using FunctionDefBuilder
        var getter = new FunctionDefBuilder()
            .WithName(MemberName.From($"get_{ctx.Name}"))
            .WithReturnType(ctx.TypeName)
            .WithIsStatic(false)
            .WithVisibility(ctx.Visibility)
            .WithBody(new ast.BlockStatement
            {
                Statements = new List<ast.Statement>
                {
                    new ast_generated.ReturnStatementBuilder()
                        .WithReturnValue(new VarRefExp
                        {
                            VarName = field.Name.Value,
                            Type = ctx.Type
                        })
                        .Build()
                }
            })
            .Build();

        // Add the getter function to the class definition
        var getmethod = new MethodDef
        {
            Name = getter.Name,
            Parent = classDef,
            Type = ctx.Type,
            Visibility = ctx.Visibility,
            IsReadOnly = ctx.IsReadOnly,
            TypeName = ctx.TypeName,
            Annotations = ctx.Annotations,
            FunctionDef = getter,
            Location = ctx.Location
        };
        classDef.MemberDefs.Add(getmethod);

        if (!ctx.IsReadOnly)
        {
            var setter = new FunctionDefBuilder()
                .WithName(MemberName.From($"set_{ctx.Name}"))
                .WithReturnType(null)
                .WithIsStatic(false)
                .WithVisibility(ctx.Visibility)
                .WithParams(new List<ast.ParamDef>
                {
                new() {
                    Name = "value",
                    TypeName = ctx.TypeName,
                    Type = ctx.Type,
                    Annotations = ctx.Annotations,
                    Location = ctx.Location,
                    Parent = ctx,
                    Visibility = ctx.Visibility,
                    DestructureDef = null,
                    ParameterConstraint = null
                }
                })
                .WithBody(new ast.BlockStatement
                {
                    Statements = new List<ast.Statement>
                    {
                    new ast.AssignmentStatement
                    {
                        LValue = new VarRefExp
                        {
                            VarName = field.Name.Value,
                            Type = ctx.Type
                        },
                        RValue = new VarRefExp
                        {
                            VarName = "value",
                            Type = ctx.Type
                        }
                    }
                    }
                })
                .Build();

            // Add the setter function to the class definition
            var setmethod = new MethodDef
            {
                Name = setter.Name,
                Parent = classDef,
                Type = ctx.Type,
                Visibility = ctx.Visibility,
                IsReadOnly = ctx.IsReadOnly,
                TypeName = ctx.TypeName,
                Annotations = ctx.Annotations,
                FunctionDef = getter,
                Location = ctx.Location
            };
            classDef.MemberDefs.Add(setmethod);
        }
        // Create setter function using FunctionDefBuilder

        return base.VisitPropertyDef(ctx);
    }
}
