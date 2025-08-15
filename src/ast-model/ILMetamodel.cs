// ReSharper disable once CheckNamespace

namespace il_ast;

public abstract record AstThing;

#region Flags

public enum MemberType
{
    Unknown,
    Field,
    Property,
    Method
}

// II.23.1.10
public enum MemberAccessability
{
    CompilerControlled, // 0x0000 Member not referenceable
    Private, // 0x0001 Accessible only by the parent type
    FamANDAssem, // 0x0002 Accessible by sub-types only in this Assembly
    Assem, // 0x0003 Accessibly by anyone in the Assembly
    Family, // 0x0004 Accessible only by type and sub-types
    FamORAssem, // 0x0005 Accessibly by sub-types anywhere, plus anyone in assembly
    Public // 0x0006 Accessibly by anyone who has visibility to this scope
}

public enum VtableLayoutMask
{
    ReuseSlot, // 0x0000 Method reuses existing slot in vtable
    NewSlot // 0x0100 Method always gets a new slot in the vtable
}

public enum InOutFlag
{
    In, Out, Opt
}

public enum CodeTypeFlag
{
    cil,
    native,
    optil,
    runtime
}

public enum ImplementationFlag
{
    forwardRef,
    preserveSig,
    internalCall,
    synchronized,
    oninlining
}

public enum MethodCallingConvention
{
    Default /*i.e. static */,
    Vararg,
    Instance,
    InstanceVararg,
    Property
}

public enum FunctionKind
{
    Normal,
    Ctor,
    Getter,
    Setter
}

#endregion

#region Assemblies

public record AssemblyDeclaration : AstThing
{
    public string Name { get; set; }
    public Version Version { get; set; }
    public ModuleDeclaration PrimeModule { get; set; }
    public List<AssemblyReference> AssemblyReferences { get; set; } = new();
}

public record AssemblyReference : AstThing
{
    public string Name { get; set; }
    public string PublicKeyToken { get; set; }
    public Version Version { get; set; }
}

public record ModuleDeclaration : AstThing
{
    public string FileName { get; set; }
    public List<ClassDefinition> Classes { get; set; } = new();
    public List<MethodDefinition> Functions { get; set; } = new();
}

public record Version : AstThing
{
    public Version(int Major, int? Minor, int? Build, int? Patch)
    {
        this.Major = Major;
        this.Minor = Minor;
        this.Build = Build;
        this.Patch = Patch;
    }

    public Version() : this(0, 0, 0, 0) { }

    public Version(string s)
    {
        var segs = Array.Empty<string>();
        if (s.Contains('.'))
        {
            segs = s.Split('.');
        }
        else if (s.Contains(':'))
        {
            segs = s.Split(':');
        }

        if (segs.Length > 0)
        {
            if (int.TryParse(segs[0], out var major))
            {
                Major = major;
            }
        }

        if (segs.Length > 1)
        {
            if (int.TryParse(segs[1], out var minor))
            {
                Minor = minor;
            }
        }

        if (segs.Length > 2)
        {
            if (int.TryParse(segs[2], out var build))
            {
                Build = build;
            }
        }

        if (segs.Length > 3)
        {
            if (int.TryParse(segs[3], out var patch))
            {
                Patch = patch;
            }
        }
    }

    public int Major { get; set; }
    public int? Minor { get; set; }
    public int? Build { get; set; }
    public int? Patch { get; set; }

    public void Deconstruct(out int Major, out int? Minor, out int? Build, out int? Patch)
    {
        Major = this.Major;
        Minor = this.Minor;
        Build = this.Build;
        Patch = this.Patch;
    }
}

#endregion

#region Classes

public record ClassDefinition : AstThing
{
    public List<FieldDefinition> Fields { get; set; } = new();
    public List<PropertyDefinition> Properties { get; set; } = new();
    public List<MethodDefinition> Methods { get; set; } = new();
    public string Name { get; set; }
    public string Namespace { get; set; }
    public List<ClassDefinition> BaseClasses { get; set; } = new();
    public AssemblyDeclaration ParentAssembly { get; set; }
    public MemberAccessability Visibility { get; set; } = MemberAccessability.Private;
}

#endregion

#region Members

[Ignore]
public abstract record MemberDefinition : AstThing
{
    public string Name { get; set; }
    public TypeReference TheType { get; set; }
    public ClassDefinition ParentClass { get; set; }
    public PropertyDefinition AssociatedProperty { get; set; }
    public MemberType TypeOfMember { get; set; }
    public bool IsStatic { get; set; }
    public bool IsFinal { get; set; }
    public bool IsVirtual { get; set; }

    public bool IsStrict { get; set; }
    public bool IsAbstract { get; set; }

    public bool IsSpecialName { get; set; }

    // Method hides by name+sig, else just by name
    public bool HideBySig { get; set; }
    public MemberAccessability Visibility { get; set; }
}


public enum MemberTarget
{
    Method, Field
}

#endregion

#region Methods

public record ParameterDeclaration : AstThing
{
    public string Name { get; set; }
    public string TypeName { get; set; }
    public bool IsUDTType { get; set; }
}

public record ParameterSignature : AstThing
{
    public InOutFlag InOut { get; set; }
    public string Name { get; set; }
    public TypeReference TypeReference { get; set; }
    public bool IsUDTType { get; set; } // internal use
}

