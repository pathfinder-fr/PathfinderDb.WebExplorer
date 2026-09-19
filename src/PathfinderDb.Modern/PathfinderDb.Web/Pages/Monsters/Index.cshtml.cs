using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

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
