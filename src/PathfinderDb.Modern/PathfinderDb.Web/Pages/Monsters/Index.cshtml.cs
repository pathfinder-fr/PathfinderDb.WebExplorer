using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web.Pages.Monsters;

public sealed class IndexModel(CatalogService catalogs) : PageModel
{
    public IReadOnlyList<decimal> ChallengeRatings => catalogs.MonsterBuckets;
}
