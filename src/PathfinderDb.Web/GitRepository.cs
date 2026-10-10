using System.Diagnostics;

namespace PathfinderDb.Web;

public sealed class GitRepository : IGitRepository
{
    public async Task<GitPullResult> PullAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo("git")
            {
                WorkingDirectory = rootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("pull");
            startInfo.ArgumentList.Add("--ff-only");

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Unable to start git.");
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;
            var error = await errorTask;

            return process.ExitCode == 0
                ? GitPullResult.Success(output.Trim())
                : GitPullResult.Failure(error.Trim());
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
            or System.ComponentModel.Win32Exception or DirectoryNotFoundException)
        {
            return GitPullResult.Failure(exception.Message);
        }
    }
}
