using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogRouteTests
{
    [Theory]
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/ByType.cshtml", "@page \"/dons/type/{type}\"")]
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/BySource.cshtml", "@page \"/dons/source/{source}\"")]
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySchool.cshtml", "@page \"/sorts/ecole/{school}\"")]
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByClass.cshtml", "@page \"/sorts/classe/{class}\"")]
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySource.cshtml", "@page \"/sorts/source/{source}\"")]
    public void Dimension_pages_declare_canonical_routes(string relativePath, string route)
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var markup = File.ReadAllText(Path.Combine(
            repositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

        Assert.Contains(route, markup);
    }

    [Fact]
    public void Landing_pages_describe_dimension_selection_without_forcing_a_catalog_page()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var feats = File.ReadAllText(Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Feats", "Index.cshtml"));
        var spells = File.ReadAllText(Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Spells", "Index.cshtml"));

        Assert.Contains("Par type", feats);
        Assert.Contains("Par école", spells);
        Assert.DoesNotContain("Model.CatalogPage!.Bucket", feats);
        Assert.DoesNotContain("Model.CatalogPage!.Bucket", spells);
    }

    [Fact]
    public void Inline_bucket_lists_hide_default_list_markers()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var stylesheet = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "wwwroot", "css", "site.css"));

        Assert.Matches(@"\.bucket-nav\s*\{[^}]*list-style:\s*none", stylesheet);
    }
}
