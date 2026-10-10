namespace PathfinderDb.Data.Import;

public sealed class DataValidationResult
{
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public bool IsValid => Errors.Count == 0;
}

public sealed record DataLoadResult(
    Domain.DataSnapshot? Snapshot,
    DataValidationResult Validation,
    string? Version)
{
    public bool IsSuccess => Snapshot is not null && Validation.IsValid;
}
