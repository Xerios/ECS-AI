using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class ArrayUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(ref T[] array, params T[] array2)
        {
            int originalLength = array.Length;

            Array.Resize(ref array, originalLength + array2.Length);
            Array.Copy(array2, 0, array, originalLength, array2.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt<T>(ref T[] array, int index)
        {
            array = array.Where((val, i) => i != index).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this T[] list, int indexA, int indexB)
        {
            T tmp = list[indexA];

            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }


        public static bool EqualsShallow<T>(this T[] a1, T[] a2)
        {
            if (a1.Length == a2.Length) {
                for (int i = 0; i < a1.Length; i++) {
                    if (!a1[i].Equals(a2[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}