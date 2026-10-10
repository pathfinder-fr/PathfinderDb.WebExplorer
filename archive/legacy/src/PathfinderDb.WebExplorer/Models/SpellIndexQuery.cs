namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Routing;

    public class SpellIndexQuery : Query
    {
        public string Sources { get; set; }

        public string Levels { get; set; }

        public string List { get; set; }

        [DefaultValue(typeof(SpellIndexViewMode), "Alphabetical")]
        public SpellIndexViewMode ViewMode { get; set; }

        public Func<Spell, bool> CreateSourcePredicate()
        {
            var sources = this.AsArray(this.Sources);
            if (sources != null && sources.Length != 0)
            {
                return s => sources.Any(so => s.Source.Id == so);
            }

            return null;
        }

        public Func<Spell, bool> CreateListPredicate()
        {
            var list = this.List;
            if (!string.IsNullOrEmpty(list))
            {
                return s => s.Levels.Any(sl => sl.List == list);
            }

            return null;

        }

        public string ViewModeState(SpellIndexViewMode viewMode, string trueString, string falseString = "")
        {
            return viewMode == this.ViewMode ? trueString : falseString;
        }

        public SpellIndexQuery WithViewMode(SpellIndexViewMode viewMode)
        {
            this.ViewMode = viewMode;
            return this;
        }

        /// <summary>
        /// Transforme cette requête pour qu'elle puisse être utilisée comme RouteValueDictionary dans un lien.
        /// </summary>
        /// <returns></returns>
        public RouteValueDictionary AsRouteValue()
        {
            var result = new RouteValueDictionary {
                { "Sources", this.Sources },
                { "List", this.List },
                { "ViewMode", this.ViewMode == SpellIndexViewMode.Alphabetical ? (SpellIndexViewMode?)null : this.ViewMode },
            };

            return result;
        }

        public SpellIndexQuery Clone()
        {
            var clone = (SpellIndexQuery)this.MemberwiseClone();
            return clone;
        }

        public SpellIndexQuery ClearList()
        {
            this.List = null;
            return this;
        }

        public SpellIndexQuery ClearSources()
        {
            this.Sources = null;
            return this;
        }

        public SpellIndexQuery ToggleSource(string sourceId)
        {
            this.Sources = ToggleArrayItem(this.Sources, sourceId);
            return this;
        }

        public SpellIndexQuery SetList(string list)
        {
            this.List = list;
            return this;
        }
    }
}