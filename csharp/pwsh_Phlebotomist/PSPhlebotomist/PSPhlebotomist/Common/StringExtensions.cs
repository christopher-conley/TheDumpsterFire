using System;
using System.Collections.Generic;
using System.Text;

namespace PSPhlebotomist.Common
{
    public static class Lazy
    {
        /// <summary>
        /// Determines whether the specified string is not null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="input">The string to test for null, emptiness, or white-space characters.</param>
        /// <returns><see langword="true"/> if <paramref name="input"/> is not null, not empty, and contains at least one
        /// non-white-space character; otherwise, <see langword="false"/>.</returns>
        public static bool IsNotNullOrWhiteSpace(this string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        
        /// <summary>
        /// Determines whether the specified string is not null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="input">The string to test for null, emptiness, or white-space characters.</param>
        /// <returns><see langword="true"/> if <paramref name="input"/> is not null, not empty, and contains at least one
        /// non-white-space character; otherwise, <see langword="false"/>.</returns>
        public static bool NotNullOrWhiteSpace(this string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Determines whether the specified string is not null or an empty string ("").
        /// </summary>
        /// <param name="input">The string to test for null or empty.</param>
        /// <returns><see langword="true"/> if <paramref name="input"/> is not null or an empty string; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsNotNullOrEmpty(this string? input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Determines whether the specified string is not null or an empty string ("").
        /// </summary>
        /// <param name="input">The string to test for null or empty.</param>
        /// <returns>true if the input string is not null or empty; otherwise, false.</returns>
        public static bool NotNullOrEmpty(this string? input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="input">The string to test for null, emptiness, or white-space characters.</param>
        /// <returns>true if the input is null, empty, or consists exclusively of white-space characters; otherwise, false.</returns>
        public static bool IsNullOrWhiteSpace(this string? input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Determines whether the specified string is null or an empty string ("").
        /// </summary>
        /// <param name="input">The string to test for null or emptiness.</param>
        /// <returns><see langword="true"/> if the <paramref name="input"/> parameter is null or an empty string; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsNullOrEmpty(this string? input)
        {
            return string.IsNullOrEmpty(input);
        }
    }
}
