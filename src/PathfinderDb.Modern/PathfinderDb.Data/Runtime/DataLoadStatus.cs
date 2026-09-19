using PathfinderDb.Data.Domain;

namespace PathfinderDb.Data.Runtime;

public enum DataLoadState
{
    Loading,
    Ready,
    Unavailable
}

public sealed record DataLoadStatus(
    DataLoadState State,
    string? Version,
    int FeatCount,
    int SpellCount,
    int MonsterCount,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors)
{
    public static DataLoadStatus LoadingStatus() => new(DataLoadState.Loading, null, 0, 0, 0, [], []);
    public static DataLoadStatus ReadyStatus(DataSnapshot snapshot, IReadOnlyList<string> warnings) =>
        new(DataLoadState.Ready, snapshot.Version, snapshot.Feats.Count, snapshot.Spells.Count, snapshot.Monsters.Count, warnings, []);
    public static DataLoadStatus UnavailableStatus(IEnumerable<string> errors, DataSnapshot? previous) =>
        new(DataLoadState.Unavailable, previous?.Version, previous?.Feats.Count ?? 0, previous?.Spells.Count ?? 0,
            previous?.Monsters.Count ?? 0, [], errors.ToArray());
}
