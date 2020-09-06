using System.Text.RegularExpressions;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Provides additional helper methods for dealing with strings.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Strips all new-line characters from a string.
        /// </summary>
        /// <param name="text">The string to strip.</param>
        /// <returns>The same string, but with 100% less hope of being readable!</returns>
        public static string StripNewLines(this string text)
        {
            return Regex.Replace(text, "/[\\r\\n\\v]/", "");
        }
    }
}