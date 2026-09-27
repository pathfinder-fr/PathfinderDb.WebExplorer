using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogSummaryRenderingTests
{
    private static readonly string RepositoryRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));

    [Theory]
    [InlineData("Feats/Index.cshtml", "_FeatCatalogItem")]
    [InlineData("Feats/ByType.cshtml", "_FeatCatalogItem")]
    [InlineData("Feats/BySource.cshtml", "_FeatCatalogItem")]
    [InlineData("Spells/Index.cshtml", "_SpellCatalogItem")]
    [InlineData("Spells/BySchool.cshtml", "_SpellCatalogItem")]
    [InlineData("Spells/ByClass.cshtml", "_SpellCatalogItem")]
    [InlineData("Spells/BySource.cshtml", "_SpellCatalogItem")]
    [InlineData("Spells/ByLevel.cshtml", "_SpellCatalogItem")]
    [InlineData("Monsters/Index.cshtml", "_MonsterCatalogItem")]
    [InlineData("Monsters/ByChallengeRating.cshtml", "_MonsterCatalogItem")]
    [InlineData("Monsters/ByType.cshtml", "_MonsterCatalogItem")]
    [InlineData("Monsters/BySource.cshtml", "_MonsterCatalogItem")]
    public void Every_catalog_result_page_uses_its_shared_item_partial(string relativePage, string partialName)
    {
        var page = ReadPage(relativePage);

        Assert.Contains($"name=\"{partialName}\"", page);
    }

    [Fact]
    public void Razor_pages_register_the_partial_tag_helper()
    {
        var imports = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "_ViewImports.cshtml"));

        Assert.Contains("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers", imports);
    }

    [Fact]
    public void Feat_summary_is_compact_and_keeps_gameplay_fields_and_detail_link()
    {
        var partial = ReadPartial("_FeatCatalogItem.cshtml");

        Assert.Contains("/dons/detail/@Model.Id", partial);
        Assert.Contains("Types", partial);
        Assert.Contains("FormatPrerequisite", partial);
        Assert.Contains("FormatSource", partial);
        Assert.Contains("catalog-summary-details", partial);
        Assert.DoesNotContain("Wiki Pathfinder-fr.org", partial);
    }

    [Fact]
    public void Spell_summary_is_compact_and_hides_wiki_and_original_name()
    {
        var partial = ReadPartial("_SpellCatalogItem.cshtml");

        Assert.Contains("/sorts/detail/@Model.Id", partial);
        Assert.Contains("FormatSpellSchool", partial);
        Assert.Contains("FormatSpellList", partial);
        Assert.Contains("FormatSpellComponent", partial);
        Assert.DoesNotContain("Model.Range", partial);
        Assert.DoesNotContain("Model.Target", partial);
        Assert.DoesNotContain("Model.CastingTime", partial);
        Assert.Contains("catalog-summary-details", partial);
        Assert.DoesNotContain("Nom VO", partial);
        Assert.DoesNotContain("en-US:name", partial);
        Assert.DoesNotContain("Wiki Pathfinder-fr.org", partial);
    }

    [Fact]
    public void Monster_summary_is_compact_and_keeps_catalog_fields_and_detail_link()
    {
        var partial = ReadPartial("_MonsterCatalogItem.cshtml");

        Assert.Contains("/monstres/detail/@Model.Id", partial);
        Assert.Contains("FormatChallengeRating", partial);
        Assert.Contains("FormatMonsterType", partial);
        Assert.Contains("Environnement", partial);
        Assert.Contains("Climat", partial);
        Assert.Contains("FormatSource", partial);
        Assert.Contains("catalog-summary-details", partial);
    }

    private static string ReadPage(string relativePage) =>
        File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages",
            relativePage.Replace('/', Path.DirectorySeparatorChar)));

    private static string ReadPartial(string name) =>
        File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Shared", name));
}
