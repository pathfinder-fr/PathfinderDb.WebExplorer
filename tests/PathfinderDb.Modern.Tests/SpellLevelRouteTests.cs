using Microsoft.AspNetCore.Mvc;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Runtime;
using PathfinderDb.Web.Pages.Spells;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class SpellLevelRouteTests
{
    [Theory]
    [InlineData("not-a-level")]
    [InlineData("-1")]
    public void Invalid_level_route_values_return_not_found(string level)
    {
        var snapshot = new DataSnapshot([], [], [], [], "version");
        var model = new ByLevelModel(new CatalogService(new SnapshotProvider(snapshot)));

        var result = model.OnGet(level);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class SnapshotProvider(DataSnapshot snapshot) : IDataSnapshotProvider
    {
        public DataSnapshot? Current => snapshot;
        public DataLoadStatus Status => DataLoadStatus.ReadyStatus(snapshot, []);
        public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);
        public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);
    }
}
