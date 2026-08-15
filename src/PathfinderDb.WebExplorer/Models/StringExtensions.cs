using System;
using System.Collections.Generic;
using System.Linq;

namespace DbBrowser.Models
{
    public static class StringExtensions
    {
        public static IEnumerable<ItemDisplay> AsItemDisplayArray(this string source)
        {
            return ArrayExtensions.FromString(source).Select(s => new ItemDisplay(s));
        }
    }
}