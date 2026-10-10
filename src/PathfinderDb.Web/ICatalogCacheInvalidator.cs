namespace PathfinderDb.Web;

public interface ICatalogCacheInvalidator
{
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
