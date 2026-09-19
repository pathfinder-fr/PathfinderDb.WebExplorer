using System.Globalization;
using PathfinderDb.Data.Runtime;

namespace PathfinderDb.Data.Domain;

public sealed class CatalogService(IDataSnapshotProvider provider)
{
    public const int PageSize = 50;

    public CatalogPage<Feat>? GetFeats(string? initial, int page = 1) =>
        GetPage(provider.Current?.FeatsByInitial, NormalizeInitial(initial), page);

    public CatalogPage<Spell>? GetSpells(string? initial, int page = 1) =>
        GetPage(provider.Current?.SpellsByInitial, NormalizeInitial(initial), page);

    public CatalogPage<Monster>? GetMonsters(string? challengeRating, int page = 1)
    {
        if (!decimal.TryParse(challengeRating, NumberStyles.Number, CultureInfo.InvariantCulture, out var cr))
            return null;

        return GetPage(provider.Current?.MonstersByChallengeRating, cr, page);
    }

    public Feat? GetFeat(string slug) => provider.Current?.FeatsById.GetValueOrDefault(slug);
    public Spell? GetSpell(string slug) => provider.Current?.SpellsById.GetValueOrDefault(slug);
    public Monster? GetMonster(string slug) => provider.Current?.MonstersById.GetValueOrDefault(slug);

    public IReadOnlyList<string> FeatBuckets =>
        provider.Current?.FeatsByInitial.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> SpellBuckets =>
        provider.Current?.SpellsByInitial.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<decimal> MonsterBuckets =>
        provider.Current?.MonstersByChallengeRating.Keys.OrderBy(x => x).ToArray() ?? [];

    private static CatalogPage<T>? GetPage<T, TKey>(
        IReadOnlyDictionary<TKey, IReadOnlyList<T>>? buckets,
        TKey bucket,
        int page)
        where TKey : notnull
    {
        if (buckets is null || page < 1 || !buckets.TryGetValue(bucket, out var items))
            return null;

        var pageCount = Math.Max(1, (items.Count + PageSize - 1) / PageSize);
        if (page > pageCount)
            return null;

        return new CatalogPage<T>(
            bucket.ToString()!,
            page,
            pageCount,
            items.Count,
            items.Skip((page - 1) * PageSize).Take(PageSize).ToArray());
    }

    private static string NormalizeInitial(string? initial) =>
        string.IsNullOrWhiteSpace(initial)
            ? "A"
            : initial.Trim().Equals("0-9", StringComparison.OrdinalIgnoreCase)
                ? "0-9"
                : initial.Trim().ToUpperInvariant();
}
