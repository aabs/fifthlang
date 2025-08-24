namespace compiler.LanguageTransformations;

/// <summary>
/// Expands a property definition into a C#-style backing field and accessor functions.
/// </summary>
public class PropertyToFieldExpander : DefaultRecursiveDescentVisitor
{
    private bool _isVisitingPropertyInitializer = false;
    
    public override PropertyInitializerExp VisitPropertyInitializerExp(PropertyInitializerExp ctx)
    {
        // Set flag to indicate we're visiting a PropertyInitializerExp
        var wasVisitingPropertyInitializer = _isVisitingPropertyInitializer;
        _isVisitingPropertyInitializer = true;
        
        try
        {
            return base.VisitPropertyInitializerExp(ctx);
        }
        finally
        {
            _isVisitingPropertyInitializer = wasVisitingPropertyInitializer;
        }
    }
    
    public override PropertyDef VisitPropertyDef(PropertyDef ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        
        // If we're visiting a PropertyInitializerExp, skip expansion of placeholder PropertyDefs
        if (_isVisitingPropertyInitializer && (ctx.Parent == null || ctx.Parent is not ClassDef))
        {
            // This is a placeholder PropertyDef from PropertyInitializerExp
            // Return it unchanged without expansion
            return ctx;
        }
        
        // For regular PropertyDef objects that don't have a parent class, throw exception
        if (ctx.Parent == null || ctx.Parent is not ClassDef)
        {
            throw new InvalidOperationException("PropertyDef must have a ClassDef parent to be expanded into backing fields and accessors.");
        }
        
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
            .WithReturnType(ctx.Type)
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
