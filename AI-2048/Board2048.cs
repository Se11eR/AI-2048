using System;
using System.CodeDom;
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
        public const int SIZE = 4;

        private static readonly ulong[] __ChunksMask = 
        {
            0xFFFFL,
            0xFFFF0000L,
            0xFFFF00000000L,
            0xFFFF000000000000L
        };

        internal class Board2048EqualityComparer : IEqualityComparer<Board2048>
        {
            public bool Equals(Board2048 x, Board2048 y)
            {
                return x.Repr == y.Repr;
            }

            public int GetHashCode(Board2048 obj)
            {
                unchecked
                {
                    var hash = 23;
                    hash = hash * 31 + (int)obj.Repr;
                    hash = hash * 31 + (int)(obj.Repr << 32);

                    return hash;
                }
            }
        }
        
        public long Repr;

        public Board2048(Board2048 b)
        {
            Repr = b.Repr;
        }

        public long this[int row, int col]
        {
            get
            {
                return (Repr >> ((row * 4 + col) * 4)) & (0xFL);
            }
            set
            {
                var i = row * 4 + col;
                Repr = (~(0xFL << (i * 4)) & Repr) | (value << (i * 4));
            }
        }

        public int GetFreeCellsCount()
        {
            var count = 0;
            var allCells = SIZE * SIZE;
            for (var i = 0; i < allCells; i++)
            {
                if (((Repr >> (i * 4)) & 0xF) == 0)
                    count++;
            }

            return count;
        }

        public int GetFreeCellsCount2()
        {
	        Repr |= (Repr >> 2) & 0x3333333333333333;
	        Repr |= (Repr >> 1);
            Repr = ~Repr & 0x1111111111111111;
	        // At this point each nibble is:
	        //  0 if the original nibble was non-zero
	        //  1 if the original nibble was zero
	        // Next sum them all
            Repr += Repr >> 32;
            Repr += Repr >> 16;
            Repr += Repr >> 8;
            Repr += Repr >> 4; // this can overflow to the next nibble if there were 16 empty positions
            return (int)Repr & 0xf;
        }

        public long ExtractChunkBlock(int i)
        {
            return (long)(((ulong)Repr & __ChunksMask[i]) >> (i * 16));
        }

        public void SetChunkBlock(long block, int i)
        {
            Repr = (~((long)__ChunksMask[i]) & Repr) | (block << (i * 16));
        }

        public void Transpose()
        {
            unchecked
            {
                var a1 = (ulong)Repr & 0xF0F00F0FF0F00F0F;
	            var a2 = Repr & 0x0000F0F00000F0F0;
	            var a3 = Repr & 0x0F0F00000F0F0000;
                var a = a1 | (ulong)(a2 << 12) | (ulong)(a3 >> 12);
	            var b1 = a & 0xFF00FF0000FF00FF;
	            var b2 = a & 0x00FF00FF00000000;
	            var b3 = a & 0x00000000FF00FF00;
                Repr = (long)(b1 | (b2 >> 24) | (b3 << 24));     
            }
        }

        #region IEquatable

        public bool Equals(Board2048 other)
        {
            return Repr == other.Repr;
        }

        public override string ToString()
        {
            var s = new StringWriter();

            for (var row = 0; row < SIZE; row++)
            {
                for (var col = 0; col < SIZE; col++)
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
