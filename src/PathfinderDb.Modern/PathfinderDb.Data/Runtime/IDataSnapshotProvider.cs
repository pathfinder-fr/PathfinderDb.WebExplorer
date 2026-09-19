using PathfinderDb.Data.Domain;

namespace PathfinderDb.Data.Runtime;

public interface IDataSnapshotProvider
{
    DataSnapshot? Current { get; }
    DataLoadStatus Status { get; }
    Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default);
    Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default);
}
