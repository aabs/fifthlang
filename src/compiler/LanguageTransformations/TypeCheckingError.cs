namespace Fifth.Parser.LangProcessingPhases
{
    using TypeSystem;

    public record FifthCompilationError
    {
        public FifthCompilationError(string message,
            string filename,
            int line,
            int column)
        {
            Message = message;
            Filename = filename;
            Line = line;
            Column = column;
        }

        public string Message { get; init; }
        public string Filename { get; init; }
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public record TypeCheckingError : FifthCompilationError
    {
        public TypeCheckingError(
            string message,
            string filename,
            int line,
            int column,
            IType[] types = default) : base(message, filename, line, column)
        {
            Types = types;
        }

        public IType[] Types { get; init; }
    }
}
