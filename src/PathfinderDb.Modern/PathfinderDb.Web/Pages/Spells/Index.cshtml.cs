using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

[OutputCache(PolicyName = "Catalog")]
public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Spell>? CatalogPage { get; private set; }
    public IReadOnlyList<string> Buckets { get; private set; } = [];
    public IReadOnlyList<string> Schools => catalogs.SpellSchoolBuckets;
    public IReadOnlyList<string> Classes => catalogs.SpellListBuckets;
    public IReadOnlyList<string> Sources => catalogs.SpellSourceBuckets;

    public IActionResult OnGet(string? initial, [FromQuery] int page = 1)
    {
        Buckets = catalogs.SpellBuckets;
        if (string.IsNullOrWhiteSpace(initial))
            return Page();

        CatalogPage = catalogs.GetSpells(initial, page);
        if (CatalogPage is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return CatalogPage is null ? NotFound() : Page();
    }
}
