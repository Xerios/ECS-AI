namespace UtilityAI
{
    // <copyright file="DynamicBufferExtensions.cs" company="BovineLabs">
    //     Copyright (c) BovineLabs. All rights reserved.
    // </copyright>

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    /// <summary>
    /// The DynamicBufferExtensions.
    /// </summary>
    public static class DynamicBufferExtensions
    {
        public static bool Contains<T, TI>(this DynamicBuffer<T> buffer, TI item)
            where T : struct, IEquatable<TI>
            where TI : struct
        {
            return buffer.IndexOf(item) >= 0;
        }

        public static int IndexOf<T, TI>(this DynamicBuffer<T> buffer, TI item)
            where T : struct, IEquatable<TI>
            where TI : struct
        {
            var length = buffer.Length;

            for (int index = 0; index < length; ++index) {
                if (buffer[index].Equals(item)) {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Remove an element from a <see cref="DynamicBuffer{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of NativeList</typeparam>
        /// <typeparam name="TI">The type of element.</typeparam>
        /// <param name="buffer">The DynamicBuffer.</param>
        /// <param name="element">The element.</param>
        /// <returns>True if removed, else false.</returns>
        public static bool Remove<T, TI>(this DynamicBuffer<T> buffer, TI element)
            where T : struct, IEquatable<TI>
            where TI : struct
        {
            var index = buffer.IndexOf(element);

            if (index < 0) {
                return false;
            }

            buffer.RemoveAt(index);
            return true;
        }
    }
}