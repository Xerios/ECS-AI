namespace Engine
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     The default equality comparer.
    /// </summary>
    internal struct EqualityComparer<T> : IEqualityComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals (T x, T y)
        {
            return x.Equals(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode (T obj)
        {
            return obj.GetHashCode();
        }
    }
}