namespace ast_model
{
    public interface ISourceContext
    {
        int Column { get; init; }

        string Filename { get; init; }

        int Line { get; init; }

        string OriginalText { get; init; }
    }

    public record SourceContext(string Filename, int Line, int Column, string OriginalText) : ISourceContext;
}
