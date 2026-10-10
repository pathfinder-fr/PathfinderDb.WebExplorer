namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System;

    public static class SourceExtensions
    {
        public static readonly Source[] AllSources = new[] 
                {
                     new Source { Id = Source.Ids.PathfinderRpg, Name = "Manuel des joueurs", Url = new Uri("http://www.black-book-editions.fr/index.php?site_id=60")},
                     new Source { Id = Source.Ids.Bestiary, Name = "Bestiaire", Url = new Uri ("http://www.black-book-editions.fr/index.php?site_id=128")},
                     new Source { Id = Source.Ids.Bestiary2, Name = "Bestiaire 2", Url = new Uri ("http://www.black-book-editions.fr/index.php?site_id=193")},
                     new Source { Id = Source.Ids.AdvancedPlayerGuide, Name = "Manuel des joueurs Règles avancées", Url = new Uri ("http://www.black-book-editions.fr/index.php?site_id=182")},
                     new Source { Id = Source.Ids.UltimateMagic, Name = "L'art de la magie", Url = new Uri ("http://www.black-book-editions.fr/index.php?site_id=196")},
                     new Source { Id = Source.Ids.UltimateCombat, Name = "L'art de la guerre", Url = new Uri ("http://www.black-book-editions.fr/index.php?site_id=195")},
                     new Source { Id = Source.Ids.PaizoBlog, Name = "Blog Paizo", Url = new Uri("http://paizo.com/paizo/blog")},
                };

        public static string ShortName(this Source source)
        {
            switch (source.Id)
            {
                case Source.Ids.AdvancedPlayerGuide:
                    return "Règles avancées";

                default:
                    return source.Name;
            }
        }
    }
}