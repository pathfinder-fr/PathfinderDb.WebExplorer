namespace DbBrowser.Models
{
    using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Routing;

    public class FeatIndexQuery : Query
    {
        public FeatIndexQuery()
        {
            this.BBA = new NumberInterval();
            this.Str = new NumberInterval();
            this.Dex = new NumberInterval();
            this.Con = new NumberInterval();
            this.Int = new NumberInterval();
            this.Wis = new NumberInterval();
            this.Cha = new NumberInterval();
        }

        public string ViewMode { get; set; }

        public string Sources { get; set; }

        public string Types { get; set; }

        public string Feat { get; set; }

        public string ClassLevels { get; set; }

        public string Races { get; set; }

        public NumberInterval BBA { get; private set; }

        public NumberInterval Str { get; private set; }

        public NumberInterval Dex { get; private set; }

        public NumberInterval Con { get; private set; }

        public NumberInterval Int { get; private set; }

        public NumberInterval Wis { get; private set; }

        public NumberInterval Cha { get; private set; }

        public string ViewModeState(string viewName, string trueString, string falseString = "")
        {
            var viewMode = (this.ViewMode ?? string.Empty);
            viewName = (viewName ?? string.Empty);

            return string.Equals(viewMode, viewName, StringComparison.OrdinalIgnoreCase) ? trueString : falseString;
        }

        public FeatIndexQuery WithViewMode(string viewMode)
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
                { "Types", this.Types },
                { "ViewMode", this.ViewMode },
                { "ClassLevels", this.ClassLevels },
                { "Races", this.Races },
            };

            this.BBA.AddTo(result, "BBA");
            this.Str.AddTo(result, "Str");
            this.Dex.AddTo(result, "Dex");
            this.Con.AddTo(result, "Con");
            this.Int.AddTo(result, "Int");
            this.Wis.AddTo(result, "Wis");
            this.Cha.AddTo(result, "Cha");

            return result;
        }

        public FeatIndexQuery ClearTypes()
        {
            this.Types = null;
            return this;
        }

        public FeatIndexQuery ClearSources()
        {
            this.Sources = null;
            return this;
        }

        public FeatIndexQuery ClearRaces()
        {
            this.Races = null;
            return this;
        }

        public FeatIndexQuery ClearClassLevels()
        {
            this.ClassLevels = null;
            return this;
        }

        public FeatIndexQuery Clone()
        {
            var clone = (FeatIndexQuery)this.MemberwiseClone();
            clone.BBA = this.BBA.Clone();
            clone.Str = this.Str.Clone();
            clone.Dex = this.Dex.Clone();
            clone.Con = this.Con.Clone();
            clone.Int = this.Int.Clone();
            clone.Wis = this.Wis.Clone();
            clone.Cha = this.Cha.Clone();
            return clone;
        }

        public FeatIndexQuery ToggleSource(string sourceId)
        {
            this.Sources = ToggleArrayItem(this.Sources, sourceId);
            return this;
        }

        public FeatIndexQuery ToggleClassLevel(string classLevel)
        {
            this.ClassLevels = ToggleArrayItem(this.ClassLevels, classLevel);
            return this;
        }

        public FeatIndexQuery ToggleType(string type)
        {
            this.Types = ToggleArrayItem(this.Types, type);
            return this;
        }
    }
}