using System.Collections;
using UnityEngine;

namespace Engine {
    public static class Vector2IntExtensions
    {
        public const int cellsize = 5;

        public static Vector3 ToWorld (this Vector2Int vec)
        {
            return new Vector3((vec.x * cellsize), 0, -(vec.y * cellsize) - cellsize * .5f);
        }

        public static Vector3 ToWorldNonCentered (this Vector2Int vec)
        {
            return new Vector3Int((vec.x * cellsize), 0, -(vec.y * cellsize));
        }

        public static Vector2Int ToPoint2D (this Vector3 vec)
        {
            return new Vector2Int(Mathf.RoundToInt(vec.x / cellsize), -Mathf.RoundToInt(vec.z / cellsize));
        }

        public static Vector2Int ToPoint2D (this Vector3Int vec)
        {
            return new Vector2Int(Mathf.RoundToInt((float)vec.x / cellsize), -Mathf.RoundToInt((float)vec.z / cellsize));
        }

        public static Vector2Int ToPoint2DNoZFlip (this Vector3Int vec)
        {
            return new Vector2Int(Mathf.RoundToInt((float)vec.x / cellsize), Mathf.RoundToInt((float)vec.z / cellsize));
        }

        public static int ToPoint (this float value)
        {
            return Mathf.RoundToInt(value / cellsize);
        }

        public static int ToPoint (this int value)
        {
            return Mathf.RoundToInt((float)value / cellsize);
        }
    }
}