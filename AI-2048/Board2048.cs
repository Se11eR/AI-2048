using System;
using System.Collections.Generic;
using System.IO;

namespace AI_2048
{
    internal struct Board2048 : IEquatable<Board2048>
    {
        public const long CONST2 = 1;
        public const long CONST4 = 2;
        public const double CONTS2_PROB = 0.9;
        public const double CONTS4_PROB = 0.1;
        private static readonly ulong[] __RowsMask = 
        {
            0xFFFFL,
            0xFFFF0000L,
            0xFFFF00000000L,
            0xFFFF000000000000L
        };

        private static readonly ulong[] __ColsMask =
        {
            0x000F000F000F000F,
            0x00F000F000F000F0,
            0x0F000F000F000F00,
            0xF000F000F000F000
        };

        internal class Board2048EqualityComparer : IEqualityComparer<Board2048>
        {
            public bool Equals(Board2048 x, Board2048 y)
            {
                return x.__Repr == y.__Repr;
            }

            public int GetHashCode(Board2048 obj)
            {
                unchecked
                {
                    var hash = 23;
                    hash = hash * 31 + (int)obj.__Repr;
                    hash = hash * 31 + (int)(obj.__Repr << 32);

                    return hash;
                }
            }
        }
        
        private long __Repr;

        public Board2048(Board2048 b)
        {
            __Repr = b.__Repr;
        }

        public long this[int row, int col]
        {
            get
            {
                return Helper.GetChunk(__Repr, row * 4 + col);
            }
            set
            {
                __Repr = Helper.SetChunk(__Repr, value, row * 4 + col);
            }
        }

        public int GetFreeCellsCount()
        {
            var count = 0;
            var allCells = Size * Size;
            for (var i = 0; i < allCells; i++)
            {
                if (((__Repr >> (i * 4)) & 0xF) == 0)
                    count++;
            }

            return count;
        }

        public long ExtractRow(int i)
        {
            return (long)(((ulong)__Repr & __RowsMask[i]) >> (i * 16));
        }

        public long ExtractColumn(int i)
        {
            var tmp = (long)(((ulong)__Repr & __ColsMask[i]) >> (i * 4));
            return tmp & 0xF | ((tmp >> (( 4 - 1) * 4)) & 0xF0) 
                             | ((tmp >> (( 8 - 2) * 4)) & 0xF00)
                             | ((tmp >> ((12 - 3) * 4)) & 0xF000);
        }

        public void SetRow(long row, int i)
        {
            __Repr = (~((long)__RowsMask[i]) & __Repr) | (row << (i * 16));
        }

        public void SetColumn(long col, int i)
        {
            __Repr = (~((long)__ColsMask[i]) & __Repr)
                        | ((col & 0xF 
                        | (((col >> 4) & 0xF) << 16) 
                        | (((col >> 8) & 0xF) << 32)
                        | (((col >> 12) & 0xF) << 48)) << (i * 4));

        }

        public int Size
        {
            get
            {
                return 4;
            }
        }

        #region IEquatable

        public bool Equals(Board2048 other)
        {
            return __Repr == other.__Repr;
        }

        public override string ToString()
        {
            var s = new StringWriter();

            for (var row = 0; row < Size; row++)
            {
                for (var col = 0; col < Size; col++)
                {
                    var val = this[row, col];
                    var value = (ulong)(val > 0 ? (1 << (int)val) : 0);
                    s.Write("{0,6}", value);
                }

                s.WriteLine();
                s.WriteLine();
            }

            return s.ToString();
        }

        #endregion
    }
}
