namespace PathfinderDb.Data.Domain;

public sealed record Spell(
    string Id,
    string Name,
    string? School,
    IReadOnlyList<SpellLevel> Levels,
    IReadOnlyList<string> ComponentKinds,
    string? Range,
    string? Target,
    string? CastingTime,
    Source? Source,
    IReadOnlyDictionary<string, string> Localization)
{
    public IReadOnlyList<Reference> References { get; init; } = [];
}
