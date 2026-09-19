using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

[OutputCache(PolicyName = "Catalog")]
public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public IReadOnlyList<decimal> ChallengeRatings => catalogs.MonsterBuckets;

public IActionResult OnGet()
{
    if (catalogs.SnapshotVersion is { } version)
        if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
            return StatusCode(StatusCodes.Status304NotModified);

    return Page();
}
}
