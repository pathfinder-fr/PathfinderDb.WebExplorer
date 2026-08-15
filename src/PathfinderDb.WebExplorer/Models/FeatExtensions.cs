namespace DbBrowser.Models
{
    using PathfinderDb.Schema;
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Html;

    public static class FeatExtensions
    {
        public static IHtmlContent RenderPrerequisites(this Feat feat, Func<FeatPrerequisite, IHtmlContent> featPrerequisiteDecoration)
        {
            object[] prerequisites = feat.Prerequisites;
            return new HtmlContentBuilder().AppendHtml(new Microsoft.AspNetCore.Html.HtmlString(RenderPrerequisitesToString(prerequisites, featPrerequisiteDecoration)));
        }

        private static string RenderPrerequisitesToString(object[] prerequisites, Func<FeatPrerequisite, IHtmlContent> featPrerequisiteDecoration)
        {
            using (var w = new StringWriter())
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

                return w.ToString();
            }
        }

        private static void RenderPrerequisite(FeatPrerequisite prerequisite, TextWriter writer, Func<FeatPrerequisite, IHtmlContent> featPrerequisiteDecoration)
        {
            var prereq = prerequisite;
            var inner = prereq.Description;
            if (prereq.Type == FeatPrerequisiteType.Feat)
            {
                var htmlContent = featPrerequisiteDecoration(prereq);
                using (var sw = new StringWriter())
                {
                    htmlContent.WriteTo(sw, System.Text.Encodings.Web.HtmlEncoder.Default);
                    inner = sw.ToString();
                }
            }

            writer.Write("<span>");
            writer.Write(inner);
            writer.Write("</span>");
        }
    }
}