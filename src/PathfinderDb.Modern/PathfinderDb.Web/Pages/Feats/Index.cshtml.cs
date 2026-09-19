using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Feats;

public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Feat>? CatalogPage { get; private set; }
    public IReadOnlyList<string> Buckets { get; private set; } = [];

    public IActionResult OnGet(string? initial, [FromQuery] int page = 1)
    {
        Buckets = catalogs.FeatBuckets;
        CatalogPage = catalogs.GetFeats(initial, page);
        if (CatalogPage is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return CatalogPage is null ? NotFound() : Page();
    }
}
