using Microsoft.Extensions.Options;
using PathfinderDb.Data.Import;
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
        var result = await loader.LoadAsync();

        Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Validation.Errors));
        Assert.NotNull(result.Snapshot);
        Assert.NotEmpty(result.Snapshot!.Feats);
        Assert.NotEmpty(result.Snapshot.Spells);
        Assert.NotEmpty(result.Snapshot.Monsters);
        Assert.NotEmpty(result.Snapshot.Sources);
        Assert.False(string.IsNullOrWhiteSpace(result.Version));
        Assert.True(result.Snapshot.Feats.Count > 100);
        Assert.True(result.Snapshot.Spells.Count > 100);
    }
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
