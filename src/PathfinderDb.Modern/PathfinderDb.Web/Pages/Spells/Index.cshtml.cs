using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Spell>? CatalogPage { get; private set; }
    public IReadOnlyList<string> Buckets { get; private set; } = [];

    public IActionResult OnGet(string? initial, [FromQuery] int page = 1)
    {
        Buckets = catalogs.SpellBuckets;
        CatalogPage = catalogs.GetSpells(initial, page);
        return CatalogPage is null ? NotFound() : Page();
    }
}
