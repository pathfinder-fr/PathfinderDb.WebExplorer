using Xunit;
using System.Runtime.Versioning;

namespace PathfinderDb.Modern.Tests;

public sealed class ModernProjectSmokeTests
{
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
