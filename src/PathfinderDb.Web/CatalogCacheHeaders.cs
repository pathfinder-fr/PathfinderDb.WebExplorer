using Microsoft.AspNetCore.Http;

namespace PathfinderDb.Web;

public static class CatalogCacheHeaders
{
    public const string CacheControlValue = "public,max-age=300,s-maxage=3600,stale-while-revalidate=86400";

    public static bool Apply(HttpResponse response, string snapshotVersion, string cacheKey)
    {
        response.Headers.CacheControl = CacheControlValue;
        var etag = $"\"{snapshotVersion}-{cacheKey.Replace("\"", "%22", StringComparison.Ordinal)}\"";
        response.Headers.ETag = etag;
        response.Headers.Vary = "Accept-Encoding";
        if (response.HttpContext.Request.Headers.IfNoneMatch.Any(value =>
                (value?.Split(',') ?? []).Any(candidate =>
                    candidate.Trim() is "*" || string.Equals(candidate.Trim(), etag, StringComparison.Ordinal))))
        {
            response.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }

        return false;
    }
}
