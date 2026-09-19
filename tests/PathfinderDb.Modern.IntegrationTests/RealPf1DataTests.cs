using Microsoft.Extensions.Options;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Import;
using PathfinderDb.Data.Runtime;
using System.Diagnostics;
using Xunit;

namespace PathfinderDb.Modern.IntegrationTests;

public sealed class RealPf1DataTests
{
    [Fact]
    public async Task Real_clone_loads_all_three_catalogs()
    {
        var root = Environment.GetEnvironmentVariable("PATHFINDER_DATA_ROOT")
            ?? Environment.GetEnvironmentVariable("PathfinderData__RootPath");
        if (Skip.If(string.IsNullOrWhiteSpace(root), "Set PATHFINDER_DATA_ROOT or PathfinderData__RootPath to run against pf1-data.")) return;
        if (Skip.IfNot(Directory.Exists(root), $"The configured pf1-data clone does not exist: {root}")) return;

        var loader = new PathfinderDataLoader(Options.Create(new PathfinderDataOptions { RootPath = root }));
        var stopwatch = Stopwatch.StartNew();
        var result = await loader.LoadAsync();
        stopwatch.Stop();
        Console.WriteLine($"pf1-data load and indexing: {stopwatch.Elapsed.TotalMilliseconds:0} ms");

        Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Validation.Errors));
        Assert.NotNull(result.Snapshot);
        Assert.NotEmpty(result.Snapshot!.Feats);
        Assert.NotEmpty(result.Snapshot.Spells);
        Assert.NotEmpty(result.Snapshot.Monsters);
        Assert.NotEmpty(result.Snapshot.Sources);
        Assert.Equal("Invocation", result.Snapshot.Labels.Get("spellSchool", "conjuration"));
        Assert.False(string.IsNullOrWhiteSpace(result.Version));
        Assert.True(result.Snapshot.Feats.Count > 100);
        Assert.True(result.Snapshot.Spells.Count > 100);
        Assert.NotEmpty(result.Snapshot.SpellsBySchool);
        Assert.NotEmpty(result.Snapshot.SpellsByList);
        Assert.NotEmpty(result.Snapshot.SpellsBySource);
        Assert.NotEmpty(result.Snapshot.FeatsByType);
        Assert.NotEmpty(result.Snapshot.FeatsBySource);
        Assert.Contains(result.Snapshot.Feats, feat => feat.References.Count > 0);
        Assert.Contains(result.Snapshot.Spells, spell => spell.References.Count > 0);
        Assert.All(result.Snapshot.Monsters, monster => Assert.Empty(monster.References));

        var catalogs = new CatalogService(new SnapshotProvider(result.Snapshot));
        var school = result.Snapshot.SpellsBySchool.Keys.First();
        var spellClass = result.Snapshot.SpellsByList.Keys.First();
        var spellSource = result.Snapshot.SpellsBySource.Keys.First();
        var featType = result.Snapshot.FeatsByType.Keys.First();
        var featSource = result.Snapshot.FeatsBySource.Keys.First();
        Assert.NotNull(catalogs.GetSpellsBySchool(school));
        Assert.NotNull(catalogs.GetSpellsByList(spellClass));
        Assert.NotNull(catalogs.GetSpellsBySource(spellSource));
        Assert.NotNull(catalogs.GetFeatsByType(featType));
        Assert.NotNull(catalogs.GetFeatsBySource(featSource));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Loading and indexing pf1-data took {stopwatch.Elapsed.TotalMilliseconds:0} ms.");
    }
}

internal sealed class SnapshotProvider(DataSnapshot snapshot) : IDataSnapshotProvider
{
    public DataSnapshot? Current => snapshot;
    public DataLoadStatus Status => DataLoadStatus.ReadyStatus(snapshot, []);
    public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Status);
    public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Status);
}

internal static class Skip
{
    public static bool If(bool condition, string reason)
    {
        if (condition)
            Console.WriteLine($"SKIPPED: {reason}");
        return condition;
    }

    public static bool IfNot(bool condition, string reason) => If(!condition, reason);
}
