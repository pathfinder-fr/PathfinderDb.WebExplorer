namespace DbBrowser.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using DbBrowser.Models;
    using PathfinderDb.Schema;

    public class SpellController : Controller
    {
        public ActionResult Index(SpellIndexQuery query)
        {
            var model = new SpellIndexViewModel();


            Func<Spell, bool> predicate = _ => true;

            predicate = predicate.AndAlso(query.CreateSourcePredicate());
            predicate = predicate.AndAlso(query.CreateListPredicate());

            model.Spells = this.DataSet().Spells.Where(predicate).OrderBy(s => s.Name).ToList();

            if (string.IsNullOrEmpty(query.List))
            {
                query.ViewMode = SpellIndexViewMode.Alphabetical;
            }
            
            if (!string.IsNullOrEmpty(query.List) && query.ViewMode == SpellIndexViewMode.ByLevel)
            {
                model.SpellGroups = model.Spells
                    .GroupBy(sg => sg.Levels.First(l => l.List == query.List).Level)
                    .OrderBy(g => g.Key);
            }

            model.Query = query;
            model.AllSources = SourceExtensions.AllSources;

            return this.View(model);
        }
    }
}