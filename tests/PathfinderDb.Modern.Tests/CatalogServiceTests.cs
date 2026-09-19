using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Runtime;
using System.Globalization;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Feat_catalog_returns_at_most_fifty_items_and_pages()
    {
        var feats = Enumerable.Range(1, 51)
            .Select(index => new Feat($"feat-{index}", $"Alpha {index}", [], [], null, null, null, null))
            .ToArray();
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            feats, [], [], [], "version")));

        var firstPage = service.GetFeats("a");
        var secondPage = service.GetFeats("A", 2);

        Assert.NotNull(firstPage);
        Assert.Equal(50, firstPage!.Items.Count);
        Assert.True(firstPage.HasNextPage);
        Assert.NotNull(secondPage);
        Assert.Single(secondPage!.Items);
    }

    [Fact]
    public void Unknown_slug_and_page_return_null()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [new Feat("known", "Known", [], [], null, null, null, null)], [], [], [], "version")));

        Assert.Null(service.GetFeat("missing"));
        Assert.Null(service.GetFeats("A", 2));
    }

    [Fact]
    public void Monster_catalog_supports_type_and_source_buckets()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [],
            [],
            [
                new Monster("wolf", "Wolf", 1, null, null, "Animal", new Source("Bestiary", [])),
                new Monster("goblin", "Goblin", 1, null, null, "Humanoid", new Source("Bestiary", [])),
                new Monster("dragon", "Dragon", 10, null, null, "Dragon", new Source("Advanced", []))
            ],
            [],
            "version")));

        var byType = service.GetMonstersByType("animal");
        var bySource = service.GetMonstersBySource("Bestiary");

        Assert.Single(byType!.Items);
        Assert.Equal("Animal", byType.Bucket);
        Assert.Equal(2, bySource!.TotalCount);
        Assert.Equal(["Advanced", "Bestiary"], service.MonsterSourceBuckets);
    }

    [Fact]
    public void Monster_catalog_supports_alphabetical_buckets()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [],
            [],
            [
                new Monster("wolf", "Wolf", 1, null, null, "Animal", null),
                new Monster("ape", "Ape", 1, null, null, "Animal", null)
            ],
            [],
            "version")));

        var page = service.GetMonstersByInitial("A");

        Assert.NotNull(page);
        Assert.Equal("A", page!.Bucket);
        Assert.Equal(["Ape"], page.Items.Select(monster => monster.Name));
        Assert.Equal(["A", "W"], service.MonsterInitialBuckets);
    }

    [Fact]
    public void Monster_challenge_rating_bucket_uses_invariant_route_format()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [],
            [],
            [new Monster("fractional", "Fractional", 1.5m, null, null, null, null)],
            [],
            "version")));
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var page = service.GetMonsters("1.5");

            Assert.NotNull(page);
            Assert.Equal("1.5", page!.Bucket);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void Monster_integer_challenge_rating_bucket_omits_decimal_suffix()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [],
            [],
            [new Monster("integer", "Integer", 14.0m, null, null, null, null)],
            [],
            "version")));

        var page = service.GetMonsters("14");

        Assert.NotNull(page);
        Assert.Equal("14", page!.Bucket);
    }

    [Fact]
    public void Spell_and_feat_dimension_queries_are_case_insensitive_and_paginated()
    {
        var service = new CatalogService(new TestProvider(new DataSnapshot(
            [new Feat("combat-feat", "Combat Feat", ["Combat"], [], null, null, null, new Source("core", []))],
            [new Spell("spell", "Spell", "Evocation", [new SpellLevel("psychiste", 1)], [], null, null, null, new Source("core", []), new Dictionary<string, string>())],
            [],
            [new Source("core", [])],
            "version")));

        Assert.Equal("Evocation", service.GetSpellsBySchool("evocation")!.Bucket);
        Assert.Equal("psychiste", service.GetSpellsByList("PSYCHISTE")!.Bucket);
        Assert.Equal("core", service.GetSpellsBySource("CORE")!.Bucket);
        Assert.Equal("Combat", service.GetFeatsByType("combat")!.Bucket);
        Assert.Equal("core", service.GetFeatsBySource("CORE")!.Bucket);
        Assert.Null(service.GetSpellsBySchool("missing"));
    }

    private sealed class TestProvider(DataSnapshot snapshot) : IDataSnapshotProvider
    {
        public DataSnapshot? Current => snapshot;
        public DataLoadStatus Status => DataLoadStatus.ReadyStatus(snapshot, []);
        public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) => Task.FromResult(Status);
        public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Status);
    }
}
