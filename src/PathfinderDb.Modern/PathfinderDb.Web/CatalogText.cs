using System.Globalization;
using PathfinderDb.Data.Domain;

namespace PathfinderDb.Web;

public static class CatalogText
{
    public static string FormatChallengeRating(decimal challengeRating) =>
        challengeRating.ToString(CultureInfo.InvariantCulture);

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
}
