using PathfinderDb.Data.Domain;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class CatalogTextTests
{
    private readonly CatalogText _text = new();

    [Fact]
    public void Formats_challenge_rating_with_invariant_decimal_separator()
    {
        var text = _text.FormatChallengeRating(1.5m);

        Assert.Equal("1.5", text);
    }

    [Fact]
    public void Omits_decimal_suffix_for_integer_challenge_rating()
    {
        var text = _text.FormatChallengeRating(14.0m);

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

        var text = _text.FormatPrerequisite(prerequisite);

        Assert.Contains("Feat", text);
        Assert.Contains("Combat Expertise", text);
        Assert.Contains("Acrobatics", text);
        Assert.Contains("5", text);
    }

    [Theory]
    [InlineData("Fey", "Fées")]
    [InlineData("Outsider", "Extérieur")]
    [InlineData("MagicalBeast", "Bête magique")]
    [InlineData("unknown-type", "unknown-type")]
    public void Formats_monster_types_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatMonsterType(value));
    }

    [Theory]
    [InlineData("um", "L’art de la magie")]
    [InlineData("apg", "Règles avancées")]
    [InlineData("unknown-source", "unknown-source")]
    public void Formats_sources_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatSource(value));
    }

    [Theory]
    [InlineData("bard", "Barde")]
    [InlineData("magus", "Magus")]
    [InlineData("bloodrager", "Bloodrager")]
    [InlineData("sahir-afiyun", "Sahir-Afiyun")]
    [InlineData("sorcerer-wizard", "Ensorceleur / Magicien")]
    [InlineData("psychiste", "Psychiste")]
    [InlineData("unknown-class", "unknown-class")]
    public void Formats_spell_lists_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatSpellList(value));
    }

    [Theory]
    [InlineData("Conjuration", "Invocation")]
    [InlineData("Necromancy", "Nécromancie")]
    [InlineData("unknown-school", "unknown-school")]
    public void Formats_spell_schools_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatSpellSchool(value));
    }

    [Theory]
    [InlineData("Combat", "Combat")]
    [InlineData("ItemCreation", "Création d’objets")]
    [InlineData("unknown-feat-type", "unknown-feat-type")]
    public void Formats_feat_types_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatFeatType(value));
    }

    [Theory]
    [InlineData("Verbal", "Verbale")]
    [InlineData("Somatic", "Somatique")]
    [InlineData("Material", "Matérielle")]
    [InlineData("Focus", "Focaliseur")]
    [InlineData("DivineFocus", "Focaliseur divin")]
    [InlineData("FocusOrDivineFocus", "Focaliseur ou focaliseur divin")]
    [InlineData("somatic", "Somatique")]
    [InlineData("UnknownComponent", "UnknownComponent")]
    public void Formats_spell_components_in_french_with_unknown_fallback(string value, string expected)
    {
        Assert.Equal(expected, _text.FormatSpellComponent(value));
    }

    [Theory]
    [InlineData("https://www.pathfinder-fr.org/Wiki/Spell", true)]
    [InlineData("http://sub.pathfinder-fr.org/Wiki/Spell", true)]
    [InlineData("https://pathfinder-fr.org/Wiki/Spell", true)]
    [InlineData("https://paizo.com/pathfinderRPG/prd/spell", false)]
    [InlineData("https://www.regles-pathfinder.fr/spell", false)]
    [InlineData("https://pathfinder-fr.org.attacker.test/spell", false)]
    [InlineData("/Wiki/Spell", false)]
    [InlineData("not a URI", false)]
    public void Accepts_only_absolute_pathfinder_wiki_reference_hosts(string href, bool expected)
    {
        var reference = new Reference(null, href, null, "fr");

        Assert.Equal(expected, _text.IsPathfinderWikiReference(reference));
    }

    [Fact]
    public void Prefers_the_reference_url_string_when_filtering_wiki_links()
    {
        var reference = new Reference(
            null,
            "https://paizo.com/spell",
            "https://www.pathfinder-fr.org/Wiki/Spell",
            "fr");

        Assert.True(_text.IsPathfinderWikiReference(reference));
    }
}
