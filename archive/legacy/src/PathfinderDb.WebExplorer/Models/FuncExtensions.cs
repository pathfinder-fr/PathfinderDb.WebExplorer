using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DbBrowser.Models
{
    internal static class FuncExtensions
    {
        public static Func<T, bool> AndAlso<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            if (predicate2 == null)
            {
                return predicate1;
            }

            return arg => predicate1(arg) && predicate2(arg);
        }

        public static Func<T, bool> OrElse<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) || predicate2(arg);
        }
    }
}