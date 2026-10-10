using Microsoft.AspNetCore.Mvc.RazorPages;
using PathfinderDb.Data.Runtime;

public sealed class IndexModel(IDataSnapshotProvider provider) : PageModel
{
    public DataLoadStatus Status => provider.Status;
}
