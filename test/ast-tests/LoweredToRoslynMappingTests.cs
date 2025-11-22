using System;
using compiler;
using FluentAssertions;
using Xunit;

namespace ast_tests;

public class LoweredToRoslynMappingTests
{
    [Fact]
    public void MappingTable_AddAndRetrieveEntries()
    {
        var table = new MappingTable();
        var entry = new MappingEntry("node1", 0, 1, 1, 1, 10);
        table.Add(entry);

        table.Entries.Count.Should().Be(1);
        table.Entries[0].NodeId.Should().Be("node1");
    }

    [Fact]
    public void MappingTable_FindByNodeId_Returns_CorrectEntry()
    {
        var table = new MappingTable();
        var entry = new MappingEntry("node-x", 0, 2, 1, 2, 10);
        table.Add(entry);

        var found = table.FindByNodeId("node-x");
        found.Should().NotBeNull();
        found!.NodeId.Should().Be("node-x");
    }
}
