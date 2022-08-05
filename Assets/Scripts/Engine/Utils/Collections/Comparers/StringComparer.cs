namespace Engine
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     The string comparer.
    /// </summary>
    internal struct StringComparer : IEqualityComparer<string>
    {
        #region Public Methods and Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals (string x, string y)
        {
            return string.Equals(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode (string obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }


    /// <summary>
    ///     The string comparer.
    /// </summary>
    internal struct StringComparerOrdinalIgnoreCase : IEqualityComparer<string>
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether the specified strings are equal.
        /// </summary>
        /// <param name="x">
        ///     The first string of type <see cref="string" /> to compare.
        /// </param>
        /// <param name="y">
        ///     The second string of type <see cref="string" /> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified strings are reference equal, otherwise <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals (string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Returns the runtime hash code for the specified string.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="string" /> for which a hash code is to be returned.
        /// </param>
        /// <returns>
        ///     A runtime hash code for the specified string.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode (string obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}