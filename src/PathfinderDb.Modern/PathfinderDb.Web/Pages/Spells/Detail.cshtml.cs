using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

[OutputCache(PolicyName = "Catalog")]
public sealed class DetailModel(CatalogService catalogs) : PageModel
{
    public Spell? Spell { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Spell = catalogs.GetSpell(slug);
        if (Spell is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return Spell is null ? NotFound() : Page();
    }
}
