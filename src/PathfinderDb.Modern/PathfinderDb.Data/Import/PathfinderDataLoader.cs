using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Json;

namespace PathfinderDb.Data.Import;

public sealed class PathfinderDataLoader(IOptions<PathfinderDataOptions> options)
{
    private static readonly string[] RequiredFiles = ["feats.json", "spells.json", "monsters.json"];

    public Task<DataLoadResult> LoadAsync(CancellationToken cancellationToken = default) =>
        LoadAsync(options.Value, cancellationToken);

    public async Task<DataLoadResult> LoadAsync(PathfinderDataOptions settings, CancellationToken cancellationToken = default)
    {
        var validation = new DataValidationResult();
        if (string.IsNullOrWhiteSpace(settings.RootPath))
        {
            validation.Errors.Add("PathfinderData:RootPath is required.");
            return new(null, validation, null);
        }

        var root = Path.GetFullPath(settings.RootPath);
        if (File.Exists(root))
        {
            validation.Errors.Add($"Configured data root is a file, not a directory: {root}");
            return new(null, validation, null);
        }
        if (!Directory.Exists(root))
        {
            validation.Errors.Add($"Configured data root directory does not exist: {root}");
            return new(null, validation, null);
        }

        var paths = RequiredFiles.ToDictionary(x => x, x => Path.Combine(root, x), StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths.Values.Where(path => !File.Exists(path)))
            validation.Errors.Add($"Required data file is missing: {path}");
        if (!validation.IsValid)
            return new(null, validation, null);

        try
        {
            var feats = await DeserializeAsync<FeatDocument>(paths["feats.json"], PathfinderJsonContext.Default.FeatDocument, cancellationToken);
            var spells = await DeserializeAsync<SpellDocument>(paths["spells.json"], PathfinderJsonContext.Default.SpellDocument, cancellationToken);
            var monsters = await DeserializeAsync<MonsterDocument>(paths["monsters.json"], PathfinderJsonContext.Default.MonsterDocument, cancellationToken);
            if (feats.Feats is null) validation.Errors.Add("feats.json must contain a Feats array.");
            if (spells.Spells is null) validation.Errors.Add("spells.json must contain a Spells array.");
            if (monsters.Monsters is null) validation.Errors.Add("monsters.json must contain a Monsters array.");
            if (!validation.IsValid) return new(null, validation, null);
            var sources = MergeSources(feats.Sources, spells.Sources, monsters.Sources, validation);
            var featModels = MapFeats(feats.Feats, sources, validation);
            var spellModels = MapSpells(spells.Spells, sources, validation);
            var monsterModels = MapMonsters(monsters.Monsters, sources, validation);

            featModels = Deduplicate(featModels, x => x.Id, "feat", validation);
            spellModels = Deduplicate(spellModels, x => x.Id, "spell", validation);
            monsterModels = Deduplicate(monsterModels, x => x.Id, "monster", validation);
            if (!validation.IsValid)
                return new(null, validation, null);

            var version = await ComputeVersionAsync(paths.Values, cancellationToken);
            return new(new DataSnapshot(featModels, spellModels, monsterModels, sources.Values, version), validation, version);
        }
        catch (JsonException ex)
        {
            validation.Errors.Add($"Invalid JSON in {ex.Path ?? "data"}: {ex.Message}");
            return new(null, validation, null);
        }
        catch (IOException ex)
        {
            validation.Errors.Add($"Unable to read Pathfinder data: {ex.Message}");
            return new(null, validation, null);
        }
    }

