using System.Globalization;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web;

public static class CatalogText
{
    private static readonly IReadOnlyDictionary<string, string> Sources =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pfrpg"] = "Manuel des joueurs",
            ["bestiary"] = "Bestiaire",
            ["bestiary2"] = "Bestiaire 2",
            ["bestiary3"] = "Bestiaire 3",
            ["bestiary4"] = "Bestiaire 4",
            ["bestiary5"] = "Bestiaire 5",
            ["codexmonstrueux"] = "Codex monstrueux",
            ["apg"] = "Règles avancées",
            ["um"] = "L’art de la magie",
            ["uc"] = "L’art de la guerre",
            ["paizoBlog"] = "Blog Paizo",
            ["bookofthedamned"] = "Livre des damnés"
        };

    private static readonly IReadOnlyDictionary<string, string> SpellLists =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["barbarian"] = "Barbare",
            ["bard"] = "Barde",
            ["cleric"] = "Prêtre",
            ["druid"] = "Druide",
            ["ranger"] = "Rôdeur",
            ["paladin"] = "Paladin",
            ["sorcerer-wizard"] = "Ensorceleur / Magicien",
            ["witch"] = "Sorcière",
            ["alchemist"] = "Alchimiste",
            ["antipaladin"] = "Antipaladin",
            ["inquisitor"] = "Inquisiteur",
            ["oracle"] = "Oracle",
            ["summoner"] = "Conjurateur",
            ["summoner-unchained"] = "Conjurateur déchaîné",
            ["shaman"] = "Chaman",
            ["hypnotiseur"] = "Hypnotiseur",
            ["medium"] = "Médium",
            ["occultiste"] = "Occultiste",
            ["psychiste"] = "Psychiste",
            ["spirite"] = "Spirite"
        };

    private static readonly IReadOnlyDictionary<string, string> SpellSchools =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Abjuration"] = "Abjuration",
            ["Conjuration"] = "Invocation",
            ["Divination"] = "Divination",
            ["Enchantment"] = "Enchantement",
            ["Evocation"] = "Évocation",
            ["Illusion"] = "Illusion",
            ["Necromancy"] = "Nécromancie",
            ["Transmutation"] = "Transmutation"
        };

    private static readonly IReadOnlyDictionary<string, string> MonsterTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Aberration"] = "Aberration",
            ["Animal"] = "Animal",
            ["Construct"] = "Créature artificielle",
            ["Dragon"] = "Dragon",
            ["Fey"] = "Fées",
            ["Humanoid"] = "Humanoïde",
            ["MagicalBeast"] = "Bête magique",
            ["MonstrousHumanoid"] = "Humanoïde monstrueux",
            ["Ooze"] = "Vase",
            ["Outsider"] = "Extérieur",
            ["Plant"] = "Plante",
            ["Undead"] = "Mort-vivant",
            ["Vermin"] = "Vermine"
        };

    private static readonly IReadOnlyDictionary<string, string> FeatTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Combat"] = "Combat",
            ["Critical"] = "Critique",
            ["General"] = "Général",
            ["Grit"] = "Audace",
            ["ItemCreation"] = "Création d’objets",
            ["Metamagic"] = "Métamagie",
            ["Monster"] = "Monstre",
            ["Performance"] = "Spectacle",
            ["Style"] = "École",
            ["Teamwork"] = "Équipe"
        };

    public static string FormatChallengeRating(decimal challengeRating) =>
        challengeRating.ToString("G29", CultureInfo.InvariantCulture);

    public static string FormatSource(string value) => Format(value, Sources);

    public static string FormatSpellList(string value) => Format(value, SpellLists);

    public static string FormatSpellSchool(string value) => Format(value, SpellSchools);

    public static string FormatMonsterType(string value) => Format(value, MonsterTypes);

    public static string FormatFeatType(string value) => Format(value, FeatTypes);

    public static string FormatPrerequisite(Prerequisite prerequisite)
    {
        var parts = new List<string>();
        var kind = prerequisite.Type ?? prerequisite.OtherType;
        if (!string.IsNullOrWhiteSpace(kind))
            parts.Add(kind);
        if (!string.IsNullOrWhiteSpace(prerequisite.Value))
            parts.Add(prerequisite.Value);
        if (prerequisite.Number is { } number)
            parts.Add(number.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(prerequisite.Description))
            parts.Add(prerequisite.Description);
        if (prerequisite.IsChoice)
            parts.Add($"Au choix : {string.Join(" / ", prerequisite.Items.Select(FormatPrerequisite))}");

        return string.Join(" — ", parts);
    }

    private static string Format(string value, IReadOnlyDictionary<string, string> labels) =>
        labels.TryGetValue(value, out var label) ? label : value;
}
