using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

[OutputCache(PolicyName = "Catalog")]
public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Monster>? CatalogPage { get; private set; }
    public IReadOnlyList<decimal> ChallengeRatings => catalogs.MonsterBuckets;
    public IReadOnlyList<string> Types => catalogs.MonsterTypeBuckets;
    public IReadOnlyList<string> Sources => catalogs.MonsterSourceBuckets;
    public IReadOnlyList<string> InitialBuckets => catalogs.MonsterInitialBuckets;

    public IActionResult OnGet(string? initial, [FromQuery] int page = 1)
    {
        if (!string.IsNullOrWhiteSpace(initial))
        {
            CatalogPage = catalogs.GetMonstersByInitial(initial, page);
            if (CatalogPage is null)
                return NotFound();
        }

        if (catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);

        return Page();
    }
}
