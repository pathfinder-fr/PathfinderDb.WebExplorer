using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Runtime;
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

    private sealed class TestProvider(DataSnapshot snapshot) : IDataSnapshotProvider
    {
        public DataSnapshot? Current => snapshot;
        public DataLoadStatus Status => DataLoadStatus.ReadyStatus(snapshot, []);
        public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) => Task.FromResult(Status);
        public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Status);
    }
}
