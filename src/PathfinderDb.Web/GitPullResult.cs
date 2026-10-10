namespace PathfinderDb.Web;

public sealed record GitPullResult(bool Succeeded, string Output, string Error)
{
    public static GitPullResult Success(string output) => new(true, output, string.Empty);
    public static GitPullResult Failure(string error) => new(false, string.Empty, error);
}
