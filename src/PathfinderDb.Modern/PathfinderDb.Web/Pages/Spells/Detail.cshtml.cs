using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Spells;

public sealed class DetailModel(CatalogService catalogs) : PageModel
{
    public Spell? Spell { get; private set; }

    public IActionResult OnGet(string slug)
    {
        Spell = catalogs.GetSpell(slug);
        return Spell is null ? NotFound() : Page();
    }
}
