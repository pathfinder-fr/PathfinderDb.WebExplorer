using PathfinderDb.Data.Runtime;

namespace PathfinderDb.Web;

public sealed class DataInitializationService(
    IDataSnapshotProvider provider,
    ILogger<DataInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var status = await provider.LoadInitialAsync(cancellationToken);
        if (status.State == DataLoadState.Unavailable)
            logger.LogError("Pathfinder data startup is degraded: {Errors}", string.Join(" | ", status.Errors));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
