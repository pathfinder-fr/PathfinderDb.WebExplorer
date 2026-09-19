using PathfinderDb.Data.Domain;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogTextTests
{
    [Fact]
    public void Formats_challenge_rating_with_invariant_decimal_separator()
    {
        var text = CatalogText.FormatChallengeRating(1.5m);

        Assert.Equal("1.5", text);
    }

    [Fact]
    public void Omits_decimal_suffix_for_integer_challenge_rating()
    {
        var text = CatalogText.FormatChallengeRating(14.0m);

        Assert.Equal("14", text);
    }

    [Fact]
    public void Formats_nested_feat_prerequisites_for_detail_pages()
    {
        var prerequisite = new Prerequisite(
            "Feat",
            null,
            "Combat Expertise",
            null,
            null,
            [
                new Prerequisite("Skill", null, "Acrobatics", 5, null, [])
            ]);

        var text = CatalogText.FormatPrerequisite(prerequisite);

        Assert.Contains("Feat", text);
        Assert.Contains("Combat Expertise", text);
        Assert.Contains("Acrobatics", text);
        Assert.Contains("5", text);
    }
}
