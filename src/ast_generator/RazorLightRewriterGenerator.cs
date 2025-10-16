using ast_model;
using RazorLight;

namespace ast_generator;

public class RazorLightRewriterGenerator
{
    private readonly ITypeProvider _typeProvider;
    private readonly IRazorLightEngine _engine;

    public RazorLightRewriterGenerator(ITypeProvider typeProvider)
    {
        _typeProvider = typeProvider;
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(RazorLightRewriterGenerator))
            .UseMemoryCachingProvider()
            .DisableEncoding() // Important: disable HTML encoding for C# code generation
            .Build();
    }

    public async Task<string> TransformTextAsync()
    {
        var model = new TemplateModel(_typeProvider);
        return await _engine.CompileRenderAsync("Templates.Rewriter.cshtml", model);
    }
}
