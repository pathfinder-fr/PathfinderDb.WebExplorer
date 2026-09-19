using Microsoft.Extensions.Options;
using PathfinderDb.Data.Import;
using PathfinderDb.Data.Runtime;

namespace PathfinderDb.Web;

public sealed class DataRefreshService(
    IGitRepository git,
    IDataSnapshotProvider provider,
    IOptions<PathfinderDataOptions> options,
    ILogger<DataRefreshService> logger) : BackgroundService
{
    private readonly PathfinderDataOptions _options = options.Value;

    public async Task<bool> RefreshOnceAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.RefreshEnabled || string.IsNullOrWhiteSpace(_options.RootPath))
            return false;

        var pull = await git.PullAsync(_options.RootPath, cancellationToken);
        if (!pull.Succeeded)
        {
            logger.LogError("Pathfinder data refresh Git pull failed: {Error}", pull.Error);
            return false;
        }

        logger.LogInformation("Pathfinder data Git pull completed: {Output}", pull.Output);
        await provider.ReloadAsync(cancellationToken);
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.RefreshEnabled)
            return;

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _options.RefreshIntervalMinutes)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RefreshOnceAsync(stoppingToken);
    }
}
