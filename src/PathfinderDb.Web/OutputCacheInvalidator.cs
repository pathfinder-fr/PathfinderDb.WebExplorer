using Microsoft.AspNetCore.OutputCaching;

namespace PathfinderDb.Web;

public sealed class OutputCacheInvalidator(IOutputCacheStore store) : ICatalogCacheInvalidator
{
    public async Task InvalidateAsync(CancellationToken cancellationToken = default) =>
        await store.EvictByTagAsync("catalog", cancellationToken);
}
