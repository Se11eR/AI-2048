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

        public int GetFreeCellsCount2()
        {
	        __Repr |= (__Repr >> 2) & 0x3333333333333333;
	        __Repr |= (__Repr >> 1);
            __Repr = ~__Repr & 0x1111111111111111;
	        // At this point each nibble is:
	        //  0 if the original nibble was non-zero
	        //  1 if the original nibble was zero
	        // Next sum them all
            __Repr += __Repr >> 32;
            __Repr += __Repr >> 16;
            __Repr += __Repr >> 8;
            __Repr += __Repr >> 4; // this can overflow to the next nibble if there were 16 empty positions
            return (int)__Repr & 0xf;
        }

        public long ExtractRow(int i)
        {
            return (long)(((ulong)__Repr & __RowsMask[i]) >> (i * 16));
        }

        public void SetRow(long row, int i)
        {
            __Repr = (~((long)__RowsMask[i]) & __Repr) | (row << (i * 16));
        }

        public void Transpose()
        {
            unchecked
            {
                var a1 = (ulong)__Repr & 0xF0F00F0FF0F00F0F;
	            var a2 = __Repr & 0x0000F0F00000F0F0;
	            var a3 = __Repr & 0x0F0F00000F0F0000;
                var a = a1 | (ulong)(a2 << 12) | (ulong)(a3 >> 12);
	            var b1 = a & 0xFF00FF0000FF00FF;
	            var b2 = a & 0x00FF00FF00000000;
	            var b3 = a & 0x00000000FF00FF00;
                __Repr = (long)(b1 | (b2 >> 24) | (b3 << 24));     
            }
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
