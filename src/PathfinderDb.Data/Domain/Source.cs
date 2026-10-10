namespace PathfinderDb.Data.Domain;

public sealed record Source(string Id, IReadOnlyList<Reference> References);
