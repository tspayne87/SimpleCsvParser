using System;
using System.Text;

namespace SimpleCsvParser
{
    /// <summary>
    /// Helper class found at https://stackoverflow.com/questions/17580150/net-stringbuilder-check-if-ends-with-string
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Checks to see if the current string builder ends with.
        /// </summary>
        /// <param name="sb">The current string builder that needs to be checked.</param>
        /// <param name="test">The test string we want to check against.</param>
        /// <returns>Will return if the string ends with the test.</returns>
        public static bool EndsWith(this StringBuilder sb, string test)
        {
            return EndsWith(sb, test, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Checks to see if the current string builder ends with.
        /// </summary>
        /// <param name="sb">The current string builder that needs to be checked.</param>
        /// <param name="test">The test string we want to check against.</param>
        /// <param name="comparison">Current string comparison type to use.</param>
        /// <returns>Will return if the string ends with the test.</returns>
        public static bool EndsWith(this StringBuilder sb, string test, StringComparison comparison)
        {
            if (sb.Length < test.Length)
                return false;

            string end = sb.ToString(sb.Length - test.Length, test.Length);
            return end.Equals(test, comparison);
        }
    }
}