//Two method signatures are said to match if and only if:
//     the calling conventions are identical;
//     both signatures are either static or instance;
//     the number of generic parameters is identical, if the method is generic;
//     for instance signatures the type of the this pointer of the overriding/hiding
//signature is assignable-to (§I.8.7) the type of the this pointer of the
//overridden/hidden signature;
//     the number and type signatures of the parameters are identical; and
//     the type signatures for the result are identical. [Note: This includes void
//(§II.23.2.11) if no value is returned.end note]
public record MethodSignature : AstThing
{
    public MethodCallingConvention CallingConvention { get; set; }
    public ushort NumberOfParameters { get; set; }
    public List<ParameterSignature> ParameterSignatures { get; set; } = new();
    public TypeReference ReturnTypeSignature { get; set; }
}

public record MethodHeader : AstThing
{
    public FunctionKind FunctionKind { get; set; }
    public bool IsEntrypoint { get; set; }
}

public record MethodRef : MemberRef
{
}

public record MethodImpl : AstThing
{
    public ImplementationFlag ImplementationFlags { get; set; }
    public bool IsManaged { get; set; }

    public Block Body { get; set; }
}

public record MethodDefinition : MemberDefinition
{
    public MethodHeader Header { get; set; }
    public MethodSignature Signature { get; set; }
    public MethodImpl Impl { get; set; }
    public CodeTypeFlag CodeTypeFlags { get; set; }
}

public record TypeReference : AstThing
{
    public string Namespace { get; set; }
    public string Name { get; set; }
    public string ModuleName { get; set; }
}
public record MemberRef : AstThing
{
    public MemberTarget Target { get; set; }
    public ClassDefinition ClassDefinition { get; set; }
    public string Name { get; set; }
    public MethodSignature Sig { get; set; }
    public FieldDefinition Field { get; set; }
}


#endregion

#region Fields

public record FieldDefinition : MemberDefinition
{
}

#endregion

#region Properties

public record PropertyDefinition : MemberDefinition
{
    public string TypeName { get; set; }
    public string Name { get; set; }
    public FieldDefinition? FieldDefinition { get; set; }
}

#endregion

#region Instructions

/// <summary>
/// Base type for all CIL instructions that map directly to IL opcodes
/// </summary>
[Ignore]
public abstract record CilInstruction : AstThing
{
    public abstract string Opcode { get; }
}

/// <summary>
/// Load instructions (ldc.i4, ldloc, ldarg, ldstr, etc.)
/// </summary>
public record LoadInstruction : CilInstruction
{
    public LoadInstruction() { }
    public LoadInstruction(string opcode, object? value = null)
    {
        _opcode = opcode;
        Value = value;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
    public object? Value { get; init; }
}

/// <summary>
/// Store instructions (stloc, stfld, starg, etc.)
/// </summary>
public record StoreInstruction : CilInstruction
{
    public StoreInstruction() { }
    public StoreInstruction(string opcode, string? target = null)
    {
        _opcode = opcode;
        Target = target;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
    public string? Target { get; init; }
}

/// <summary>
/// Arithmetic and logical instructions (add, sub, mul, div, ceq, clt, etc.)
/// </summary>
public record ArithmeticInstruction : CilInstruction
{
    public ArithmeticInstruction() { }
    public ArithmeticInstruction(string opcode)
    {
        _opcode = opcode;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
}

/// <summary>
/// Branch instructions (br, brtrue, brfalse, etc.)
/// </summary>
public record BranchInstruction : CilInstruction
{
    public BranchInstruction() { }
    public BranchInstruction(string opcode, string? targetLabel = null)
    {
        _opcode = opcode;
        TargetLabel = targetLabel;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
    public string? TargetLabel { get; init; }
}

/// <summary>
/// Call instructions (call, callvirt, newobj, etc.)
/// </summary>
public record CallInstruction : CilInstruction
{
    public CallInstruction() { }
    public CallInstruction(string opcode, string? methodSignature = null)
    {
        _opcode = opcode;
        MethodSignature = methodSignature;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
    public string? MethodSignature { get; init; }
}

/// <summary>
/// Stack manipulation instructions (dup, pop, etc.)
/// </summary>
public record StackInstruction : CilInstruction
{
    public StackInstruction() { }
    public StackInstruction(string opcode)
    {
        _opcode = opcode;
    }

    private readonly string _opcode = "";
    public override string Opcode => _opcode;
}

/// <summary>
/// Return instruction
/// </summary>
public record ReturnInstruction : CilInstruction
{
    public override string Opcode => "ret";
}

/// <summary>
/// Label marker for branch targets
/// </summary>
public record LabelInstruction : CilInstruction
{
    public LabelInstruction() { }
    public LabelInstruction(string label)
    {
        Label = label;
    }

    public override string Opcode => "";
    public string Label { get; init; } = "";
}

/// <summary>
/// Container for a sequence of CIL instructions
/// </summary>
public record InstructionSequence : AstThing
{
    public List<CilInstruction> Instructions { get; set; } = new();
    
    public void Add(CilInstruction instruction)
    {
        Instructions.Add(instruction);
    }
    
    public void AddRange(IEnumerable<CilInstruction> instructions)
    {
        Instructions.AddRange(instructions);
    }
}

#endregion

#region Statements

/// <summary>
/// Statement that contains a sequence of CIL instructions
/// This is the bridge between high-level IL constructs and instruction-level IL
/// </summary>
public record InstructionStatement : AstThing
{
    public InstructionSequence Instructions { get; set; } = new();
}

#endregion

#region Block and Method Body

public record Block : AstThing
{
    public List<ast.Statement> Statements { get; set; } = new();
}

#endregion
