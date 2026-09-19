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
    [InlineData("src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Monsters/Detail.cshtml", "@page \"/monstres/detail/{slug}\"")]
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
    public void Monster_landing_page_exposes_alphabetical_navigation()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var monsters = File.ReadAllText(Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Monsters", "Index.cshtml"));

        Assert.Contains("Par ordre alphabétique", monsters);
        Assert.Contains("/monstres/@bucket", monsters);
    }

    [Fact]
    public void Catalog_indexes_hide_selection_menus_when_a_bucket_is_selected()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var pages = new[]
        {
            Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Feats", "Index.cshtml"),
            Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Spells", "Index.cshtml"),
            Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Monsters", "Index.cshtml")
        };

        foreach (var page in pages)
        {
            var markup = File.ReadAllText(page);
            Assert.Contains("@if (Model.CatalogPage is null)", markup);
        }
    }

    [Fact]
    public void Catalog_selection_menus_use_compact_bullet_lists()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var stylesheet = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "wwwroot", "css", "site.css"));

        Assert.Contains(".selection-list", stylesheet);
        Assert.Contains("display: flex", stylesheet);
        Assert.Contains("flex-wrap: wrap", stylesheet);
        Assert.Matches(@"\.selection-list\s*\{[^}]*list-style:\s*none", stylesheet);
        Assert.Matches(@"\.selection-list\s*>\s*li\s*\{[^}]*border-radius:\s*\.6rem", stylesheet);
    }

    [Fact]
    public void Catalog_lists_share_one_three_column_style()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var stylesheet = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "wwwroot", "css", "site.css"));
        var monsters = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Monsters", "Index.cshtml"));
        var spells = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Spells", "Index.cshtml"));

        Assert.Matches(@"\.catalog-list\s*\{[^}]*grid-template-columns:\s*repeat\(3,", stylesheet);
        Assert.Contains("min-height: 2.9rem", stylesheet);
        Assert.Contains(".catalog-list > a,\r\n.catalog-list > li a", stylesheet);
        Assert.DoesNotContain(".bucket-nav", stylesheet);
        Assert.DoesNotContain("class=\"bucket-nav\"", monsters);
        Assert.DoesNotContain("class=\"bucket-nav\"", spells);
    }

    [Fact]
    public void Header_uses_white_brand_surface_with_brown_rules()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var stylesheet = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "wwwroot", "css", "site.css"));

        Assert.Matches(@"\.site-header\s*\{[^}]*background:\s*#fff", stylesheet);
        Assert.Contains("border-top: 2px solid var(--accent-dark)", stylesheet);
        Assert.Contains("border-bottom: 2px solid var(--accent-dark)", stylesheet);
        Assert.Contains(".main-nav a {\r\n    color: var(--accent-dark);", stylesheet);
    }

    [Fact]
    public void Detail_pages_render_original_reference_links_when_available()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var feats = File.ReadAllText(Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Feats", "Detail.cshtml"));
        var spells = File.ReadAllText(Path.Combine(repositoryRoot, "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Spells", "Detail.cshtml"));

        Assert.Contains("Model.Feat.References", feats);
        Assert.Contains("Model.Spell.References", spells);
        Assert.Contains("target=\"_blank\"", feats);
        Assert.Contains("target=\"_blank\"", spells);
    }
}
