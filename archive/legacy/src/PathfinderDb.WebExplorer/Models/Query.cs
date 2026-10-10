namespace DbBrowser.Models
{
    using System;
    using System.Linq;

    public abstract class Query
    {
        protected Query()
        {
        }

        public string IsEqualCssClass(string value, string itemValue)
        {
            if(string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            else if (string.Equals(value, itemValue, StringComparison.OrdinalIgnoreCase))
            {
                return "text-success";
            }
            else
            {
                return "text-error";
            }
        }

        public string IsPresentCssClass(string arrayText, string item)
        {
            var present = IsPresent(arrayText, item);
            if (present == null)
                return string.Empty;
            else if (present.Value)
                return "text-success";
            else
                return "text-error";
        }

        public bool? IsPresent(string arrayText, string item)
        {
            if (string.IsNullOrWhiteSpace(arrayText))
                return null;

            return AsArray(arrayText).Any(i => i.Equals(item, StringComparison.OrdinalIgnoreCase));
        }

        public string[] AsArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            return value.Split(new[] { ArrayExtensions.ArrayItemSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Bascule la valeur de l'élément d'un tableau stocké dans une chaîne, en supprimant l'élément s'il est présent ou en l'ajoutant s'il manque.
        /// </summary>
        protected static string ToggleArrayItem(string array, string item)
        {
            if (string.IsNullOrWhiteSpace(array))
            {
                return item;
            }
            else if (array.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1)
            {
                if (array.Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }

                if (array.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    return array.Substring(item.Length + 1);
                }

                return array.Replace(ArrayExtensions.ArrayItemSeparator + item, string.Empty);
            }
            else
            {
                return array + ArrayExtensions.ArrayItemSeparator + item;
            }
        }
    }
}