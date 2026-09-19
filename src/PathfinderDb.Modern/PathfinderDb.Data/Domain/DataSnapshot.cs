using System.Collections.ObjectModel;

namespace PathfinderDb.Data.Domain;

public sealed class DataSnapshot
{
    public DataSnapshot(
        IEnumerable<Feat> feats,
        IEnumerable<Spell> spells,
        IEnumerable<Monster> monsters,
        IEnumerable<Source> sources,
        string version)
    {
        Version = string.IsNullOrWhiteSpace(version) ? throw new ArgumentException("A snapshot version is required.", nameof(version)) : version;
        Feats = feats.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        Spells = spells.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        Monsters = monsters.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        Sources = sources.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToArray();
        FeatsById = ReadOnlyDictionary(Feats.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase));
        SpellsById = ReadOnlyDictionary(Spells.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase));
        MonstersById = ReadOnlyDictionary(Monsters.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase));
        SourcesById = ReadOnlyDictionary(Sources.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase));
        SpellLists = Spells.SelectMany(x => x.Levels).Select(x => x.List).Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public string Version { get; }
    public IReadOnlyList<Feat> Feats { get; }
    public IReadOnlyList<Spell> Spells { get; }
    public IReadOnlyList<Monster> Monsters { get; }
    public IReadOnlyList<Source> Sources { get; }
    public IReadOnlyList<string> SpellLists { get; }
    public IReadOnlyDictionary<string, Feat> FeatsById { get; }
    public IReadOnlyDictionary<string, Spell> SpellsById { get; }
    public IReadOnlyDictionary<string, Monster> MonstersById { get; }
    public IReadOnlyDictionary<string, Source> SourcesById { get; }

    private static IReadOnlyDictionary<string, T> ReadOnlyDictionary<T>(Dictionary<string, T> value) =>
        new ReadOnlyDictionary<string, T>(value);
}
