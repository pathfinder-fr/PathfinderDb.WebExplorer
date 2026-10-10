namespace PathfinderDb.Data.Domain;

public sealed record Prerequisite(
    string? Type,
    string? OtherType,
    string? Value,
    decimal? Number,
    string? Description,
    IReadOnlyList<Prerequisite> Items)
{
    public bool IsChoice => Items.Count > 0;
}
