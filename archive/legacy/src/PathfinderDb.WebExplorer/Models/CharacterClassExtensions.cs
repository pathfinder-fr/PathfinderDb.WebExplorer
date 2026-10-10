namespace DbBrowser.Models
{
    public static class CharacterClassExtensions
    {
        public static readonly ItemDisplay[] AllCharacterClasses = new[] {
            new ItemDisplay { Id = "barbare", DisplayName = "Barbare", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "barde", DisplayName = "Barde", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "druide", DisplayName = "Druide", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "ensorceleur", DisplayName = "Ensorceleur", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "guerrier", DisplayName = "Guerrier", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "magicien", DisplayName = "Magicien", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "moine", DisplayName = "Moine", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "paladin", DisplayName = "Paladin", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "prêtre", DisplayName = "Prêtre", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "rôdeur", DisplayName = "Rôdeur", Group = "Manuel des joueurs" },
            new ItemDisplay { Id = "roublard", DisplayName = "Roublard", Group = "Manuel des joueurs" },

            new ItemDisplay { Id = "alchimiste", DisplayName = "Alchimiste", Group = "Règles avancées" },
            new ItemDisplay { Id = "antipaladin", DisplayName = "Antipaladin", Group = "Règles avancées" },
            new ItemDisplay { Id = "chevalier", DisplayName = "Chevalier", Group = "Règles avancées" },
            new ItemDisplay { Id = "inquisiteur", DisplayName = "Inquisiteur", Group = "Règles avancées" },
            new ItemDisplay { Id = "conjurateur", DisplayName = "Conjurateur", Group = "Règles avancées" },
            new ItemDisplay { Id = "oracle", DisplayName = "Oracle", Group = "Règles avancées" },
            new ItemDisplay { Id = "socrière", DisplayName = "Sorcière", Group = "Règles avancées" },
            
            new ItemDisplay { Id = "magus", DisplayName = "Magus", Group = "L'art de la magie" },

            new ItemDisplay { Id = "ninja", DisplayName = "Ninja", Group = "L'art de la guerre" },
            new ItemDisplay { Id = "pistolier", DisplayName = "Pistolier", Group = "L'art de la guerre" },
            new ItemDisplay { Id = "samuraï", DisplayName = "Samuraï", Group = "L'art de la guerre" },
        };
    }
}