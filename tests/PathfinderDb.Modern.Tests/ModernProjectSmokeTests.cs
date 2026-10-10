using Xunit;
using System.Runtime.Versioning;

namespace PathfinderDb.Modern.Tests;

public sealed class ModernProjectSmokeTests
{
    [Fact]
    public void Current_projects_are_promoted_and_legacy_site_is_archived()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));

        Assert.True(File.Exists(Path.Combine(repositoryRoot, "PathfinderDb.sln")));
        Assert.True(File.Exists(Path.Combine(repositoryRoot, "src", "PathfinderDb.Data", "PathfinderDb.Data.csproj")));
        Assert.True(File.Exists(Path.Combine(repositoryRoot, "src", "PathfinderDb.Web", "PathfinderDb.Web.csproj")));
        Assert.True(File.Exists(Path.Combine(repositoryRoot, "archive", "legacy", "src", "PathfinderDb.WebExplorer", "PathfinderDb.WebExplorer.csproj")));
    }

    [Fact]
    public void Data_project_exposes_the_modern_target_framework()
    {
        var assembly = typeof(PathfinderDb.Data.Domain.DataSnapshot).Assembly;

        var targetFramework = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), inherit: false)
            .Cast<TargetFrameworkAttribute>()
            .Single();

        Assert.Equal(".NETCoreApp,Version=v10.0", targetFramework.FrameworkName);
    }
}