    private static async Task<T> DeserializeAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken)
            ?? throw new JsonException($"Root object is null in {path}.");
    }

    private static Dictionary<string, Source> MergeSources(
        IEnumerable<SourceJson>? first, IEnumerable<SourceJson>? second, IEnumerable<SourceJson>? third,
        DataValidationResult validation)
    {
        var result = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in (first ?? []).Concat(second ?? []).Concat(third ?? []))
        {
            if (string.IsNullOrWhiteSpace(source.Id))
            {
                validation.Errors.Add("A source is missing its required Id.");
                continue;
            }
            var mapped = new Source(source.Id.Trim(), (source.References ?? []).Select(MapReference).ToArray());
            if (result.TryGetValue(mapped.Id, out var existing))
            {
                result[mapped.Id] = existing with { References = existing.References.Concat(mapped.References).Distinct().ToArray() };
            }
            else result.Add(mapped.Id, mapped);
        }
        return result;
    }

    private static IReadOnlyList<Feat> MapFeats(IEnumerable<FeatJson>? values, IReadOnlyDictionary<string, Source> sources, DataValidationResult validation) =>
        (values ?? []).Select((x, i) =>
        {
            var id = Required(x.Id, $"feats[{i}].Id", validation);
            var name = Required(x.Name, $"feats[{i}].Name", validation);
            return new Feat(id, name, (x.Types ?? []).Where(y => !string.IsNullOrWhiteSpace(y)).ToArray(),
                (x.Prerequisites ?? []).Select(MapPrerequisite).ToArray(), x.Description, x.Benefit, x.Normal,
                MapSource(x.Source, sources, $"feats[{i}].Source", validation))
            {
                References = MapReferences(x.Source?.References)
            };
        }).ToArray();

    private static IReadOnlyList<Spell> MapSpells(IEnumerable<SpellJson>? values, IReadOnlyDictionary<string, Source> sources, DataValidationResult validation) =>
        (values ?? []).Select((x, i) =>
        {
            var id = Required(x.Id, $"spells[{i}].Id", validation);
            var name = Required(x.Name, $"spells[{i}].Name", validation);
            var levels = (x.Levels ?? []).Select((level, j) =>
            {
                var list = Required(level.List, $"spells[{i}].Levels[{j}].List", validation);
                if (level.Level is null or < 0) validation.Warnings.Add($"spells[{i}].Levels[{j}].Level is missing or negative.");
                return new SpellLevel(list, Math.Max(0, level.Level ?? 0));
            }).ToArray();
            return new Spell(id, name, x.School, levels, SplitKinds(x.Components?.Kinds),
                x.Range?.SpecificValue ?? JsonValue(x.Range?.Value), JsonValue(x.Target?.Value), JsonValue(x.CastingTime?.Value),
                MapSource(x.Source, sources, $"spells[{i}].Source", validation), MapLocalization(x.Localization))
            {
                References = MapReferences(x.Source?.References)
            };
        }).ToArray();

    private static IReadOnlyList<Monster> MapMonsters(IEnumerable<MonsterJson>? values, IReadOnlyDictionary<string, Source> sources, DataValidationResult validation) =>
        (values ?? []).Select((x, i) => new Monster(
            Required(x.Id, $"monsters[{i}].Id", validation),
            Required(x.Name, $"monsters[{i}].Name", validation), x.CR, x.Climate, x.Environment, x.Type,
            MapSource(x.Source, sources, $"monsters[{i}].Source", validation))).ToArray();

    private static Source? MapSource(SourceJson? value, IReadOnlyDictionary<string, Source> sources, string location, DataValidationResult validation)
    {
        if (value is null) return null;
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            validation.Errors.Add($"{location}.Id is required.");
            return null;
        }
        if (!sources.TryGetValue(value.Id, out var source))
        {
            validation.Errors.Add($"{location} references unknown source '{value.Id}'.");
            return null;
        }
        return source;
    }

    private static Prerequisite MapPrerequisite(PrerequisiteJson value) =>
        new(value.Type, value.OtherType, value.Value, value.Number, value.Description,
            (value.Items ?? []).Select(MapPrerequisite).ToArray());

    private static Reference MapReference(ReferenceJson value) => new(value.Name, value.Href, value.HrefString, value.Lang);

    private static IReadOnlyList<Reference> MapReferences(IEnumerable<ReferenceJson>? values) =>
        (values ?? []).Select(MapReference).ToArray();

    private static string Required(string? value, string location, DataValidationResult validation)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validation.Errors.Add($"{location} is required.");
            return string.Empty;
        }
        return value.Trim();
    }

    private static string[] SplitKinds(string? value) =>
        string.IsNullOrWhiteSpace(value) ? [] : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string? JsonValue(JsonElement? value) =>
        value is null ? null : value.Value.ValueKind == JsonValueKind.String ? value.Value.GetString() : value.Value.ToString();

    private static IReadOnlyDictionary<string, string> MapLocalization(LocalizationJson? localization) =>
        (localization?.Languages ?? []).SelectMany(language => (language.Entries ?? []).Select(entry =>
                new KeyValuePair<string, string>($"{language.Lang ?? string.Empty}:{entry.Href ?? string.Empty}", entry.Value ?? string.Empty)))
            .Where(x => x.Value.Length > 0)
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last().Value, StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<T> Deduplicate<T>(IReadOnlyList<T> items, Func<T, string> id, string category, DataValidationResult validation)
    {
        var result = new List<T>();
        foreach (var group in items.Where(x => id(x).Length > 0).GroupBy(id, StringComparer.OrdinalIgnoreCase))
        {
            var first = group.First();
            result.Add(first);
            foreach (var duplicate in group.Skip(1))
            {
                if (EqualityComparer<T>.Default.Equals(first, duplicate))
                    validation.Warnings.Add($"Duplicate identical {category} identifier '{group.Key}' was ignored.");
                else
                    validation.Warnings.Add($"Conflicting {category} identifier '{group.Key}' was resolved using the first entry.");
            }
        }
        return result;
    }

    private static async Task<string> ComputeVersionAsync(IEnumerable<string> paths, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var path in paths.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetFileName(path)));
            hash.AppendData(bytes);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
