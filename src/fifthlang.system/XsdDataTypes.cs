namespace Fifth.System;

/// <summary>
/// Common XSD data type URIs.
/// </summary>
public static class XsdDataTypes
{
    public static readonly Uri String = UriFactory.Create("http://www.w3.org/2001/XMLSchema#string");
    public static readonly Uri Int = UriFactory.Create("http://www.w3.org/2001/XMLSchema#int");
    public static readonly Uri Boolean = UriFactory.Create("http://www.w3.org/2001/XMLSchema#boolean");
    public static readonly Uri DateTime = UriFactory.Create("http://www.w3.org/2001/XMLSchema#dateTime");
    public static readonly Uri Decimal = UriFactory.Create("http://www.w3.org/2001/XMLSchema#decimal");
    public static readonly Uri Float = UriFactory.Create("http://www.w3.org/2001/XMLSchema#float");
    public static readonly Uri Double = UriFactory.Create("http://www.w3.org/2001/XMLSchema#double");
    public static readonly Uri Long = UriFactory.Create("http://www.w3.org/2001/XMLSchema#long");
    public static readonly Uri Short = UriFactory.Create("http://www.w3.org/2001/XMLSchema#short");
    public static readonly Uri Byte = UriFactory.Create("http://www.w3.org/2001/XMLSchema#byte");
    public static readonly Uri UnsignedInt = UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedInt");
    public static readonly Uri UnsignedLong = UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedLong");
    public static readonly Uri UnsignedShort = UriFactory.Create("http://www.w3.org/2001/XMLSchema#unsignedShort");
}
