using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class HomepageRenderingTests
{
    [Fact]
    public void Homepage_places_reader_navigation_before_technical_status()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var markup = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Index.cshtml"));

        Assert.True(markup.IndexOf("Parcourir les dons", StringComparison.Ordinal) <
                    markup.IndexOf("État des données", StringComparison.Ordinal));
        Assert.Contains("pf-fr-db", markup);
    }
}
