namespace DbBrowser.Controllers
{
    using DbBrowser.Models;
    using PathfinderDb.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    public class FeatController : Controller
    {
        public ActionResult Index(FeatIndexQuery query)
        {
            query = query ?? new FeatIndexQuery();

            var db = this.DataSet();

            var model = new FeatIndexViewModel();
            model.Query = query;

            model.Feats = db.Feats;


            var prereqPredicates = new List<Func<FeatPrerequisite, bool>>();
            var prereqValidator = new FeatPrerequisiteValidator(prereqPredicates);
            Func<Feat, bool> predicate = prereqValidator.RunOn;

            var sources = query.AsArray(query.Sources);
            if (sources != null && sources.Length != 0)
            {
                predicate = predicate.AndAlso(f => sources.Any(s => s == f.Source.Id));
            }

            var types = query.AsArray(query.Types).Select(t => (FeatType)Enum.Parse(typeof(FeatType), t)).ToArray();
            if (!types.IsNullOrEmpty())
            {
                predicate = predicate.AndAlso(f => types.Any(t => f.Types.Any(ft => ft == t)));
            }

            if (query.BBA.HasValue())
                prereqPredicates.Add(query.BBA.Match(FeatPrerequisiteType.BBA));

            if (query.Str.HasValue())
                prereqPredicates.Add(query.Str.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Strength));

            if (query.Dex.HasValue())
                prereqPredicates.Add(query.Dex.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Dexterity));

            if (query.Con.HasValue())
                prereqPredicates.Add(query.Con.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Constitution));

            if (query.Int.HasValue())
                prereqPredicates.Add(query.Int.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Intelligence));

            if (query.Wis.HasValue())
                prereqPredicates.Add(query.Wis.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Wisdom));

            if (query.Cha.HasValue())
                prereqPredicates.Add(query.Cha.Match(FeatPrerequisiteType.Attribute, CharacterAttributes.Charisma));

            var classLevels = query.AsArray(query.ClassLevels).Select(a => new ClassLevel(a)).ToArray();
            if (!classLevels.IsNullOrEmpty())
            {
                prereqPredicates.Add(p => ClassLevel.MatchAll(classLevels, p));
            }

            if (!string.IsNullOrWhiteSpace(query.Feat))
                prereqPredicates.Add(p => p.Type != FeatPrerequisiteType.Feat || p.Value == query.Feat);

            if (predicate != null)
                model.Feats = model.Feats.Where(predicate);

            model.AllSources = SourceExtensions.AllSources;
            model.AllTypes = FeatTypeExtensions.AllFeatTypes;
            model.AllClasses = CharacterClassExtensions.AllCharacterClasses;
            model.AllRaces = RacesExtensions.AllRaces;

            return View(model);
        }

        class FeatPrerequisiteValidator
        {
            List<Func<FeatPrerequisite, bool>> prereqPredicates;

            public FeatPrerequisiteValidator(List<Func<FeatPrerequisite, bool>> prereqPredicates)
            {
                this.prereqPredicates = prereqPredicates;
            }

            public bool RunOn(Feat feat)
            {
                if (feat.Prerequisites == null || feat.Prerequisites.Length == 0)
                {
                    return true;
                }

                foreach (var item in feat.Prerequisites)
                {
                    var prereq = item as FeatPrerequisite;
                    if (prereq != null)
                    {
                        if (!this.ValidatePrereq(prereq))
                        {
                            // Si au moins un des prerequis n'est pas satisfait c'est une erreur
                            return false;
                        }
                    }
                    else
                    {
                        var choice = item as FeatPrerequisiteChoice;
                        if (choice != null)
                        {
                            var validated = false;
                            foreach (var subPrereq in choice.Items)
                            {
                                if (this.ValidatePrereq(subPrereq))
                                {
                                    // On a satisfait au moins un prerequis, on s'arrête
                                    validated = true;
                                    break;
                                }
                            }

                            if (!validated)
                            {
                                // Ca n'est pas satisfait si aucun des prerequis optionel n'est satisfait
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            private bool ValidatePrereq(FeatPrerequisite prereq)
            {
                foreach (var predicate in this.prereqPredicates)
                {
                    if (!predicate(prereq))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
