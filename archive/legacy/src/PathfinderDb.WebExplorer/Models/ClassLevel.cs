using PathfinderDb.Schema;
using System.Collections.Generic;
using System.Linq;

namespace DbBrowser.Models
{
    public class ClassLevel
    {
        public ClassLevel()
        {
        }

        public ClassLevel(string arrayItem)
        {
            var parts = arrayItem.Split(ArrayExtensions.ArrayItemValueSeparator);
            this.Id = parts[0];
            this.Name = CharacterClassExtensions.AllCharacterClasses.First(c => c.Id == this.Id).DisplayName;
            this.Level = int.Parse(parts[1]);
        }

        public string Id { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", this.Name, this.Level); }
        }

        public string AsArrayItem()
        {
            return string.Concat(this.Id, ArrayExtensions.ArrayItemValueSeparator, this.Level);
        }

        public static bool MatchAll(ClassLevel[] levels, FeatPrerequisite p)
        {
            if (p.Type != FeatPrerequisiteType.ClassLevel)
            {
                return true;
            }

            // Calcul de la somme des niveaux de la classe
            var classLevels = levels.Where(l => l.Id == p.Value).Sum(l => (int?)l.Level) ?? 0;

            // C'est ok si la somme des niveaux est supérieure au niveau nécessaire par le prerequis
            return p.Number <= classLevels;
        }

        public static IEnumerable<ClassLevel> FromStringArray(string textArray)
        {
            return ArrayExtensions.FromString(textArray).Select(s => new ClassLevel(s));
        }
    }
}