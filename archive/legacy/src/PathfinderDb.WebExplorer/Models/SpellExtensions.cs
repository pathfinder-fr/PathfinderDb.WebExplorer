using PathfinderDb.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DbBrowser.Models
{
    public static class SpellExtensions
    {
        public static string SchoolDisplayName(this Spell spell)
        {
            switch (spell.School)
            {
                case SpellSchool.Enchantment: return "Enchantement";
                case SpellSchool.Evocation: return "Évocation";
                case SpellSchool.Necromancy: return "Nécromancie";
                case SpellSchool.Universal: return "Universel";
                default: return spell.School.ToString();
            }
    }
    }
}