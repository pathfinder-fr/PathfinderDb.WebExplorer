namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System;
    using System.Web.Routing;

    public class NumberInterval
    {
        public int? Min { get; set; }

        public int? Max { get; set; }

        public bool HasValue()
        {
            return this.Min.HasValue || this.Max.HasValue;
        }

        public NumberInterval Clone()
        {
            return (NumberInterval)this.MemberwiseClone();
        }

        public void AddTo(RouteValueDictionary routeValues, string prefix)
        {
            routeValues.Add(prefix + ".Min", this.Min);
            routeValues.Add(prefix + ".Max", this.Max);
        }

        public Func<FeatPrerequisite, bool> Match(FeatPrerequisiteType type)
        {
            return p => p.Type != type || Match(p);
        }

        public Func<FeatPrerequisite, bool> Match(FeatPrerequisiteType type, string value)
        {
            return p => p.Type != type || p.Value != value || Match(p);
        }

        public bool Match(FeatPrerequisite p)
        {
            return (!this.Min.HasValue || p.Number > this.Min.Value) && (!this.Max.HasValue || p.Number <= this.Max.Value);
        }
    }
}