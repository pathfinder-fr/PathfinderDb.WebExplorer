namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.WebPages;

    public static class FeatExtensions
    {
        public static HelperResult RenderPrerequisites(this Feat feat, Func<FeatPrerequisite, MvcHtmlString> featPrerequisiteDecoration)
        {
            object[] prerequisites = feat.Prerequisites;
            return new HelperResult(w =>
            {
                for (int i = 0; i < prerequisites.Length; i++)
                {
                    if (i != 0)
                    {
                        w.Write(", ");
                    }
                    var prerequisite = prerequisites[i];
                    if (prerequisite is FeatPrerequisite)
                    {
                        RenderPrerequisite((FeatPrerequisite)prerequisite, w, featPrerequisiteDecoration);
                    }
                    else
                    {
                        var items = ((FeatPrerequisiteChoice)prerequisite).Items;
                        for (int j = 0; j < items.Length; j++)
                        {
                            if (j != 0)
                            {
                                w.Write(" ou ");
                            }
                            var choice = items[j];
                            RenderPrerequisite(choice, w, featPrerequisiteDecoration);

                        }
                    }
                }
            });
        }

        private static void RenderPrerequisite(FeatPrerequisite prerequisite, TextWriter writer, Func<FeatPrerequisite, MvcHtmlString> featPrerequisiteDecoration)
        {
            var prereq = prerequisite;
            var inner = prereq.Description;
            if (prereq.Type == FeatPrerequisiteType.Feat)
            {
                inner = featPrerequisiteDecoration(prereq).ToString();
            }

            writer.Write("<span>");
            writer.Write(inner);
            writer.Write("</span>");
        }
    }
}