namespace compiler;

using System.Collections.Generic;

/// <summary>
/// Represents a mapping from a lowered-AST node to generated source coordinates.
/// Kept intentionally simple for the POC.
/// </summary>
public record MappingEntry(string NodeId, int SourceIndex, int StartLine, int StartColumn, int EndLine, int EndColumn);

public class MappingTable
{
    private readonly List<MappingEntry> _entries = new();

    public void Add(MappingEntry entry) => _entries.Add(entry);

    public IReadOnlyList<MappingEntry> Entries => _entries.AsReadOnly();

    public MappingEntry? FindByNodeId(string nodeId) => _entries.Find(e => e.NodeId == nodeId);
}

