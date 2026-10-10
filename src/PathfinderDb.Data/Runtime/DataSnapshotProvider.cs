using Microsoft.Extensions.Logging;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Import;

namespace PathfinderDb.Data.Runtime;

public sealed class DataSnapshotProvider(
    PathfinderDataLoader loader,
    ILogger<DataSnapshotProvider> logger) : IDataSnapshotProvider
{
    private DataSnapshot? _current;
    private DataLoadStatus _status = DataLoadStatus.LoadingStatus();

    public DataSnapshot? Current => Volatile.Read(ref _current);
    public DataLoadStatus Status => Volatile.Read(ref _status);

    public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) => ReloadAsync(cancellationToken);

    public async Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default)
    {
        Volatile.Write(ref _status, DataLoadStatus.LoadingStatus());
        var result = await loader.LoadAsync(cancellationToken);
        if (result.IsSuccess && result.Snapshot is not null)
        {
            Interlocked.Exchange(ref _current, result.Snapshot);
            var ready = DataLoadStatus.ReadyStatus(result.Snapshot, result.Validation.Warnings);
            Volatile.Write(ref _status, ready);
            return ready;
        }

        var unavailable = DataLoadStatus.UnavailableStatus(result.Validation.Errors, Volatile.Read(ref _current));
        Volatile.Write(ref _status, unavailable);
        logger.LogError("Pathfinder data snapshot unavailable: {Errors}", string.Join(" | ", unavailable.Errors));
        return unavailable;
    }
}
