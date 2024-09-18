using System.Text;
using System.Text.RegularExpressions;

namespace Unity.Behavior
{
    internal static class StringExtensions
    {
        internal static string CleanUp(this string input)
        {
            input = Regex.Replace(input, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline);
            var charsToRemove = new[] { ":", "(", ")", "[", "]", "{", "}","-" };
            return input.RemoveChars(charsToRemove);
        }
        
        private static string RemoveChars(this string input, string[] charsToRemove)
        {
            var sb = new StringBuilder();
            sb.Append(input);
            foreach (var badChar in charsToRemove)
                sb = sb.Replace(badChar, string.Empty);
            return sb.ToString();
        }

        internal static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var firstChar = char.ToUpper(input[0]);
            var remainingChars = input.Substring(1);
            return firstChar + remainingChars;
        }
    }
}