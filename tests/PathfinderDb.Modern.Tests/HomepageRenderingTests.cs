using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class HomepageRenderingTests
{
    [Fact]
    public void Homepage_identifies_first_edition_and_keeps_reader_navigation_prominent()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var markup = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Index.cshtml"));

        Assert.Contains("Pathfinder première édition", markup);
        Assert.Contains("Fonds de Pathfinder première édition", markup);
        Assert.Contains("pf-fr-db", markup);
        Assert.DoesNotContain("État du catalogue", markup);
    }

    [Fact]
    public void Homepage_shows_version_and_unavailable_errors_in_compact_technical_footer()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var markup = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Index.cshtml"));

        Assert.Contains("homepage-technical-status", markup);
        Assert.Contains("Model.Status.Version", markup);
        Assert.Contains("Model.Status.Warnings.Count", markup);
        Assert.Contains("DataLoadState.Unavailable", markup);
        Assert.Contains("Model.Status.Errors", markup);
        Assert.Contains("role=\"alert\"", markup);
        Assert.DoesNotContain("detail-grid", markup);
        Assert.DoesNotContain("string.Join(\" \", Model.Status.Warnings)", markup);
    }

    [Fact]
    public void Homepage_portal_cards_show_catalog_counts_only_for_ready_data()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var layout = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Shared", "_Layout.cshtml"));
        var homepage = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Index.cshtml"));

        Assert.Contains("Pathfinder première édition", layout);
        Assert.DoesNotContain("catalogStatus", layout);
        Assert.Contains("DataLoadState.Ready", homepage);
        Assert.Contains("Model.Status.FeatCount", homepage);
        Assert.Contains("Model.Status.SpellCount", homepage);
        Assert.Contains("Model.Status.MonsterCount", homepage);
        Assert.Contains("class=\"portal-card\"", homepage);
        Assert.DoesNotContain("Dons @if", homepage);
        Assert.DoesNotContain("Sorts @if", homepage);
        Assert.DoesNotContain("Monstres @if", homepage);
    }
}
