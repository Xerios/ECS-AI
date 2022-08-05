using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine
{
    public class Flat2DArray : Flat2DArray<float>, IFlat2DArrayStackable
    {
        public Vector2Int Offset;

        public Flat2DArray(int width, int height) : base(width, height){ }
        public Flat2DArray(Vector2Int offset, int width, int height) : base(width, height)
        {
            Offset = offset;
        }

        public float GetValue (int x, int y)
        {
            var xx = x + Offset.x;
            var yy = y + Offset.y;

            if (xx < 0 || yy < 0) return 0.0f;
            var idx = GetIndex(xx, yy);
            if (xx >= Width || yy >= Height) return 0.0f;
            return _dataarray[idx];
        }

        public void BoxBlurMin ()
        {
            var newDataArray = new float[Length];

            for (int y = 1; y < Height - 1; y++) {
                for (int x = 1; x < Width - 1; x++) {
                    float initial = _dataarray[GetIndex(x, y)];
                    float sum = 0;

                    sum += _dataarray[GetIndex(x, y - 1)];
                    sum += _dataarray[GetIndex(x, y + 1)];
                    sum += _dataarray[GetIndex(x - 1, y)];
                    sum += _dataarray[GetIndex(x + 1, y)];
                    sum /= 4;
                    newDataArray[GetIndex(x, y)] = Mathf.Min(initial, sum);
                }
            }
            _dataarray = newDataArray;
        }

        public Vector2Int GetOffset ()
        {
            return Offset;
        }
    }

    public class Flat2DArray<T>: IEnumerable<float>
    {
        protected T[] _dataarray;

        /// <summary>
        /// Creates a new instance of Flat2DArray
        /// </summary>
        /// <param name="height">Number of rows</param>
        /// <param name="width">Number of columns</param>
        public Flat2DArray(int width, int height)
        {
            Length = width * height;
            _dataarray = new T[Length];
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates a new instance of Flat2DArray
        /// </summary>
        /// <param name="height">Number of rows</param>
        /// <param name="width">Number of columns</param>
        public Flat2DArray(BoundsInt bounds)
        {
            var ConvertedSize = new Vector3(bounds.size.x, 0, bounds.size.z).ToPoint2D(); // Converted

            Length = ConvertedSize.x * ConvertedSize.y;
            _dataarray = new T[Length];
            Width = ConvertedSize.x;
            Height = ConvertedSize.y;
        }

        /// <summary>
        /// Creates a new instance of Flat2DArray
        /// </summary>
        /// <param name="source">Source Flat2DArray</param>
        public Flat2DArray(Flat2DArray<T> source)
        {
            Length = source.Width * source.Height;
            _dataarray = new T[Length];
            Width = source.Width;
            Height = source.Height;
            Array.Copy(source._dataarray, this._dataarray, source._dataarray.LongLength);
        }

        /// <summary>
        /// Gets the number of columns
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the number of rows
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the number of rows
        /// </summary>
        public int Length { get; private set; }


        public T[] Data { get { return _dataarray; } }

        /// <summary>
        /// IEnumerable implementation.
        /// </summary>
        /// <returns>internal array enumerator</returns>
        public IEnumerator<float> GetEnumerator ()
        {
            return (IEnumerator<float>)_dataarray.GetEnumerator();
        }

        /// <summary>
        /// IEnumerable Implementation
        /// </summary>
        /// <returns>internal array enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _dataarray.GetEnumerator();
        }

        public void Clear ()
        {
            Array.Clear(_dataarray, 0, Length);
        }

        public int GetIndex (int x, int y) => Width * y + x;

        public delegate void ProcessPoint (int x, int y, T current);
        public delegate T ProcessAndChangePoint (int x, int y, T current);
        public delegate T ChangePoint (float distSq, T current);

        public void ForEach (ProcessAndChangePoint action)
        {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    _dataarray[GetIndex(x, y)] = action.Invoke(x, y, _dataarray[GetIndex(x, y)]);
                }
            }
        }

        public void ForEach (params ProcessAndChangePoint[] actions)
        {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    T current = _dataarray[GetIndex(x, y)];
                    for (int i = 0; i < actions.Length; i++) {
                        current = actions[i].Invoke(x, y, current);
                    }
                    _dataarray[GetIndex(x, y)] = current;
                }
            }
        }

        public void ForEach (ProcessPoint action)
        {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    action.Invoke(x, y, _dataarray[GetIndex(x, y)]);
                }
            }
        }

        public void ForEachStopable (Func<int, int, T, bool> action)
        {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    if (action.Invoke(x, y, _dataarray[GetIndex(x, y)])) break;
                }
            }
        }

        public void ForEachSpiral (ProcessPoint action)
        {
            int levl;
            int c = Height / 2;
            int x, y;

            x = y = c;

            for (levl = 1; c + levl <= Height; levl++) {
                for (; y <= c + levl && y < Height; y++) action.Invoke(x, y, _dataarray[GetIndex(x, y)]);

                // Since we always start from the center going towards right, top row (going left to right)
                // will always be the last remaining row to print
                if (x == 0 && y == Height) break;

                for (x++, y--; x <= c + levl && x < Height; x++) action.Invoke(x, y, _dataarray[GetIndex(x, y)]);
                for (x--, y--; y >= c - levl; y--) action.Invoke(x, y, _dataarray[GetIndex(x, y)]);
                for (x--, y++; x >= c - levl; x--) action.Invoke(x, y, _dataarray[GetIndex(x, y)]);
                x++;
                y++;
            }
        }

        public void ForEachSpiralInwards (ProcessPoint action)
        {
            for (int i = Width - 1, j = 0; i >= 0; i--, j++) {
                for (int k = j; k < i; k++) action.Invoke(j, k, _dataarray[GetIndex(j, k)]);
                for (int k = j; k < i; k++) action.Invoke(k, i, _dataarray[GetIndex(k, i)]);
                for (int k = i; k > j; k--) action.Invoke(i, k, _dataarray[GetIndex(i, k)]);
                for (int k = i; k > j; k--) action.Invoke(k, j, _dataarray[GetIndex(k, j)]);
            }
        }

        public void PointSize (Vector2Int pos, int size, ChangePoint action)
        {
            if (size == 1) {
                Point(pos, size, action);
            }else{
                Ellipse(pos, size, action);
            }
        }

        private void Point (Vector2Int pos, int size, ChangePoint action)
        {
            int idx = GetIndex(pos.x, pos.y);

            if (idx >= 0 && idx < Length) _dataarray[idx] = action.Invoke(0, _dataarray[idx]);
        }

        private void Ellipse (Vector2Int pos, int size, ChangePoint action)
        {
            int sizeSq = (size * size) - 2;

            for (int y = -size; y <= size; y++) {
                for (int x = -size; x <= size; x++) {
                    int newx = (pos.x + x);
                    int newy = (pos.y + y);

                    int xDist = pos.x - newx;
                    int yDist = pos.y - newy;
                    float dist = (xDist * xDist + yDist * yDist);

                    int idx = GetIndex(newx, newy);

                    if (dist < sizeSq && idx >= 0 && idx < Length) {
                        // Debug.DrawRay(new Vector2Int(newx, newy).ToVector3(), Vector3.up, Color.red, 100f);
                        _dataarray[idx] = action.Invoke(dist, _dataarray[idx]);
                    }
                }
            }
        }
    }
}