namespace Fifth.LangProcessingPhases;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AST;
using AST.Builders;
using AST.Visitors;
using fifth.metamodel.metadata;

/// <summary>
/// Expands a property definition into a C#-style backing field and accessor functions.
/// </summary>
public class PropertyToFieldExpander : BaseAstVisitor
{
    public override void EnterPropertyDefinition(PropertyDefinition ctx)
    {
        // define a backing field
        var f = FieldDefinitionBuilder.CreateFieldDefinition()
                                      .WithName($"{ctx.Name}__BackingField")
                                      .WithTypeName(ctx.TypeName)
                                      .WithBackingFieldFor(ctx)
                                      .Build();
        var cd = ctx.ParentNode as ClassDefinition;
        cd.Fields.Add(f);
        // annotate the property with the details of the field
        ctx.BackingField = f;
        // create accessor functions for the property
        var getter = FunctionDefinitionBuilder.CreateFunctionDefinition()
                                              .WithName($"get_{ctx.Name}")
                                              .WithFunctionKind(FunctionKind.Getter)
                                              .WithTypename(ctx.TypeName)
                                              .WithIsEntryPoint(false)
                                              .WithParameterDeclarations(new ParameterDeclarationList(new List<IParameterListItem>()))
                                              .WithBody(
                                                  BlockBuilder.CreateBlock()
                                                              .AddingItemToStatements(
                                                                  ReturnStatementBuilder.CreateReturnStatement()
                                                                      .WithSubExpression(
                                                                          VariableReferenceBuilder
                                                                              .CreateVariableReference()
                                                                              .WithName(f.Name)
                                                                              .Build()
                                                                      )
                                                                      .Build()
                                                              )
                                                              .Build()
                                              )
                                              .Build();
        getter["propdecl"] = ctx;
        cd.Functions.Add(getter);
        ctx.GetAccessor = getter;
        var setter = FunctionDefinitionBuilder.CreateFunctionDefinition()
                                              .WithName($"set_{ctx.Name}")
                                              .WithFunctionKind(FunctionKind.Setter)
                                              .WithParameterDeclarations(new ParameterDeclarationList(
                                                  new List<IParameterListItem>
                                                  {
                                                      ParameterDeclarationBuilder.CreateParameterDeclaration()
                                                          .WithTypeName(ctx.TypeName)
                                                          .WithParameterName(new Identifier("value"))
                                                          .Build()
                                                  })
                                              )
                                              .WithBody(BlockBuilder.CreateBlock()
                                                                    .AddingItemToStatements(AssignmentStmtBuilder
                                                                        .CreateAssignmentStmt()
                                                                        .WithVariableRef(new VariableReference(f.Name))
                                                                        .WithExpression(new VariableReference("value"))
                                                                        .Build())
                                                                    .Build()
                                              )
                                              .Build();
        setter["propdecl"] = ctx;
        cd.Functions.Add(setter);
        ctx.SetAccessor = setter;
    }
}
