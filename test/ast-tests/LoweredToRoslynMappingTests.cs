using System;
using compiler;
using FluentAssertions;
using TUnit;

namespace ast_tests;

public class LoweredToRoslynMappingTests
{
    [Test]
    public void MappingTable_AddAndRetrieveEntries()
    {
        var table = new MappingTable();
        var entry = new MappingEntry("node1", 0, 1, 1, 1, 10);
        table.Add(entry);

        table.Entries.Count.Should().Be(1);
        table.Entries[0].NodeId.Should().Be("node1");
    }
}
