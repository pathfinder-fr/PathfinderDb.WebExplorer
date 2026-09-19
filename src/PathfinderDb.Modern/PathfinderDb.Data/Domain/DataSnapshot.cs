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
        FeatsByInitial = BuildInitialIndex(Feats);
        SpellsByInitial = BuildInitialIndex(Spells);
        MonstersByChallengeRating = BuildChallengeRatingIndex(Monsters);
        MonstersByType = BuildStringIndex(Monsters, monster => [monster.Type]);
        MonstersBySource = BuildStringIndex(Monsters, monster => [monster.Source?.Id]);
        SpellsBySchool = BuildStringIndex(Spells, spell => [spell.School]);
        SpellsByList = BuildStringIndex(Spells, spell => spell.Levels.Select(level => level.List));
        SpellsBySource = BuildStringIndex(Spells, spell => [spell.Source?.Id]);
        FeatsByType = BuildStringIndex(Feats, feat => feat.Types);
        FeatsBySource = BuildStringIndex(Feats, feat => [feat.Source?.Id]);
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
    public IReadOnlyDictionary<string, IReadOnlyList<Feat>> FeatsByInitial { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Spell>> SpellsByInitial { get; }
    public IReadOnlyDictionary<decimal, IReadOnlyList<Monster>> MonstersByChallengeRating { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Monster>> MonstersByType { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Monster>> MonstersBySource { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Spell>> SpellsBySchool { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Spell>> SpellsByList { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Spell>> SpellsBySource { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Feat>> FeatsByType { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<Feat>> FeatsBySource { get; }

    private static IReadOnlyDictionary<string, T> ReadOnlyDictionary<T>(Dictionary<string, T> value) =>
        new ReadOnlyDictionary<string, T>(value);

    private static IReadOnlyDictionary<string, IReadOnlyList<T>> BuildInitialIndex<T>(
        IEnumerable<T> items)
        where T : notnull
    {
        var groups = items.GroupBy(
                item => InitialBucket(item switch
                {
                    Feat feat => feat.Name,
                    Spell spell => spell.Name,
                    _ => throw new ArgumentException($"Unsupported index type: {typeof(T).Name}.")
                }),
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<T>)group.ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return ReadOnlyDictionary(groups);
    }

    private static IReadOnlyDictionary<decimal, IReadOnlyList<Monster>> BuildChallengeRatingIndex(
        IEnumerable<Monster> monsters) =>
        new ReadOnlyDictionary<decimal, IReadOnlyList<Monster>>(monsters
            .Where(monster => monster.ChallengeRating.HasValue)
            .GroupBy(monster => monster.ChallengeRating!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Monster>)group.ToArray()));

    private static IReadOnlyDictionary<string, IReadOnlyList<T>> BuildStringIndex<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<string?>> selector) =>
        new ReadOnlyDictionary<string, IReadOnlyList<T>>(items
            .SelectMany(item => selector(item)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => (Item: item, Key: value!.Trim())))
            .GroupBy(value => value.Key!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<T>)group.Select(value => value.Item).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase));

    private static string InitialBucket(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "0-9";

        var first = char.ToUpperInvariant(name[0]);
        return first is >= 'A' and <= 'Z' ? first.ToString() : "0-9";
    }
}
