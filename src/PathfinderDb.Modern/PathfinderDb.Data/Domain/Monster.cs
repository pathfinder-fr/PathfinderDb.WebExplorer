namespace PathfinderDb.Data.Domain;

public sealed record Monster(
    string Id,
    string Name,
    decimal? ChallengeRating,
    string? Climate,
    string? Environment,
    string? Type,
    Source? Source);
