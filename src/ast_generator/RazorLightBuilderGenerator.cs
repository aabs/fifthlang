using ast_model;
using RazorLight;

namespace ast_generator;

public class RazorLightBuilderGenerator
{
    private readonly ITypeProvider _typeProvider;
    private readonly IRazorLightEngine _engine;

    public RazorLightBuilderGenerator(ITypeProvider typeProvider)
    {
        _typeProvider = typeProvider;
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(RazorLightBuilderGenerator))
            .UseMemoryCachingProvider()
            .DisableEncoding() // Important: disable HTML encoding for C# code generation
            .Build();
    }

    public async Task<string> TransformTextAsync()
    {
        var model = new TemplateModel(_typeProvider);
        return await _engine.CompileRenderAsync("Templates.Builders.cshtml", model);
    }
}