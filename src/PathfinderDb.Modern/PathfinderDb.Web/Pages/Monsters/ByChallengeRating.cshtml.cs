using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

public sealed class ByChallengeRatingModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Monster>? CatalogPage { get; private set; }

    public IActionResult OnGet(string challengeRating, [FromQuery] int page = 1)
    {
        CatalogPage = catalogs.GetMonsters(challengeRating, page);
        if (CatalogPage is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return CatalogPage is null ? NotFound() : Page();
    }
}
