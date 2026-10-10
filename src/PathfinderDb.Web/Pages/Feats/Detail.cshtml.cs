using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Feats;

[OutputCache(PolicyName = "Catalog")]
public sealed class DetailModel(CatalogService catalogs) : PageModel
{
    public Feat? Feat { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Feat = catalogs.GetFeat(slug);
        if (Feat is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return Feat is null ? NotFound() : Page();
    }
}
