using Microsoft.AspNetCore.Http;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogCacheHeadersTests
{
    [Fact]
    public void Applies_shared_cache_policy_and_snapshot_etag()
    {
        var context = new DefaultHttpContext();

        CatalogCacheHeaders.Apply(context.Response, "snapshot-123", "/dons/A?page=2");

        Assert.Equal(
            "public,max-age=300,s-maxage=3600,stale-while-revalidate=86400",
            context.Response.Headers.CacheControl.ToString());
        Assert.Equal("\"snapshot-123-/dons/A?page=2\"", context.Response.Headers.ETag.ToString());
    }

    [Fact]
    public void Detects_matching_if_none_match()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.IfNoneMatch = "\"snapshot-123-/dons/A?page=2\"";

        var notModified = CatalogCacheHeaders.Apply(
            context.Response,
            "snapshot-123",
            "/dons/A?page=2");

        Assert.True(notModified);
        Assert.Equal(StatusCodes.Status304NotModified, context.Response.StatusCode);
    }
}
