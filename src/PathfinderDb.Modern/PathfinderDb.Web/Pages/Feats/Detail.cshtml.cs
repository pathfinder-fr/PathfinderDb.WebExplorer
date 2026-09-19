using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Feats;

public sealed class DetailModel(CatalogService catalogs) : PageModel
{
    public Feat? Feat { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Feat = catalogs.GetFeat(slug);
        return Feat is null ? NotFound() : Page();
    }
}
