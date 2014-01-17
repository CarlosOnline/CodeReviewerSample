using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CodeReviewer.Extensions
{
    /// <summary>
    /// Provides a case-insensitive comparer for use in sorting.
    /// </summary>
    public class CaseInsensitiveComparer : IComparer<string>
    {
        /// <summary>
        /// Compares two strings in a case-insensitive manner.
        /// </summary>
        /// <param name="x">The first string.</param>
        /// <param name="y">The second string.</param>
        int IComparer<string>.Compare(string x, string y)
        {
            return string.Compare(x, y, true);
        }
    }

    /// <summary>
    /// Provides extension methods to simplify common string operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Performs a ordinal case-insensitive equality test.
        /// </summary>
        /// <param name="lhs"> The "this" parameter </param>
        /// <param name="prefix"> The string to test for equality. </param>
        /// <returns> true if strings are equal, false otherwise. </returns>
        public static bool EqualsIgnoreCase(this String lhs, String rhs)
        {
            return lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Performs a ordinal case-insensitive test to see if the target
        /// string starts with given string.
        /// </summary>
        /// <param name="lhs"> The "this" parameter </param>
        /// <param name="prefix"> The prefix string. </param>
        /// <returns> true if 'this' starts with 'prefix'; false otherwise. </returns>
        public static bool StartsWithIgnoreCase(this string lhs, string prefix)
        {
            return lhs.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Performs a ordinal case-insensitive test to see if the target
        /// string ends with given string.
        /// </summary>
        /// <param name="lhs"> The "this" parameter </param>
        /// <param name="postfix"> The postfix string. </param>
        /// <returns> true if 'this' ends with 'postfix'; false otherwise. </returns>
        public static bool EndsWithIgnoreCase(this string lhs, string postfix)
        {
            return lhs.EndsWith(postfix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Used to determine if a string is null or empty.
        /// </summary>
        /// <returns>
        /// Returns true if string reference null or is the empty string; returns false otherwise.
        /// </returns>
        public static bool IsNullOrEmpty(this string _this)
        {
            return string.IsNullOrEmpty(_this);
        }

        public static string ToLowerCultureInvariant(this string _this)
        {
            return _this.ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Provides extension methods to simplify common List operations.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Returns true if _this is null or has an element count of zero.
        /// </summary>
        public static bool IsEmpty<T>(this List<T> _this)
        {
            return _this.Count() == 0;
        }

        /// <summary>
        /// Returns true if _this is null or has an element count of zero.
        /// </summary>
        public static bool IsEmpty<T>(this IList<T> _this)
        {
            return _this.Count() == 0;
        }

        /// <summary>
        /// Returns true if _this is null or has an element count of zero.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> _this)
        {
            return _this == null || _this.IsEmpty();
        }

        /// <summary>
        /// Returns true if _this is null or has an element count of zero.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> _this)
        {
            return _this == null || _this.IsEmpty();
        }

        /// <summary>
        /// Returns the index of the first element for which predicate returns true;
        /// otherwise returns this.Count().
        /// </summary>
        public static int IndexOfFirst<T>(this IList<T> _this, Func<T, bool> predicate)
        {
            return _this.IndexOfFirst(predicate, 0);
        }

        /// <summary>
        /// Returns the index of the first element for which predicate returns true;
        /// otherwise returns this.Count().
        /// </summary>
        /// <param name="startIndex">The index at which to start the search.</param>
        public static int IndexOfFirst<T>(this IList<T> _this, Func<T, bool> predicate, int startIndex)
        {
            int i = startIndex;
            for (; i < _this.Count(); ++i)
                if (predicate(_this[i]))
                    return i;
            return i;
        }
    }
}
