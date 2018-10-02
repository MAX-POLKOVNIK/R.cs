using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace R.cs.Core
{
    internal static class ValidNamesProvider
    {
        private static readonly char Separator = Mono.IsMono() ? '/' : '\\';
        private static readonly char[] SymbolsToRemove = { '-', '.', '(', ')' };

        public static string GetCorrectConstName(string original)
        {
            if (Regex.IsMatch(original, @"^\d"))
            {
                original = $"_{original}";
            }

            return SymbolsToRemove.Aggregate(original, (current, c) => current.Replace(c, '_'));
        }

        public static string GetCorrectResourceBundleName(string original)
        {
            if (original.StartsWith($"Resources{Separator}"))
            {
                original = original.Replace($"Resources{Separator}", "");
            }

            original = new[] { "@1x", "@2x", "@3x" }.Aggregate(original, (current, s1) => current.Replace(s1, ""));

            original = original.Replace(@"\", "/");
            
            var extension = Path.GetExtension(original);
            var newExtension = extension == ".png" || extension == ".otf" || extension == ".ttf" || extension == "" ? null : extension;

            return Path.ChangeExtension(original, newExtension);
        }
    }
}
