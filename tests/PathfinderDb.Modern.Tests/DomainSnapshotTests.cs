using PathfinderDb.Data.Domain;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class DomainSnapshotTests
{
    [Fact]
    public void Snapshot_builds_deterministic_indexes_and_dynamic_lists()
    {
        var snapshot = new DataSnapshot(
            [new Feat("zeta", "Zeta", [], [], null, null, null, null), new Feat("alpha", "Alpha", [], [], null, null, null, null)],
            [new Spell("spell", "Spell", null, [new SpellLevel("psychiste", 1)], [], null, null, null, null, new Dictionary<string, string>())],
            [new Monster("monster", "Monster", 1, null, null, null, null)],
            [new Source("new-source", [])],
            "version");

        Assert.Equal("Alpha", snapshot.Feats[0].Name);
        Assert.True(snapshot.FeatsById.ContainsKey("zeta"));
        Assert.Contains("psychiste", snapshot.SpellLists);
    }
}
