namespace PathfinderDb.Data.Domain;

public sealed record CatalogPage<T>(
    string Bucket,
    int Page,
    int PageCount,
    int TotalCount,
    IReadOnlyList<T> Items)
{
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < PageCount;
}
