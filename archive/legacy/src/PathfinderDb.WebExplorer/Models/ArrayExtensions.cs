
namespace DbBrowser.Models
{
    public static class ArrayExtensions
    {
        public const char ArrayItemSeparator = '~';

        public const char ArrayItemValueSeparator = '.';

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string[] FromString(string textArray)
        {
            if (textArray == null)
                return new string[0];

            return textArray.Split(ArrayItemSeparator);
        }
        
    }
}