using System.Globalization;
using PathfinderDb.Data.Runtime;

namespace PathfinderDb.Data.Domain;

public sealed class CatalogService(IDataSnapshotProvider provider)
{
    public const int PageSize = 50;
    public string? SnapshotVersion => provider.Current?.Version;

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

    public CatalogPage<Monster>? GetMonstersByType(string? type, int page = 1) =>
        GetPage(provider.Current?.MonstersByType, NormalizeBucket(type), page);

    public CatalogPage<Monster>? GetMonstersBySource(string? source, int page = 1) =>
        GetPage(provider.Current?.MonstersBySource, NormalizeBucket(source), page);

    public CatalogPage<Spell>? GetSpellsBySchool(string? school, int page = 1) =>
        GetPage(provider.Current?.SpellsBySchool, NormalizeBucket(school), page);

    public CatalogPage<Spell>? GetSpellsByList(string? list, int page = 1) =>
        GetPage(provider.Current?.SpellsByList, NormalizeBucket(list), page);

    public CatalogPage<Spell>? GetSpellsBySource(string? source, int page = 1) =>
        GetPage(provider.Current?.SpellsBySource, NormalizeBucket(source), page);

    public CatalogPage<Feat>? GetFeatsByType(string? type, int page = 1) =>
        GetPage(provider.Current?.FeatsByType, NormalizeBucket(type), page);

    public CatalogPage<Feat>? GetFeatsBySource(string? source, int page = 1) =>
        GetPage(provider.Current?.FeatsBySource, NormalizeBucket(source), page);

    public Feat? GetFeat(string slug) => provider.Current?.FeatsById.GetValueOrDefault(slug);
    public Spell? GetSpell(string slug) => provider.Current?.SpellsById.GetValueOrDefault(slug);
    public Monster? GetMonster(string slug) => provider.Current?.MonstersById.GetValueOrDefault(slug);

    public IReadOnlyList<string> FeatBuckets =>
        provider.Current?.FeatsByInitial.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> SpellBuckets =>
        provider.Current?.SpellsByInitial.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<decimal> MonsterBuckets =>
        provider.Current?.MonstersByChallengeRating.Keys.OrderBy(x => x).ToArray() ?? [];

    public IReadOnlyList<string> MonsterTypeBuckets =>
        provider.Current?.MonstersByType.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> MonsterSourceBuckets =>
        provider.Current?.MonstersBySource.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> SpellSchoolBuckets =>
        provider.Current?.SpellsBySchool.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> SpellListBuckets =>
        provider.Current?.SpellsByList.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> SpellSourceBuckets =>
        provider.Current?.SpellsBySource.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> FeatTypeBuckets =>
        provider.Current?.FeatsByType.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    public IReadOnlyList<string> FeatSourceBuckets =>
        provider.Current?.FeatsBySource.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    private static CatalogPage<T>? GetPage<T, TKey>(
        IReadOnlyDictionary<TKey, IReadOnlyList<T>>? buckets,
        TKey bucket,
        int page)
        where TKey : notnull
    {
        if (buckets is null || page < 1)
            return null;

        if (bucket is string requestedBucket)
        {
            var canonicalBucket = buckets.Keys
                .FirstOrDefault(key => key is string value &&
                    string.Equals(value, requestedBucket, StringComparison.OrdinalIgnoreCase));
            if (canonicalBucket is null)
                return null;

            bucket = canonicalBucket;
        }

        if (!buckets.TryGetValue(bucket, out var items))
            return null;

        var pageCount = Math.Max(1, (items.Count + PageSize - 1) / PageSize);
        if (page > pageCount)
            return null;

        return new CatalogPage<T>(
            bucket switch
            {
                decimal decimalBucket => decimalBucket.ToString("G29", CultureInfo.InvariantCulture),
                _ => bucket.ToString()!
            },
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

    private static string NormalizeBucket(string? bucket) => bucket?.Trim() ?? string.Empty;
}
