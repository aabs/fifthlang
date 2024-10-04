namespace ast_model;

public class CompilationException : Exception
{
    public CompilationException()
    {
    }

    public CompilationException(string message) : base(message)
    {
    }

    public CompilationException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected CompilationException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[System.Serializable]
public class Exception : System.Exception
{
    public Exception()
    {
    }

    public Exception(string message) : base(message)
    {
    }

    public Exception(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected Exception(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class InvalidVariableReferenceException : RuntimeException
{
    public InvalidVariableReferenceException()
    {
    }

    public InvalidVariableReferenceException(string message) : base(message)
    {
    }

    public InvalidVariableReferenceException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected InvalidVariableReferenceException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class RuntimeException : Exception
{
    public RuntimeException()
    {
    }

    public RuntimeException(string message) : base(message)
    {
    }

    public RuntimeException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected RuntimeException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[System.Serializable]
public class TypeCheckingException : Exception
{
    public TypeCheckingException()
    {
    }

    public TypeCheckingException(string message) : base(message)
    {
    }

    public TypeCheckingException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected TypeCheckingException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[Serializable]
public class CodeGenerationException : Exception
{
    // For guidelines regarding the creation of new exception types, see
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

    public CodeGenerationException()
    { }

    public CodeGenerationException(string message) : base(message)
    {
    }

    public CodeGenerationException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected CodeGenerationException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}
