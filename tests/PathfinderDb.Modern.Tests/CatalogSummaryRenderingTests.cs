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
    public void Feat_summary_includes_types_prerequisites_source_and_detail_link()
    {
        var partial = ReadPartial("_FeatCatalogItem.cshtml");

        Assert.Contains("/dons/detail/@Model.Id", partial);
        Assert.Contains("Types", partial);
        Assert.Contains("FormatPrerequisite", partial);
        Assert.Contains("FormatSource", partial);
        Assert.Contains("@if", partial);
    }

    [Fact]
    public void Spell_summary_includes_key_fields_and_original_name_label()
    {
        var partial = ReadPartial("_SpellCatalogItem.cshtml");

        Assert.Contains("/sorts/detail/@Model.Id", partial);
        Assert.Contains("FormatSpellSchool", partial);
        Assert.Contains("FormatSpellList", partial);
        Assert.Contains("FormatSpellComponent", partial);
        Assert.Contains("Portée", partial);
        Assert.Contains("Cible", partial);
        Assert.Contains("Temps d'incantation", partial);
        Assert.Contains("Nom VO", partial);
        Assert.Contains("en-US:name", partial);
    }

    [Fact]
    public void Monster_summary_includes_catalog_fields_and_detail_link()
    {
        var partial = ReadPartial("_MonsterCatalogItem.cshtml");

        Assert.Contains("/monstres/detail/@Model.Id", partial);
        Assert.Contains("FormatChallengeRating", partial);
        Assert.Contains("FormatMonsterType", partial);
        Assert.Contains("Environnement", partial);
        Assert.Contains("Climat", partial);
        Assert.Contains("FormatSource", partial);
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
