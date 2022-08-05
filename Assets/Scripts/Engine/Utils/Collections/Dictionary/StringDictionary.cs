namespace Engine
{
    using System.Collections.Generic;

    /// <summary>
    ///     The string dictionary.
    /// </summary>
    /// <typeparam name="T">
    ///     The value type.
    /// </typeparam>
    /// <typeparam name="TComparer">
    ///     The comparer type.
    /// </typeparam>
    internal class StringDictionary<T, TComparer> : FastDictionary<string, T, TComparer>
        where TComparer : struct, IEqualityComparer<string>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringDictionary{T}" /> class.
        /// </summary>
        internal StringDictionary()
        {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringDictionary{T}" /> class.
        /// </summary>
        internal StringDictionary(int initialBucketCount)
            : base(initialBucketCount)
        {}

        #endregion
    }

    internal sealed class StringDictionary<T> : StringDictionary<T, StringComparer>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringDictionary{T}" /> class.
        /// </summary>
        internal StringDictionary()
        {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringDictionary{T}" /> class.
        /// </summary>
        internal StringDictionary(int initialBucketCount)
            : base(initialBucketCount)
        {}

        #endregion
    }
}