using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

[OutputCache(PolicyName = "Catalog")]
public sealed class BySourceModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Spell>? CatalogPage { get; private set; }

    public IActionResult OnGet(string source, [FromQuery] int page = 1)
    {
        CatalogPage = catalogs.GetSpellsBySource(source, page);
        if (CatalogPage is not null && catalogs.SnapshotVersion is { } version &&
            CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
            return StatusCode(StatusCodes.Status304NotModified);

        return CatalogPage is null ? NotFound() : Page();
    }
}
