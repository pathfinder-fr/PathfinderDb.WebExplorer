using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

[OutputCache(PolicyName = "Catalog")]
public sealed class ByLevelModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Spell>? CatalogPage { get; private set; }

    public IActionResult OnGet(string level, [FromQuery] int page = 1)
    {
        if (!int.TryParse(level, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedLevel) ||
            parsedLevel < 0)
            return NotFound();

        CatalogPage = catalogs.GetSpellsByLevel(parsedLevel, page);
        if (CatalogPage is null)
            return NotFound();

        if (catalogs.SnapshotVersion is { } version &&
            CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
            return StatusCode(StatusCodes.Status304NotModified);

        return Page();
    }
}
