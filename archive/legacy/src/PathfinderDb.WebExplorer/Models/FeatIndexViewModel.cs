namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System.Collections.Generic;

    public class FeatIndexViewModel
    {
        public FeatIndexQuery Query { get; set; }

        public IEnumerable<Feat> Feats { get; set; }

        public Source[] AllSources { get; set; }

        public ItemDisplay[] AllTypes { get; set; }

        public ItemDisplay[] AllClasses { get; set; }

        public ItemDisplay[] AllRaces { get; set; }
    }
}