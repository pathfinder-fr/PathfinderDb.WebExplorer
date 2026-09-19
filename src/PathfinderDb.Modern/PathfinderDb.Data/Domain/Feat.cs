namespace PathfinderDb.Data.Domain;

public sealed record Feat(
    string Id,
    string Name,
    IReadOnlyList<string> Types,
    IReadOnlyList<Prerequisite> Prerequisites,
    string? Description,
    string? Benefit,
    string? Normal,
    Source? Source);
