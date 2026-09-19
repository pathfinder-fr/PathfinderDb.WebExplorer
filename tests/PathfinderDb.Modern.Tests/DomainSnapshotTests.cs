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
        Assert.Equal(["Alpha"], snapshot.FeatsByInitial["A"].Select(x => x.Name));
        Assert.Equal(["Monster"], snapshot.MonstersByChallengeRating[1].Select(x => x.Name));
    }

    [Fact]
    public void Snapshot_indexes_spells_and_feats_by_content_dimensions()
    {
        var snapshot = new DataSnapshot(
            [
                new Feat("combat-feat", "Combat Feat", ["Combat"], [], null, null, null, new Source("core", [])),
                new Feat("general-feat", "General Feat", ["General"], [], null, null, null, new Source("core", []))
            ],
            [
                new Spell("evocation", "Evocation Spell", "Evocation",
                    [new SpellLevel("wizard", 1), new SpellLevel("psychiste", 2)],
                    [], null, null, null, new Source("core", []), new Dictionary<string, string>()),
                new Spell("illusion", "Illusion Spell", "Illusion",
                    [new SpellLevel("wizard", 2)],
                    [], null, null, null, new Source("advanced", []), new Dictionary<string, string>())
            ],
            [],
            [new Source("core", []), new Source("advanced", [])],
            "version");

        Assert.Equal(["evocation"], snapshot.SpellsBySchool["Evocation"].Select(x => x.Id));
        Assert.Equal(["evocation"], snapshot.SpellsByList["psychiste"].Select(x => x.Id));
        Assert.Equal(["illusion"], snapshot.SpellsBySource["advanced"].Select(x => x.Id));
        Assert.Equal(["combat-feat"], snapshot.FeatsByType["Combat"].Select(x => x.Id));
        Assert.Equal(["combat-feat", "general-feat"], snapshot.FeatsBySource["core"].Select(x => x.Id));
    }
}
