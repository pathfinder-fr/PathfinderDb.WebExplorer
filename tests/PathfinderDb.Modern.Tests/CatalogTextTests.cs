using PathfinderDb.Data.Domain;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogTextTests
{
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
