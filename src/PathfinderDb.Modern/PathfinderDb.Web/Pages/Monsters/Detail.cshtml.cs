using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

public sealed class DetailModel(CatalogService catalogs) : PageModel
{
    public Monster? Monster { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Monster = catalogs.GetMonster(slug);
        if (Monster is not null && catalogs.SnapshotVersion is { } version)
            if (CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
                return StatusCode(StatusCodes.Status304NotModified);
        return Monster is null ? NotFound() : Page();
    }
}
