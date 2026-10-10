namespace PathfinderDb.Web;

public interface IGitRepository
{
    Task<GitPullResult> PullAsync(string rootPath, CancellationToken cancellationToken = default);
}
