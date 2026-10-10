namespace DbBrowser.Models
{
    using System;
    using System.Linq;
    using PathfinderDb.Schema;

    public static class FeatTypeExtensions
    {
        public static readonly ItemDisplay[] AllFeatTypes = Enum
            .GetValues(typeof(FeatType))
            .Cast<FeatType>()
            .Select(ft => ft.AsDisplay())
            .ToArray();

        public static ItemDisplay AsDisplay(this FeatType type)
        {
            return new ItemDisplay
            {
                Id = type.ToString(),
                DisplayName = type.AsDisplayName()
            };
        }

        public static string AsDisplayName(this FeatType type)
        {
            switch (type)
            {
                case FeatType.Combat: return "Combat";
                case FeatType.Critical: return "Critique";
                case FeatType.General: return "Général";
                case FeatType.Grit: return "Audace";
                case FeatType.ItemCreation: return "Création d'objets";
                case FeatType.Metamagic: return "Métamagie";
                case FeatType.Monster: return "Monstre";
                case FeatType.Performance: return "Spectacle";
                case FeatType.Style: return "École";
                case FeatType.Teamwork: return "Équipe";
                default: return type.ToString();
            }
        }

        public static string AsCssClass(this FeatType type)
        {
            if (type == FeatType.General)
            {
                return string.Empty;
            }
            else
            {
                return "label-info";
            }
        }
    }
}