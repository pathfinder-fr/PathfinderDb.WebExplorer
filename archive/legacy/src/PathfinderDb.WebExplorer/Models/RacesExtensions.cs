namespace DbBrowser.Models
{
    public static class RacesExtensions
    {
        public static readonly ItemDisplay[] AllRaces = new[] { 
            new ItemDisplay { Id = "halfElf", DisplayName = "Demi-Elfe" },
            new ItemDisplay { Id = "halfOrc", DisplayName = "Demi-Orque" },
            new ItemDisplay { Id = "elf", DisplayName = "Elfe" },
            new ItemDisplay { Id = "gnome", DisplayName = "Gnome" },
            new ItemDisplay { Id = "halfling", DisplayName = "Halfelin" },
            new ItemDisplay { Id = "human", DisplayName = "Humain" },
            new ItemDisplay { Id = "dwarf", DisplayName = "Nain" },
        };
    }
}