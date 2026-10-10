namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System.Collections.Generic;
using System.Linq;

    public class SpellIndexViewModel
    {
        public SpellIndexQuery Query { get; set; }

        public List<Spell> Spells { get; set; }

        public IEnumerable<IGrouping<int, Spell>> SpellGroups { get; set; }

        public Source[] AllSources { get; set; }

        public string DisplayGroupKey(int key)
        {
            switch (key)
            {
                case 0: return "Niveau 0 (Oraison / Tour)";
                case 1: return "Niveau 1";
                case 2: return "Niveau 2";
                case 3: return "Niveau 3";
                case 4: return "Niveau 4";
                case 5: return "Niveau 5";
                case 6: return "Niveau 6";
                case 7: return "Niveau 7";
                case 8: return "Niveau 8";
                case 9: return "Niveau 9";
                default: return string.Empty;
            }
        }

        public ItemDisplay[] AllLevels = new ItemDisplay[] {
            new ItemDisplay { NumericId = 0, DisplayName = "Niveau 0 (Oraison / Tour)" },
            new ItemDisplay { NumericId = 1, DisplayName = "Niveau 1" },
            new ItemDisplay { NumericId = 2, DisplayName = "Niveau 2" },
            new ItemDisplay { NumericId = 3, DisplayName = "Niveau 3" },
            new ItemDisplay { NumericId = 4, DisplayName = "Niveau 4" },
            new ItemDisplay { NumericId = 5, DisplayName = "Niveau 5" },
            new ItemDisplay { NumericId = 6, DisplayName = "Niveau 6" },
            new ItemDisplay { NumericId = 7, DisplayName = "Niveau 7" },
            new ItemDisplay { NumericId = 8, DisplayName = "Niveau 8" },
            new ItemDisplay { NumericId = 9, DisplayName = "Niveau 9" },
        };

        public ItemDisplay[] AllLists = new ItemDisplay[] {
            new ItemDisplay { Id = SpellList.Ids.Alchemist, DisplayName = "Alchimiste" },
            new ItemDisplay { Id = SpellList.Ids.AntiPaladin, DisplayName = "Antipaladin" },
            new ItemDisplay { Id = SpellList.Ids.Bard, DisplayName = "Barde" },
            new ItemDisplay { Id = SpellList.Ids.Summoner, DisplayName = "Conjurateur" },
            new ItemDisplay { Id = SpellList.Ids.Druid, DisplayName = "Druide" },
            new ItemDisplay { Id = SpellList.Ids.SorcererWizard, DisplayName = "Ensorceleur/magicien" },
            new ItemDisplay { Id = SpellList.Ids.Inquisitor, DisplayName = "Inquisiteur" },
            new ItemDisplay { Id = SpellList.Ids.Magus, DisplayName = "Magus" },
            new ItemDisplay { Id = SpellList.Ids.Oracle, DisplayName = "Oracle" },
            new ItemDisplay { Id = SpellList.Ids.Paladin, DisplayName = "Paladin" },
            new ItemDisplay { Id = SpellList.Ids.Cleric, DisplayName = "Prêtre" },
            new ItemDisplay { Id = SpellList.Ids.Ranger, DisplayName = "Rôdeur" },
            new ItemDisplay { Id = SpellList.Ids.Witch, DisplayName = "Sorcière" },
        };
    }
}