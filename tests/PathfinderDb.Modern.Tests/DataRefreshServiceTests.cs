using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Import;
using PathfinderDb.Data.Runtime;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class DataRefreshServiceTests
{
    [Fact]
    public async Task Reloads_snapshot_after_successful_git_pull()
    {
        var git = new FakeGitRepository(GitPullResult.Success("updated"));
        var provider = new FakeSnapshotProvider();
        var service = new DataRefreshService(
            git,
            provider,
            Options.Create(new PathfinderDataOptions
            {
                RootPath = "D:\\data",
                RefreshEnabled = true
            }),
            NullLogger<DataRefreshService>.Instance);

        var result = await service.RefreshOnceAsync();

        Assert.True(result);
        Assert.Equal(1, git.PullCount);
        Assert.Equal(1, provider.ReloadCount);
    }

    [Fact]
    public async Task Keeps_existing_snapshot_when_git_pull_fails()
    {
        var git = new FakeGitRepository(GitPullResult.Failure("network unavailable"));
        var provider = new FakeSnapshotProvider();
        var service = new DataRefreshService(
            git,
            provider,
            Options.Create(new PathfinderDataOptions
            {
                RootPath = "D:\\data",
                RefreshEnabled = true
            }),
            NullLogger<DataRefreshService>.Instance);

        var result = await service.RefreshOnceAsync();

        Assert.False(result);
        Assert.Equal(1, git.PullCount);
        Assert.Equal(0, provider.ReloadCount);
    }

    [Fact]
    public async Task Reports_git_start_failure_without_throwing()
    {
        var result = await new GitRepository().PullAsync("D:\\path-that-does-not-exist");

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Error);
    }

    private sealed class FakeGitRepository(GitPullResult result) : IGitRepository
    {
        public int PullCount { get; private set; }

        public Task<GitPullResult> PullAsync(string rootPath, CancellationToken cancellationToken = default)
        {
            PullCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeSnapshotProvider : IDataSnapshotProvider
    {
        public int ReloadCount { get; private set; }
        public DataSnapshot? Current => null;
        public DataLoadStatus Status => DataLoadStatus.LoadingStatus();
        public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);

        public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default)
        {
            ReloadCount++;
            return Task.FromResult(Status);
        }
    }
}
